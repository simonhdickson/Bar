module Components
open System
open System.Collections.Generic
open System.IO
open System.Reflection
open System.Text
open System.Threading.Tasks
open Newtonsoft.Json
open Owin
open Bar

let plainResponse next (enviroment:IDictionary<string,Object>) =
    if enviroment.ContainsKey("bar.RawResponse") then
        let response = enviroment.["bar.RawResponse"]
        let responseBytes = ASCIIEncoding.UTF8.GetBytes (response.ToString())
        let responseStream = enviroment.["owin.ResponseBody"] :?> Stream
        let responseHeaders = enviroment.["owin.ResponseHeaders"] :?> IDictionary<string, string[]>
        responseHeaders.Add("Content-Length", [|responseBytes.Length.ToString()|])
        responseHeaders.Add("Content-Type", [|"text/plain"|])
        responseStream.AsyncWrite(responseBytes, 0, responseBytes.Length) |> Async.StartAsTask :> Task
    else
        Task.Run (fun () -> next enviroment)

type IAppBuilder with
    member x.PlainResponse () =
        x.Use(fun next -> plainResponse (Func2 next))
    
let simpleTypeConverter input =
    match input with
    | (value, valueType) when valueType = typeof<string> -> value :> obj
    | (value, valueType) -> JsonConvert.DeserializeObject(value, valueType)
