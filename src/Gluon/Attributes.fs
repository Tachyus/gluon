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

namespace Gluon

open System
open System.Net.Http

/// Marks static methods that comprise an auto-detected Gluon service.
[<Sealed>]
[<AttributeUsage(AttributeTargets.Method)>]
type RemoteAttribute() =
    inherit Attribute()

    /// Preferred path for the HTTP convention.
    /// Example: "/MyController/MyMethod".
    /// If null, it is inferred from method and enclosing class name.
    member val Path : string = null with get, set

    /// Preferred HTTP method or verb.
    member val Verb = "POST" with get, set
