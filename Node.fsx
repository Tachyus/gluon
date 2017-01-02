#I "packages/FAKE/tools"
#r "FakeLib.dll"
open System
open System.IO
open Fake

let getProgramFiles() =
    seq {
        match Environment.Is64BitOperatingSystem, Environment.Is64BitProcess with
        | true, true ->
            yield Environment.GetFolderPath Environment.SpecialFolder.ProgramFiles // C:\Program Files
            yield Environment.GetFolderPath Environment.SpecialFolder.ProgramFilesX86 // C:\Program Files (x86)
        | true, false ->
            yield Environment.GetEnvironmentVariable "ProgramW6432" // C:\Program Files
            yield Environment.GetFolderPath Environment.SpecialFolder.ProgramFiles // C:\Program Files (x86)
        | false, _ ->
            yield Environment.GetFolderPath Environment.SpecialFolder.ProgramFiles
    }

let tryFindApp (app: string) (dirs: string seq) =
    dirs |> Seq.map (fun dir -> dir </> app) |> Seq.tryFind File.Exists

let npm workingDir task =
    let fileName, arguments =
        let programFiles = getProgramFiles() |> List.ofSeq
        let subdirs = programFiles |> Seq.map (fun dir -> dir </> "nodejs")
        match tryFindApp "node.exe" subdirs with
        | Some path ->
            let npmPath = (Path.GetDirectoryName path) </> @"node_modules/npm/bin/npm-cli.js"
            path, sprintf "\"%s\" %s" npmPath task
        | None -> "npm", task // try to run npm directly
    let result =
        ExecProcess (fun p ->
            p.FileName <- fileName
            p.Arguments <- arguments
            if not (String.IsNullOrEmpty workingDir) then
                p.WorkingDirectory <- p.WorkingDirectory @@ workingDir
            logfn "%s> \"%s\" %s" p.WorkingDirectory p.FileName p.Arguments
        ) (TimeSpan.FromMinutes 5.)
    if result <> 0 then failwithf "npm %s failed" task
