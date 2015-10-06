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

namespace Tachyus.Gluon

open System
open System.IO

/// A capability to perform JSON serialization.
[<Sealed>]
type JsonSerializer =

    member CanSerialize<'T> : unit -> bool
    member CanSerialize : Type -> bool
    member FromJsonString<'T> : string -> 'T
    member FromJsonString : Type * string -> obj
    member ReadJson<'T> : TextReader -> 'T
    member ReadJson : Type * TextReader -> obj
    member ToJsonString<'T> : 'T -> string
    member ToJsonString : Type * obj -> string
    member WriteJson<'T> : TextWriter * 'T -> unit
    member WriteJson : Type * TextWriter * obj -> unit

    /// Constructs a serializer for a given set of seed types.
    /// The serializer can the serialize any type in the transitive closure of
    /// the `depends` relation over the seed types. A record type `depends`
    /// on the types of its fields, a union type on the types of its case
    /// fields, and so on.
    static member Create : seedTypes: seq<Type> -> JsonSerializer
