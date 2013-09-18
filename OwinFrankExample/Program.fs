namespace OwinFrankExample
open System
open Owin
open Microsoft.Owin.Hosting

module Program =
    type MyApp() =
        member x.``GET /`` () =
            "Hello World"
        member x.``POST /`` () =
            "Thanks!"
        member x.``GET /talk`` name =
            "Hello " + name
        member x.``POST /talk`` name =
            "Thanks " + name + "!"

    type Startup() =
        member x.Configuration(app: IAppBuilder) =
            app.Use(fun next -> Framework.myFramework(MyApp()))
            |> ignore

    [<EntryPoint>]
    let main argv =
        let url =
            match argv.Length with                 
                | i when i > 1 -> "http://*:" + argv.[0]
                | _ -> "http://localhost:5000"
        use disposable = WebApp.Start<Startup>(url)
        Console.WriteLine("Server running on " + url)
        Console.WriteLine("Press Enter to stop.")
        Console.ReadLine() |> ignore
        0