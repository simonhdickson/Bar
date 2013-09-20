module Bar
open System
open System.IO
open System.Collections.Generic
open System.Reflection
open System.Threading.Tasks
open Owin

let (<<) f g x = f(g x)

let getMethods verb (methods:seq<MethodInfo>) =
    methods
    |> Seq.where (fun x -> x.Name.StartsWith(verb + " /"))
    |> Seq.map (fun x -> (x.Name.Replace(verb + " ", ""), x))

let getMethod requestPath methods =
    methods
    |> Seq.where (fun (path, _) -> path = requestPath)
    |> Seq.map (fun (_,response) -> response)
    |> Seq.head

let parseQueryString (queryString:string) =
    queryString.Split '&'
    |> Seq.map (fun x -> ((x.Split '=').[0], (x.Split '=').[1]))

let getParameter parameters (x:ParameterInfo) =
    parameters
    |> Seq.find (fun (name,_)-> name = x.Name)
    |> (fun (_,z) -> (z, x.ParameterType))

let invokeMethod instance parseParameter (methodInfo:MethodInfo) =
    let parameters =
        methodInfo.GetParameters()
        |> Seq.map parseParameter
        |> Seq.toArray
    methodInfo.Invoke(instance, parameters)

let useBar instance next (converter:string*Type->obj) (enviroment:IDictionary<string,obj>) =
    let requestMethod = enviroment.["owin.RequestMethod"] :?> string
    let requestPath = enviroment.["owin.RequestPath"] :?> string
    let queryString = enviroment.["owin.RequestQueryString"] :?> string
    let requestBody = (new StreamReader (enviroment.["owin.RequestBody"] :?> Stream)).ReadToEnd()
    let parseParameter =
        parseQueryString queryString
        |> Seq.append [("body", requestBody)]
        |> getParameter
        |> (fun x -> converter << x)
    let response =
        instance.GetType().GetMethods()
        |> getMethods requestMethod
        |> getMethod requestPath
        |> invokeMethod instance parseParameter
    enviroment.Add("bar.RawResponse", response)
    Task.Run (fun () -> next enviroment)