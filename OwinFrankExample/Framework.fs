module Framework
open System
open System.IO
open System.Collections.Generic
open System.Threading.Tasks
open System.Text
open System.Reflection
open Owin

let getMethods app verb =
    app.GetType().GetMethods()
    |> Seq.where (fun x -> x.Name.StartsWith(verb + " /"))
    |> Seq.map (fun x -> (x.Name.Replace(verb + " ", ""), x))

let getMethod app verb requestPath =
    let (_,response) =
        getMethods app verb
        |> Seq.where (fun (path, _) -> path = requestPath)
        |> Seq.head
    response

let getParameters (queryString:string) =
    queryString.Split '&'
    |> Seq.map (fun x -> ((x.Split '=').[0], (x.Split '=').[1]))

let applyparameters app (methodInfo:MethodInfo) (parameters:seq<string * string>) =
    let paras =
        methodInfo.GetParameters()
        |> Seq.map (fun x -> parameters |> Seq.find(fun (name,_)-> name=x.Name))
        |> Seq.map (fun (_,value) -> value :> obj)
        |> Seq.toArray
    methodInfo.Invoke(app,paras).ToString()

let myFramework app (enviroment:IDictionary<string,Object>) =
    let response =
        getMethod app (enviroment.["owin.RequestMethod"] :?> string) (enviroment.["owin.RequestPath"] :?> string)
        |> applyparameters app 
    let responseBytes = ASCIIEncoding.UTF8.GetBytes(response(getParameters(enviroment.["owin.RequestQueryString"] :?> string)))
    let responseStream = enviroment.["owin.ResponseBody"] :?> Stream
    let responseHeaders = enviroment.["owin.ResponseHeaders"] :?> IDictionary<string, string[]>
    responseHeaders.Add("Content-Length", [|responseBytes.Length.ToString()|])
    responseHeaders.Add("Content-Type", [|"text/plain"|])
    responseStream.AsyncWrite(responseBytes, 0, responseBytes.Length) |> Async.StartAsTask :> Task