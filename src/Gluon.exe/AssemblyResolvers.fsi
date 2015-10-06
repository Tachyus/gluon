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

type AssemblyLoader
type AssemblyResolver

val directory : path: string -> AssemblyResolver
val file : path: string -> AssemblyResolver
val firstMatch : seq<AssemblyResolver> -> AssemblyResolver

type AssemblyLoader with
    member LoadFile : path: string -> Assembly
    member Install : unit -> unit
    static member Create : AssemblyResolver -> AssemblyLoader
