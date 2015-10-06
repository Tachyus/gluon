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
open System.Reflection

/// An executable runtime descriptor of a Gluon remote method.
[<Sealed>]
type Method =

    /// Given a encoded input arguments, invokes the method
    /// and asynchronously computes the boxed response.
    /// Argument encoding depends on arity:
    ///     arity = 0   arg = null
    ///     arity = 1   arg = box x
    ///     arity = N   arg = box (x, y, .. z)
    member Invoke : Context * arg: obj -> Async<obj>

    /// Types used in input and output.
    member internal IOTypes : Reflect.IOTypes

    /// Schema metadata associated with the method.
    member Schema : Schema.Method

    /// Defines a method with arity = 0.
    static member Create : name: string * body: (unit -> Async<'A>) -> Method

    /// Defines a method with arity = 1.
    static member Create :
        name: string * argName: string *
        body: ('A -> Async<'B>) -> Method

    /// Defines a method with arity = 2.
    static member Create :
        name: string * arg1Name: string * arg2Name: string *
        body: ('A -> 'B -> Async<'C>) -> Method

    /// Defines a method by reflecting a static runtime method.
    static member Reflect : MethodInfo -> Method
