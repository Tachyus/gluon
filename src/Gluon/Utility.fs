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
open System.Collections.Generic

module Utility =

    let bindingFlags =
        BindingFlags.NonPublic
        ||| BindingFlags.Public
        ||| BindingFlags.Static
        ||| BindingFlags.Instance

    let topSortBy<'K,'T when 'K : equality>
            (key: 'T -> 'K)
            (pred: 'T -> seq<'T>)
            (seed: seq<'T>) =
        let out = ResizeArray()
        let visited =
            HashSet<'T> {
                new IEqualityComparer<'T> with
                    member this.GetHashCode(value) =
                        hash (key value)
                    member this.Equals(a, b) =
                        key a = key b
            }
        let rec visit ty =
            if visited.Add(ty) then
                Seq.iter visit (pred ty)
                out.Add(ty)
        for sT in seed do
            visit sT
        List.ofSeq out :> seq<_>

    let topSort predecessors seed =
        topSortBy (fun x -> x) predecessors seed
