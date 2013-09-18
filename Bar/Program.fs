namespace BarExample
open System
open Microsoft.Owin.Hosting
open Owin

module Program =
    type MyApp() =
        member x.``GET /`` () =
            "Hello World"
        member x.``POST /`` () =
            "Thanks!"
        member x.``GET /talk`` name =
            "Hello " + name
        member x.``GET /square`` number =
            number * number
        member x.``POST /talk`` name =
            "Thanks " + name + "!"

    let Func2 (x:Func<'a,'b>) y = x.Invoke(y)

    type Startup() =
        member x.Configuration(app: IAppBuilder) =
            app.Use(fun next -> Bar.useBar (MyApp()) (Func2 next))
               .Use(fun next -> PlainResponse.plainResponse)
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