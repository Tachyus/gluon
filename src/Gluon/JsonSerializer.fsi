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

    open System.Runtime.CompilerServices

    /// A contract resolver that requires presence of all properties
    /// that are not of type option<_>
    [<Sealed>]
    type RequireNonOptionalPropertiesContractResolver =
        inherit Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver
        new : unit -> RequireNonOptionalPropertiesContractResolver

    /// Compact serialization where tuples are serialized as JSON objects
    [<Sealed>]
    type CompactStrictSettings =
        static member formatting : Newtonsoft.Json.Formatting
        static member settings : Newtonsoft.Json.JsonSerializerSettings

    type private S = Microsoft.FSharpLu.Json.With<CompactStrictSettings>

    /// Serialize an object to Json with the specified converter
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline serialize< ^T> : x:obj -> string
    /// Serialize an object to Json with the specified converter and save the result to a file
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline serializeToFile< ^T> : file:string -> obj:obj -> unit
    /// Serialize an object to Json with the specified converter and save the result to a stream
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline serializeToStream< ^T> : stream:System.IO.Stream -> obj: ^T -> unit
    /// Try to deserialize json to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline tryDeserialize< ^T> : json:string -> Choice< ^T, string>
    /// Try to read Json from a file and desrialized it to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline tryDeserializeFile< ^T> : file:string -> Choice< ^T, string>
    /// Try to deserialize a stream to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline tryDeserializeStream< ^T> : stream:System.IO.Stream -> Choice< ^T, string>
    /// Deserialize a Json to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline deserialize< ^T> : json:string ->  ^T
    /// Read Json from a file and desrialized it to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline deserializeFile< ^T> : file:string -> ^T
    /// Deserialize a stream to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    val inline deserializeStream< ^T> : stream:System.IO.Stream -> ^T
    /// Serialize an object to Json with the specified converter
    val inline serializeType : typ:System.Type -> x:obj -> string
    /// Serialize an object to Json with the specified converter and save the result to a file
    val inline serializeTypeToFile : typ:System.Type -> file:string -> obj:obj -> unit
    /// Serialize an object to Json with the specified converter and save the result to a stream
    val inline serializeTypeToStream : typ:System.Type -> stream:System.IO.Stream -> obj:obj -> unit
    /// Deserialize a Json to an object of type typ
    val inline deserializeType : typ:System.Type -> json:string -> obj
    /// Read Json from a file and desrialized it to an object of type typ
    val inline deserializeTypeFromFile : typ:System.Type -> file:string -> obj
    /// Deserialize a stream to an object of type typ
    val inline deserializeTypeFromStream : typ:System.Type -> stream:System.IO.Stream -> obj
