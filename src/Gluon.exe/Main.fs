// Copyright 2015 Tachyus Corp.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License. You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied. See the License for the specific language governing
// permissions and limitations under the License.

module internal Tachyus.Gluon.Main

module AR = AssemblyResolvers

[<EntryPoint>]
let start args =
    CommandLine.main args <| fun args ->
        let resolver =
            AR.firstMatch [
                for arg in args do
                    match arg with
                    | CommandLine.Bin dir -> yield AR.directory dir
                    | CommandLine.Reference dll -> yield AR.file dll
                    | CommandLine.Reflect dll -> yield AR.file dll
                    | _ -> ()
            ]
        let loader = AR.AssemblyLoader.Create(resolver)
        loader.Install()
        let service =
            args
            |> List.choose (function
                | CommandLine.Reflect dll -> Some dll
                | _ -> None)
            |> List.map loader.LoadFile
            |> Seq.map Service.FromAssembly
            |> Service.Merge
        let outFile =
            args
            |> List.pick (function
                | CommandLine.Out file -> Some file
                | _ -> None)
        printfn "writing %s" outFile
        TypeScript.Generator.Create().WriteFile(service, outFile)
