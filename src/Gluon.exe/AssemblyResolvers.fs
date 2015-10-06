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

module internal Tachyus.Gluon.AssemblyResolvers

open System
open System.Reflection
open System.Collections.Generic
open System.IO

type AssemblyResolver =
    { resolve : AssemblyName -> option<string> }

let directory (dir: string) =
    let dir = Path.GetFullPath(dir)
    let resolve (asm: AssemblyName) =
        let p1 = sprintf "%s.dll" asm.Name
        let p2 = sprintf "%s.exe" asm.Name
        [p1; p2]
        |> List.map (fun p -> Path.Combine(dir, p))
        |> List.tryFind File.Exists
    { resolve = resolve }

let file file =
    let file = Path.GetFullPath(file)
    let name = AssemblyName.GetAssemblyName(file)
    let resolve (asm: AssemblyName) =
        if asm.Name = name.Name then Some file else None
    { resolve = resolve }

let firstMatch resolvers =
    let resolve asm =
        resolvers
        |> Seq.tryPick (fun r -> r.resolve asm)
    { resolve = resolve }

let memoized (d: Dictionary<'A,'B>) (f: 'A -> 'B) (x: 'A) : 'B =
    if d.ContainsKey(x) then d.[x] else
        let y = f x
        d.Add(x, y)
        y

let localAssemblies =
    [
        typedefof<list<_>>.Assembly // FSharp.Core.dll
        typeof<Tachyus.Gluon.Schema.Service>.Assembly // Gluon.dll
    ]

let tryFindLocalAssembly (asm: AssemblyName) =
    localAssemblies
    |> Seq.tryFind (fun x ->
        x.GetName().Name = asm.Name)

let tryFindLoadedAssembly (asm: AssemblyName) =
    AppDomain.CurrentDomain.GetAssemblies()
    |> Seq.tryFind (fun x ->
        x.GetName().Name = asm.Name)

[<Sealed>]
type AssemblyLoader(resolver: AssemblyResolver) =
    let table = Dictionary()

    let load (name: AssemblyName) =
        match tryFindLocalAssembly name with
        | Some asm -> asm
        | None ->
            match tryFindLoadedAssembly name with
            | Some asm -> asm
            | None ->
                match resolver.resolve(name) with
                | None ->
                    eprintfn "could not resolve assembly: %s" name.Name
                    null
                | Some file -> Assembly.LoadFrom(file)

    member this.LoadFile(file: string) =
        AssemblyName.GetAssemblyName(file)
        |> memoized table load

    member this.Install() =
        AppDomain.CurrentDomain.add_AssemblyResolve(ResolveEventHandler(fun ctx args ->
            let name = AssemblyName(args.Name)
            memoized table load name))

    static member Create(resolver) =
        AssemblyLoader(resolver)
