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

namespace Tachyus.Gluon.TypeScript

open System
open System.IO
open Tachyus.Gluon

/// Represents a TypeScript code unit.
[<Sealed>]
type CodeUnit =

    /// Writes code to a writer.
    member Write : TextWriter -> unit

    /// Writes code to a file.
    member WriteFile : path: string -> unit

    /// Writes code to a string.
    member WriteString : unit -> string

/// TypeScript program.
[<Sealed>]
type Program =

    /// Definitions code.
    member Definitions : CodeUnit

    /// Intialization code.
    member Initializer : CodeUnit

/// Provides TypeScript code generation.
[<Sealed>]
type Generator =

    /// Generates code for a service.
    member GenerateServiceCode : Service -> Program

    /// Generates type definitions and serialization code helpers.
    member GenerateTypeCode : seq<Schema.TypeDefinition> -> Program

    /// Writes code to a writer.
    member Write : service: Service * TextWriter -> unit

    /// Writes code to a file.
    member WriteFile : service: Service * path: string -> unit

    /// Writes code to a string.
    member WriteString : service: Service -> string

    /// Creates an instance.
    static member Create : unit  -> Generator
