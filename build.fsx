// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------
#I "packages/build/FAKE/tools"
#r "NuGet.Core.dll"
#r "FakeLib.dll"

open System
open System.Diagnostics
open System.IO
open Fake
open Fake.Git
open Fake.ReleaseNotesHelper
open Fake.YarnHelper

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

let private yarnFileName =
    let which = if isWindows then "where.exe" else "which"
    let yarn = if isWindows then "yarn.cmd" else "yarn"
    let defaultPath = if isWindows then "C:\\Program Files (x86)\\Yarn\\bin\\yarn.cmd" else "/usr/bin/yarn"
    let info =
        new ProcessStartInfo(which, yarn,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            RedirectStandardOutput = true,
            UseShellExecute        = false,
            CreateNoWindow         = true)
    use proc = Process.Start info
    proc.WaitForExit()
    match proc.ExitCode with
        | 0 when not proc.StandardOutput.EndOfStream ->
          proc.StandardOutput.ReadLine()
        | _ -> defaultPath

Target "BuildVersion" <| fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore

// --------------------------------------------------------------------------------------
// Clean build results & restore NuGet packages

Target "Clean" <| fun _ ->
    CleanDirs ["bin"; "temp"]
    if (Directory.Exists "src/Gluon.Client/node_modules") then
        DeleteDir "src/Gluon.Client/node_modules"

Target "CleanDocs" <| fun _ ->
    CleanDirs ["docs/output"]

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" <| fun _ ->
    DotNetCli.Build (fun p ->
        { p  with
            Project = "src\Gluon"
            Configuration = "Release" })
    Yarn (fun p ->
        { p with
            Command = Install Standard
            YarnFilePath = yarnFileName
            WorkingDirectory = "./src/Gluon.Client" })
    Yarn (fun p ->
        { p with
            Command = (Custom "build")
            YarnFilePath = yarnFileName
            WorkingDirectory = "./src/Gluon.Client" })

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

Target "PackGluon" <| fun _ ->
    DotNetCli.Pack <| fun x ->
        { x with
            Project = "src\Gluon"
            OutputPath = buildDir
            AdditionalArgs =
              [ "--no-build"
                sprintf "/p:Version=%s" nugetVersion
                //"/p:ReleaseNotes=" + (toLines release.Notes)
              ]
        }

Target "PackGluonClient" <| fun _ ->
    Paket.Pack <| fun x ->
        { x with
            OutputPath = "bin"
            Version = nugetVersion
            ReleaseNotes = String.concat Environment.NewLine release.Notes }

Target "PublishNuGet" <| fun _ ->
    Paket.Push <| fun p ->
        { p with WorkingDir = "bin" }

// --------------------------------------------------------------------------------------
// Generate the documentation

// --------------------------------------------------------------------------------------
// Generate the documentation

Target "GenerateReferenceDocs" <| fun _ ->
    if not <| executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"; "--define:REFERENCE"] [] then
      failwith "generating reference documentation failed"

let generateHelp' fail debug =
    let args =
        if debug then ["--define:HELP"]
        else ["--define:RELEASE"; "--define:HELP"]
    if executeFSIWithArgs "docs/tools" "generate.fsx" args [] then
        traceImportant "Help generated"
    else
        if fail then
            failwith "generating help documentation failed"
        else
            traceImportant "generating help documentation failed"

let generateHelp fail =
    generateHelp' fail false

Target "GenerateHelp" <| fun _ ->
    DeleteFile "docs/content/release-notes.md"
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"
    generateHelp true

Target "GenerateHelpDebug" <| fun _ ->
    DeleteFile "docs/content/release-notes.md"
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"
    generateHelp' true true

Target "KeepRunning" <| fun _ ->
    use watcher = !! "docs/content/**/*.*" |> WatchChanges (fun changes ->
        generateHelp false)
    traceImportant "Waiting for help edits. Press any key to stop."
    System.Console.ReadKey() |> ignore
    watcher.Dispose()

Target "GenerateDocs" DoNothing

// --------------------------------------------------------------------------------------
// Release Scripts

Target "ReleaseDocs" <| fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir
    CopyRecursive "docs/output" tempDocsDir true |> tracefn "%A"
    StageAll tempDocsDir
    Commit tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir

Target "Release" <| fun _ ->
    StageAll ""
    Commit "" (sprintf "Bump version to %s" release.NugetVersion)
    Branches.push ""
    Branches.tag "" release.NugetVersion
    Branches.pushTag "" "origin" release.NugetVersion

Target "BuildPackage" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  =?> ("BuildVersion", isAppVeyorBuild)
  ==> "Build"
  ==> "RunTests"
  ==> "All"
  =?> ("GenerateReferenceDocs",isLocalBuild && not isMono)
  =?> ("GenerateDocs",isLocalBuild && not isMono)
  =?> ("ReleaseDocs",isLocalBuild && not isMono)

"All" 
  ==> "PackGluon"
  ==> "PackGluonClient"
  ==> "BuildPackage"

"CleanDocs"
  ==> "GenerateHelp"
  ==> "GenerateReferenceDocs"
  ==> "GenerateDocs"
    
"ReleaseDocs"
  ==> "Release"

"BuildPackage"
  ==> "PublishNuGet"
  ==> "Release"

RunTargetOrDefault "All"
