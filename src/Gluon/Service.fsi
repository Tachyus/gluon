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
open System.Reflection

/// A runtime desctiptor of a Gluon service, a set of remote-callable methods.
[<Sealed>]
type Service =

    /// All types used in method input and output.
    member internal IOTypes : seq<Type>

    /// Constituent methods.
    member Methods : seq<Method>

    /// Schema metadata associated with the service.
    member Schema : Schema.Service

    /// Traverses an assembly to detect methods marked with
    /// RemoteAttribute and constructs a corresponding service.
    static member FromAssembly : Assembly -> Service

    /// Creates a single-method service.
    static member FromMethod : Method -> Service

    /// Merges multiple services into one.
    static member Merge : seq<Service> -> Service
