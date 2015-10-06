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
open System.IO

module PrettyPrint =

    type Layout =
        {
            /// Document size when flattened.
            flatSize : int

            /// Minimal width of the first line.
            minWidth : int

            /// The document node.
            node : Node

            /// True if document contains no newline nodes.
            singleLine : bool
        }

    and Node =
        | Append of Layout * Layout
        | Empty
        | Group of Layout
        | Indent of int * Layout
        | Line of string
        | Text of string

    let append a b =
        match a.node, b.node with
        | Empty, _ -> b
        | _, Empty -> a
        | _ ->
            {
                node = Append (a, b)
                flatSize = a.flatSize + b.flatSize
                minWidth =
                    if a.singleLine
                        then a.minWidth + b.minWidth
                        else a.minWidth
                singleLine = a.singleLine && b.singleLine
            }

    let empty =
        {
            node = Empty
            flatSize = 0
            minWidth = 0
            singleLine = true
        }

    let group x =
        { x with node = Group x }

    let indent k x =
        { x with node = Indent (k, x) }

    let line x =
        {
            node = Line x
            flatSize = String.length x
            minWidth = 0
            singleLine = false
        }

    let text x =
        let n = String.length x
        {
            node = Text x
            flatSize = n
            minWidth = n
            singleLine = true
        }

    let rec flatten x =
        match x.node with
        | Append (a, b) -> append (flatten a) (flatten b)
        | Empty | Text _ -> x
        | Group x | Indent (_, x) -> flatten x
        | Line x -> text x

    module Operators =
        let ( +. ) a b = append a b
        let ( ++ ) a b = a +. text " " +. b
        let ( +/ ) a b = a +. line " " +. b

    type StackNode =
        {
            doc : Layout
            minTotal : int
            offset : int
        }

    type Stack =
        | N
        | C of StackNode * Stack

    let minTotal stack =
        match stack with
        | N -> 0
        | C (x, _) -> x.minTotal

    let push o n stack =
        let m =
            if n.singleLine
                then minTotal stack + n.minWidth
                else n.minWidth
        C ({ doc = n; offset = o; minTotal = m }, stack)

    type Options =
        {
            NewLine : string
            PrintIndentation : TextWriter -> int -> unit
            Width : int
            Writer : TextWriter
        }

        static member Create() =
            {
                NewLine = Environment.NewLine
                PrintIndentation = fun w n ->
                    for i in 1 .. n do
                        w.Write(' ')
                Writer = stdout
                Width = 79
            }

    let write options layout =
        let width = options.Width
        let writer = options.Writer
        let newLine = options.NewLine
        let printIndentation = options.PrintIndentation
        let output (s: string) =
            writer.Write(s)
        let indent (n: int) =
            writer.Write(newLine)
            printIndentation writer n
        let rec pr k x =
            match x with
            | N -> ()
            | C (x, z) ->
                let i = x.offset
                match x.doc.node with
                | Append (a, b)  ->
                    pr k (push i a (push i b z))
                | Empty ->
                    pr k z
                | Group x ->
                    let y =
                        if x.flatSize + minTotal z <= width - k
                            then flatten x
                            else x in
                    pr k (push i y z)
                | Indent (j, x) ->
                    pr k (push (i + j) x z)
                | Line _ ->
                    indent i
                    pr i z
                | Text s ->
                    output s
                    pr (k + String.length s) z in
            pr 0 (push 0 layout N)

    let opts options =
        match options with
        | None -> Options.Create()
        | Some options -> options

    type Layout with

        member this.Write(?options) =
            write (opts options) this

        member this.WriteToString(?options) =
            use sw = new StringWriter()
            write { opts options with Writer = sw } this
            sw.ToString()

    let writeLayout out (layout: Layout) =
        let opts = { Options.Create() with Writer = out }
        layout.Write(opts)
        out.WriteLine()
