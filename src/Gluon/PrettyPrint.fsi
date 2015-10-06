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

/// Wadler-style pretty-printer.
module internal PrettyPrint =

    type Layout

    module Operators =
        val ( +. ) : Layout -> Layout -> Layout
        val ( +/ ) : Layout -> Layout -> Layout
        val ( ++ ) : Layout -> Layout -> Layout

    val empty : Layout

    val append : Layout -> Layout -> Layout
    val group : Layout -> Layout
    val indent : int -> Layout -> Layout

    val line : string -> Layout
    val text : string -> Layout

    type Options =
        {
            NewLine : string
            PrintIndentation : TextWriter -> int -> unit
            Width : int
            Writer : TextWriter
        }

        static member Create : unit -> Options

    type Layout with
        member Write : ?options: Options -> unit
        member WriteToString : ?options: Options -> string

    val writeLayout : TextWriter -> layout: Layout -> unit
