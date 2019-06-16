// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------
#I "packages/build/FAKE/tools"
#r "NuGet.Core.dll"
#r "FakeLib.dll"

open System
open System.IO
open Fake
open Fake.Git
open Fake.ReleaseNotesHelper

// --------------------------------------------------------------------------------------
// Provide project-specific details below
// --------------------------------------------------------------------------------------

// File system information 
// (<solutionFile>.sln is built during the building process)
let solutionFile = "Gluon.sln"

// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "test/**/bin/Release/*Tests*.dll"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted 
let gitHome = "https://github.com/Tachyus"

// The name of the project on GitHub
let gitName = "gluon"

let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/Tachyus"

let buildDir = IO.Path.Combine(Environment.CurrentDirectory, "bin")

// --------------------------------------------------------------------------------------
// The rest of the file includes standard build steps 
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let (!!) includes =
    (!! includes).SetBaseDirectory __SOURCE_DIRECTORY__

let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")
let isAppVeyorBuild = not(isNull(environVar "APPVEYOR"))

let nugetVersion =
    if isAppVeyorBuild then
        let isTagged = Boolean.Parse(environVar "APPVEYOR_REPO_TAG")
        if isTagged then
            environVar "APPVEYOR_REPO_TAG_NAME"
        else
            sprintf "%s-b%03i" release.NugetVersion (int buildVersion)
    else release.NugetVersion

Target "BuildVersion" <| fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore

// --------------------------------------------------------------------------------------
// Clean build results & restore NuGet packages

Target "Clean" <| fun _ ->
    CleanDirs ["bin"]
    !!"src/Gluon/bin"
    ++"src/Gluon/obj"
    ++"src/Gluon.CLI/bin"
    ++"src/Gluon.CLI/obj"
    |> Seq.iter (fun dir -> if Directory.Exists dir then DeleteDir dir)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" <| fun _ ->
    !! "src\Gluon"
    ++ "src\Gluon.CLI"
    |> Seq.iter (fun project ->
        DotNetCli.Build (fun p ->
            { p  with
                Project = project
                Configuration = "Release" }))

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "RunTests" <| fun _ ->
    DotNetCli.Test (fun p ->
        { p with
            Project = "test\Gluon.Tests"
            Configuration = "Release"
            TimeOut = TimeSpan.FromMinutes 20.
            AdditionalArgs =
              [ yield "--test-adapter-path:."
                yield if isAppVeyorBuild then
                        sprintf "--logger:Appveyor"
                      else
                        sprintf "--logger:xunit;LogFileName=%s" (IO.Path.Combine(buildDir, "TestResults.xml")) ]
        })

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target "BuildPackage" <| fun _ ->
    !! "src\Gluon"
    ++ "src\Gluon.CLI"
    |> Seq.iter (fun project ->
        DotNetCli.Pack <| fun x ->
          { x with
              Project = project
              OutputPath = buildDir
              AdditionalArgs =
                [ "--no-build"
                  sprintf "/p:Version=%s" nugetVersion
                  //"/p:ReleaseNotes=" + (toLines release.Notes)
                ]
          })

Target "PublishNuGet" <| fun _ ->
    Paket.Push <| fun p ->
        { p with WorkingDir = "bin" }

// --------------------------------------------------------------------------------------
// Release Scripts

Target "Release" <| fun _ ->
    StageAll ""
    Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.push ""
    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  =?> ("BuildVersion", isAppVeyorBuild)
  ==> "Build"
  ==> "RunTests"
  ==> "All"
  ==> "BuildPackage"
  ==> "PublishNuGet"
  ==> "Release"

RunTargetOrDefault "All"
