#I "../../packages/FAKE/tools"
#r "FakeLib.dll"

open System
open Fake

let msbuild target projects =
    let properties =
        [ "Optimize",      environVarOrDefault "Build.Optimize"      "True"
          "DebugSymbols",  environVarOrDefault "Build.DebugSymbols"  "True"
          "Configuration", environVarOrDefault "Build.Configuration" "Release"
          "OutputPath",    environVarOrDefault "Build.OutputPath"    "bin" |> FullName |> trimSeparator ]
    for projFile in projects do
        build (fun x ->
            { x with
                NodeReuse = false
                Properties = properties
                Targets = [ target ]
                Verbosity = Some Quiet }) projFile

Target "Clean" <| fun _ ->
    CleanDirs ["bin"]
    msbuild "Clean" !!"../SampleApp/SampleApp.fsproj"

Target "Build" <| fun _ ->
    msbuild "Build" !!"../SampleApp/SampleApp.fsproj"

Target "Help" <| fun _ ->
    printfn "------------------------------"
    printfn "Gluon Sample FAKE build script"
    printfn "------------------------------"
    printfn "Targets:"
    printfn "* Build - builds the SampleApp and outputs assemblies in WebApp/bin"
    printfn "* Clean - cleans the SampleApp and removes assemblies from WebApp/bin"
    printfn "------------------------------"

RunTargetOrDefault "Help"

