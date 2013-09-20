namespace BarExample
open System
open Microsoft.Owin.Hosting
open Bar
open Owin

module Program =
    type MyApp() =
        member x.``GET /`` () =
            "Hello World"
        member x.``POST /`` () =
            "Thanks!"
        member x.``POST /cheese`` name body =
            "Thanks, " + body + " is tasty " + name + "!"
        member x.``GET /talk`` name =
            "Hello " + name
        member x.``GET /square`` (number:decimal) =
            number * number

    type Startup() =
        member x.Configuration(app: IAppBuilder) =
            app.UseBar(MyApp(), Components.simpleTypeConverter)
               .Use(fun next -> Components.plainResponse)
            |> ignore

    [<EntryPoint>]
    let main argv =
        let url =
            match argv.Length with                 
                | i when i >= 1 -> "http://*:" + argv.[0]
                | _ -> "http://localhost:5000"
        use disposable = WebApp.Start<Startup>(url)
        Console.WriteLine("Server running on " + url)
        Console.WriteLine("Press Enter to stop.")
        Console.ReadLine() |> ignore
        0