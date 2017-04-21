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

namespace Gluon.Tests

open System
open System.Collections.Generic
open System.Reflection
open Xunit
open FsCheck
open FsCheck.Xunit
open Gluon

module TestApp =

    [<Remote(Verb="GET")>]
    let testGet () = 1

    [<Remote>]
    let testPost (x:string) = 1

module SampleApp =

    [<Remote(Verb="GET")>]
    let sampleGet () = 1

    [<Remote>]
    let samplePost (x:string) = 1

[<Trait("Kind", "UnitTest")>]
module CodeGenTests =
    open Gluon.TypeScript

    let service = Service.FromAssembly(Assembly.GetExecutingAssembly())
    let internal defs =
        Syntax.DefinitionSequence [
            CodeGen.typeDefinitions service.Schema.TypeDefinitions
            CodeGen.methodStubs service.Schema
        ]

    [<Property>]
    let ``should create service with two methods`` () =
        service.Methods |> Seq.length = 4
    
    [<Property>]
    let ``should create service with 2 namespace groups`` () =
        match defs.GroupNamespaces() with
        | Syntax.DefinitionSequence ns -> ns.Length = 2
        | _ -> failwith "Expected a DefinitionSequence at the top of the tree"

    [<Property>]
    let ``should create service with 2 namespace groups matching module names`` () =
        match defs.GroupNamespaces() with
        | Syntax.DefinitionSequence [Syntax.InNamespace(ns1, _); Syntax.InNamespace(ns2,_)] ->
            ns1 = "Gluon.Tests.SampleApp" && ns2 = "Gluon.Tests.TestApp"
        | _ -> failwith "Expected a DefinitionSequence at the top of the tree"
