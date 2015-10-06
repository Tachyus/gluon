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

namespace Tachyus.Gluon.Tests

open System
open System.Collections.Generic
open Xunit
open FsCheck
open FsCheck.Xunit
open Tachyus
open Tachyus.Gluon

[<Trait("Kind", "UnitTest")>]
module JsonProtocolTests =

    let fromString<'T> (s: string) : 'T =
        let ser = JsonSerializer.Create([typeof<'T>])
        ser.FromJsonString(s)

    let toString<'T> (value: 'T) : string =
        let ser = JsonSerializer.Create([typeof<'T>])
        ser.ToJsonString(value)

    let turnsAround<'T when 'T : equality> (value: 'T) =
        fromString (toString value) = value

    [<Property>]
    let ``bool turnaround`` () =
        turnsAround true && turnsAround false

    [<Property>]
    let ``int turnaround`` (i: int) =
        turnsAround i

    [<Property>]
    let ``float turnaround`` (f: float) =
        Double.IsNaN(f) || turnsAround f

    [<Property>]
    let ``int-looking float gets parsed`` (i: int) =
        fromString (toString i) = float i

    [<Property>]
    let ``nan turnaround`` () =
        Double.IsNaN(fromString (toString nan))

    [<Property>]
    let ``infinity turnaround`` () =
        turnsAround infinity && turnsAround -infinity

    [<Property>]
    let ``string turnaround`` (s: string) =
        turnsAround s

    [<Property>]
    let ``null string turnaround`` () =
        let s : string = null
        turnsAround s

    [<Property>]
    let ``datelike string turnaround`` () =
        turnsAround "2015-03-16T17:34:33.1257117-07:00"

    type U =
        | U0
        | U1 of string
        | U2 of int * U * U

    [<Property>]
    let ``union turnaround`` (u: U) =
        turnsAround u

    type Record =
        {
            name : string
            age : int
        }

    [<Property>]
    let ``record turnaround`` (r: Record) =
        turnsAround r

    type LAInput =
        | IntList of list<int>
        | IntArray of array<int>
        | StringList of list<string>
        | StringArray of array<string>
        | DeepList of list<list<string>>
        | DeepArray of array<array<string>>

    [<Property>]
    let ``list and array turnaround`` (input: LAInput) =
        turnsAround input

    [<Property>]
    let ``seq turnaround`` (xs: int[]) =
        let sq = Seq.ofArray xs
        fromString (toString sq) |> Seq.toArray = xs

    [<Property>]
    let ``option turnaroud`` (x: option<option<int>>) =
        turnsAround x

    [<Property>]
    let ``dateTime turnaround`` (x: DateTime) =
        turnsAround x

    type TupleInput =
        | T2 of (int * string)
        | T3 of (int * string * bool)
        | T4 of ((int * string) * list<int> * bool * int)

    [<Property>]
    let ``tuple turnaround`` (t: TupleInput) =
        turnsAround t

    [<Property>]
    let ``string dict turnaround`` (t: list<string * int>) =
        if t |> List.exists (fun (k, _) -> k = null) then
            true
        else
            let d1 = dict t
            let d2 = fromString<IDictionary<string,int>> (toString d1)
            Seq.toList d1 = Seq.toList d2

    [<Property>]
    let ``bytes turnaround`` (bytes: byte[]) =
        turnsAround bytes

    [<Property>]
    let ``raw json turnaround`` () =
        let jt = Json.FromJsonString("""{"foo": [1, true, "bar"]}""")
        string (fromString<Json> (toString jt)) = string jt
