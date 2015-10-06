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

module internal Gluon.CLI.CommandLine

open System
open System.IO
open System.Reflection
open Nessos.Argu

let usage = "Gluon: generate TypeScript client proxies to F# web services."

type Args =
    | Bin of string
    | [<Mandatory>] Out of string
    | Reference of string
    | [<Mandatory>] Reflect of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Bin _ -> "add a bin-like folder to search for references."
            | Reference _ -> "add a reference assembly."
            | Reflect _ -> "reflect an assembly to search for services."
            | Out _ -> "generate a TypeScript file."

let main args handle =
    let validate (args: seq<Args>) =
        let fail fmt =
            Printf.ksprintf (fun msg-> eprintfn "%s" msg; exit 1) fmt
        let assertAssembly file =
            if File.Exists(file) |> not then
                fail "file does not exist: %s" file
            try AssemblyName.GetAssemblyName(file) |> ignore
            with _ -> fail "not an assembly: %s" file
        let outCount = ref 0
        for arg in args do
            match arg with
            | Bin dir ->
                if Directory.Exists(dir) |> not then
                    fail "directory does not exist: %s" dir
            | Out file ->
                if Path.GetExtension(file) <> ".ts" then
                    fail "not a .ts TypeScript file: %s" file
                incr outCount
                if !outCount > 1 then
                    fail "more than one output file"
            | Reference file | Reflect file ->
                assertAssembly file
    let prepareDir args =
        for arg in args do
            match arg with
            | Out file ->
                let file = Path.GetFullPath(file)
                let dir = Path.GetDirectoryName(file)
                if Directory.Exists(dir) |> not then
                    Directory.CreateDirectory(dir) |> ignore
            | _ -> ()
    let parser = ArgumentParser.Create<Args>(usage)
    let results = parser.ParseCommandLine(args, errorHandler=ProcessExiter())
    let all = results.GetAllResults()
    validate all
    prepareDir all
    handle all
    0
