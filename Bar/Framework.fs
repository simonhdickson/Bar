module Bar
open System
open System.Collections.Generic
open System.Reflection
open System.Threading.Tasks
open Newtonsoft.Json
open Owin

let (<<) f g x = f(g x)

let getMethods (appType:Type) verb =
    appType.GetMethods()
    |> Seq.where (fun x -> x.Name.StartsWith(verb + " /"))
    |> Seq.map (fun x -> (x.Name.Replace(verb + " ", ""), x))

let getMethod requestPath methods =
    methods
    |> Seq.where (fun (path, _) -> path = requestPath)
    |> Seq.map (fun (_,response) -> response)
    |> Seq.head

let parseParameters (queryString:string) =
    queryString.Split '&'
    |> Seq.map (fun x -> ((x.Split '=').[0], (x.Split '=').[1]))

let parseParameter parameters (x:ParameterInfo) =
    parameters
    |> Seq.find (fun (name,_)-> name = x.Name)
    |> (fun (_,z) -> (z, x.ParameterType))

let typeConverter input =
    match input with
    | (value, valueType) when valueType = typeof<string> -> value :> obj
    | (value, valueType) -> JsonConvert.DeserializeObject(value, valueType)

let applyParameters instance getParameter (methodInfo:MethodInfo) =
    let paras =
        methodInfo.GetParameters()
        |> Seq.map getParameter
        |> Seq.toArray
    methodInfo.Invoke(instance, paras)

let useBar instance next (enviroment:IDictionary<string,obj>) =
    let appType = instance.GetType()
    let requestMethod = enviroment.["owin.RequestMethod"] :?> string
    let requestPath = enviroment.["owin.RequestPath"] :?> string
    let queryString = enviroment.["owin.RequestQueryString"] :?> string
    let getParameter =
        parseParameters queryString
        |> parseParameter
        |> (fun x -> typeConverter << x)
    let response =
        getMethods appType requestMethod
        |> getMethod requestPath
        |> applyParameters instance getParameter
    enviroment.Add("bar.RawResponse", response)
    Task.Run (fun () -> next enviroment)