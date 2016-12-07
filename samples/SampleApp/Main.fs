module SampleApp.Main

open System
open System.Net
open Gluon
open Suave
open Suave.Filters
open Suave.Http
open Suave.Logging
open Suave.Operators
open Suave.Owin
open Suave.RequestErrors
open Suave.Web

let buildApp staticFilesLocation : WebPart =
    let assembly = Reflection.Assembly.GetCallingAssembly()
    let staticFilesLocation = defaultArg staticFilesLocation Environment.CurrentDirectory
    printfn "serving static files from %s" staticFilesLocation

    let gluonMidFunc =
        let service = Service.FromAssembly(assembly)
        let options = OwinOptions.Create(service)
        Owin.middleware options

    choose [
        Filters.pathRegex "(.*?)\.(fs|fsx|dll|pdb|mdb|log|config)" >=> FORBIDDEN "Access denied"
        Files.browse staticFilesLocation
        OwinApp.ofMidFunc "/" gluonMidFunc
        NOT_FOUND "Resource not found"
    ]

[<EntryPoint>]
let main argv =

    let port, staticFilesLocation =
        match argv with
        | [| port; path |] -> uint16 port, Some path
        | [| port |] -> uint16 port, None
        | _ -> 7000us, None

    let config =
      { defaultConfig with
          bindings = [ HttpBinding.mk HTTP IPAddress.Loopback port ]
          logger = Loggers.saneDefaultsFor LogLevel.Verbose
          listenTimeout = TimeSpan.FromMilliseconds 3000. }

    let app = buildApp staticFilesLocation
    startWebServer config app

    0

