#I "../../packages/FAKE/tools"
#r "FakeLib.dll"

open System
open Fake

Target "Clean" <| fun _ ->
    try
        CleanDirs ["bin"]
        !! "../SampleApp/SampleApp.fsproj"
        |> MSBuildReleaseExt "bin" ["Verbosity","Quiet"] "Clean" |> ignore
    finally
        killMSBuild()

Target "Build" <| fun _ ->
    try
        !! "../SampleApp/SampleApp.fsproj"
        |> MSBuildReleaseExt "bin" ["Verbosity","Quiet"] "Build" |> ignore
    finally
        killMSBuild()

Target "Help" <| fun _ ->
    printfn "------------------------------"
    printfn "Gluon Sample FAKE build script"
    printfn "------------------------------"
    printfn "Targets:"
    printfn "* Build - builds the SampleApp and outputs assemblies in WebApp/bin"
    printfn "* Clean - cleans the SampleApp and removes assemblies from WebApp/bin"
    printfn "------------------------------"

RunTargetOrDefault "Help"

