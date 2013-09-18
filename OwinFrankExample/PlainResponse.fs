module PlainResponse
open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Threading.Tasks
open Owin

let plainResponse (enviroment:IDictionary<string,Object>) =
    let response = enviroment.["owin.RawResponse"]
    let responseBytes = ASCIIEncoding.UTF8.GetBytes (response.ToString())
    let responseStream = enviroment.["owin.ResponseBody"] :?> Stream
    let responseHeaders = enviroment.["owin.ResponseHeaders"] :?> IDictionary<string, string[]>
    responseHeaders.Add("Content-Length", [|responseBytes.Length.ToString()|])
    responseHeaders.Add("Content-Type", [|"text/plain"|])
    responseStream.AsyncWrite(responseBytes, 0, responseBytes.Length) |> Async.StartAsTask :> Task