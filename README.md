Bar
===

Prototype of an FSharp Nano-framework based on Sinatra

It only has 2 dependancies (Owin, and Newtonsoft.Json), and Newtonsoft.Json is only used for deserializing query string members

Hello World looks like this:

    type MyApp() =
        member x.``GET /`` () =
            "Hello World"
            
Does support different verbs but there is no way to actually get posted data (it is only a prototype):

    type MyApp() =
        member x.``POST /`` () =
            "Thanks!"
            
Types parameters from the query string too:

    type MyApp() =
        member x.``GET /square`` number =
            number * number
            
By default does no content negotiation whatesoever, although there is a dumb component that can be used to just return plain text.
