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

open Newtonsoft.Json
open Newtonsoft.Json.Linq

/// Represents arbitrary raw pass-through JSON.
[<Sealed>]
type Json internal (rawJson: JToken) =

    /// Converts to a raw JSON string.
    member this.ToJsonString() =
        rawJson

    /// Same as ToJsonString.
    override this.ToString() =
        rawJson.ToString(Formatting.None)

    /// The JToken representation.
    member this.JToken = rawJson

    /// Lifts a raw Json string to Json.
    static member FromJsonString(rawJson: string) =
        if rawJson = null then
            nullArg "rawJson"
        Json(JToken.Parse(rawJson))
