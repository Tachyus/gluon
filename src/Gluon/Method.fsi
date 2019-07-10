// Copyright 2019 Tachyus Corp.
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

open System.Reflection
open System.Threading.Tasks
open Microsoft.AspNetCore.Http

type internal MethodKey =
    {
        httpMethod : Schema.HttpMethod
        localPath : string
    }

/// An executable runtime descriptor of a Gluon remote method.
[<Sealed>]
type Method =

    /// Given a encoded input arguments, invokes the method
    /// and asynchronously computes the boxed response.
    /// Argument encoding depends on arity:
    ///     arity = 0   arg = null
    ///     arity = 1   arg = box x
    ///     arity = N   arg = box (x, y, .. z)
    member Invoke : HttpContext * arg: obj -> Task<obj>

    /// Types used in input and output.
    member internal IOTypes : Reflect.IOTypes

    /// The httpMethod and localPath pair.
    member internal Key : MethodKey

    /// Schema metadata associated with the method.
    member Schema : Schema.Method

    /// Defines a method with arity = 0.
    static member Create : name: string * body: (unit -> Task<'A>) -> Method

    /// Defines a method with arity = 1.
    static member Create :
        name: string * argName: string *
        body: ('A -> Task<'B>) -> Method

    /// Defines a method with arity = 2.
    static member Create :
        name: string * arg1Name: string * arg2Name: string *
        body: ('A -> 'B -> Task<'C>) -> Method

    /// Defines a method by reflecting a static runtime method.
    static member Reflect : MethodInfo -> Method
