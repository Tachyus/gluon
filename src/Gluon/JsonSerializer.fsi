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

/// A capability to perform JSON serialization.
/// Compact serializer where desearilization requires presence of all properties 
/// expect optional ones (of type option<_>)
namespace Gluon

module JsonSerializer =

    /// Serialize an object to Json with the specified converter
    val serialize<'T> : x:'T -> string
    /// Serialize an object to Json with the specified converter and save the result to a stream
    val serializeToStream<'T> : stream:System.IO.Stream -> obj:'T -> unit
    /// Serialize an object to Json with the specified converter
    val serializeType : typ:System.Type -> x:obj -> string
    /// Serialize an object to Json with the specified converter and save the result to a stream
    val serializeTypeToStream : typ:System.Type -> stream:System.IO.Stream -> obj:obj -> unit
    /// Deserialize a Json to an object of type typ
    val deserializeType : typ:System.Type -> json:string -> obj
    /// Deserialize a stream to an object of type typ
    val deserializeTypeFromStream : typ:System.Type -> stream:System.IO.Stream -> obj
