module Framework
open System
open System.Collections.Generic
open System.Reflection
open System.Threading.Tasks
open Newtonsoft.Json
open Owin

let getMethods app verb =
    app.GetType().GetMethods()
    |> Seq.where (fun x -> x.Name.StartsWith(verb + " /"))
    |> Seq.map (fun x -> (x.Name.Replace(verb + " ", ""), x))

let getMethod app verb requestPath =
    getMethods app verb
    |> Seq.where (fun (path, _) -> path = requestPath)
    |> Seq.map (fun (_,response) -> response)
    |> Seq.head

let getParameters (queryString:string) =
    queryString.Split '&'
    |> Seq.map (fun x -> ((x.Split '=').[0], (x.Split '=').[1]))

let applyparameters app (methodInfo:MethodInfo) (parameters:seq<string * string>) =
    let paras =
        methodInfo.GetParameters()
        |> Seq.map (fun x -> parameters
                             |> Seq.find (fun (name,_)-> name = x.Name)
                             |> (fun (y,z) -> (y,z,x.ParameterType)))
        |> Seq.map (fun (_, value, valueType) -> JsonConvert.DeserializeObject(value, valueType))
        |> Seq.toArray
    methodInfo.Invoke(app, paras)

let myFramework app next (enviroment:IDictionary<string,Object>) =
    let response =
        getMethod app (enviroment.["owin.RequestMethod"] :?> string) (enviroment.["owin.RequestPath"] :?> string)
        |> applyparameters app
    enviroment.Add("owin.RawResponse", response(getParameters(enviroment.["owin.RequestQueryString"] :?> string)))
    Task.Run (fun () -> next enviroment)