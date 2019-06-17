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
    open Microsoft.FSharpLu.Json
    open Newtonsoft.Json

    /// A contract resolver that requires presence of all properties
    /// that are not of type option<_>
    [<Sealed>]
    type RequireNonOptionalPropertiesContractResolver() =
        inherit Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        override __.CreateProperty(_member, memberSerialization) =
            let property = base.CreateProperty(_member, memberSerialization)
            let isRequired = not (property.PropertyType.IsGenericType 
                               && property.PropertyType.GetGenericTypeDefinition() = typedefof<option<_>>)
            if isRequired then 
                property.Required <- Required.Always
                property.NullValueHandling <- System.Nullable NullValueHandling.Ignore
            property

    /// Compact serialization where tuples are serialized as JSON objects
    [<Sealed>]
    type CompactStrictSettings =
        static member formatting = Formatting.Indented
        static member settings =
            JsonSerializerSettings
                (
                    ContractResolver = RequireNonOptionalPropertiesContractResolver(),
                    Converters = [|CompactUnionJsonConverter()|]
                )

    type private S = With<CompactStrictSettings>

    /// Serialize an object to Json with the specified converter
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline serialize< ^T> x = S.serialize x
    /// Serialize an object to Json with the specified converter and save the result to a file
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline serializeToFile< ^T> file obj = S.serializeToFile file obj
    /// Serialize an object to Json with the specified converter and save the result to a stream
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline serializeToStream< ^T> (stream:System.IO.Stream) (obj: ^T) =
        let serializer = JsonSerializer.Create(CompactStrictSettings.settings)
        serializer.Formatting <- CompactStrictSettings.formatting
        // Leave stream open after writing
        let DefaultStreamWriterEncoding = System.Text.UTF8Encoding.UTF8
        let DefaultStreamWriterBufferSize = 1024
        use streamWriter = new System.IO.StreamWriter(stream, DefaultStreamWriterEncoding, DefaultStreamWriterBufferSize, true)
        use jsonTextWriter = new JsonTextWriter(streamWriter)
        serializer.Serialize(jsonTextWriter, obj)
    /// Try to deserialize json to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline tryDeserialize< ^T> json = S.tryDeserialize< ^T> json
    /// Try to read Json from a file and desrialized it to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline tryDeserializeFile< ^T> file = S.tryDeserializeFile< ^T> file
    /// Try to deserialize a stream to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline tryDeserializeStream< ^T> stream = S.tryDeserializeStream< ^T> stream
    /// Deserialize a Json to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline deserialize< ^T> json : ^T = S.deserialize< ^T> json
    /// Read Json from a file and desrialized it to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline deserializeFile< ^T> file = S.deserializeFile< ^T> file
    /// Deserialize a stream to an object of type ^T
    [<MethodImplAttribute(MethodImplOptions.NoInlining)>]
    let inline deserializeStream< ^T> stream = S.deserializeStream< ^T> stream
    /// Serialize an object to Json with the specified converter
    let inline serializeType typ (x:obj) =
        Newtonsoft.Json.JsonConvert.SerializeObject(x, typ, CompactStrictSettings.formatting, CompactStrictSettings.settings)
    /// Serialize an object to Json with the specified converter and save the result to a stream
    let inline serializeTypeToStream (typ:System.Type) (stream:System.IO.Stream) (obj:obj) =
        let serializer = JsonSerializer.Create(CompactStrictSettings.settings)
        serializer.Formatting <- CompactStrictSettings.formatting
        // Leave stream open after writing
        let DefaultStreamWriterEncoding = System.Text.UTF8Encoding.UTF8
        let DefaultStreamWriterBufferSize = 1024
        use streamWriter = new System.IO.StreamWriter(stream, DefaultStreamWriterEncoding, DefaultStreamWriterBufferSize, true)
        use jsonTextWriter = new JsonTextWriter(streamWriter)
        serializer.Serialize(jsonTextWriter, obj, typ)
    /// Serialize an object to Json with the specified converter and save the result to a file
    let inline serializeTypeToFile typ file (obj:obj) =
        use stream = System.IO.File.OpenRead(file)
        serializeTypeToStream typ stream obj
    /// Deserialize a Json to an object of type typ
    let inline deserializeType typ json =
        Newtonsoft.Json.JsonConvert.DeserializeObject(json, typ, CompactStrictSettings.settings)
    /// Deserialize a stream to an object of type typ
    let inline deserializeTypeFromStream (typ:System.Type) (stream:System.IO.Stream) =
        let serializer = Newtonsoft.Json.JsonSerializer.Create(CompactStrictSettings.settings)
        use streamReader = new System.IO.StreamReader(stream)
        use jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader)
        serializer.Deserialize(jsonTextReader, typ)
    /// Read Json from a file and desrialized it to an object of type typ
    let inline deserializeTypeFromFile typ file =
        use stream = System.IO.File.OpenRead(file)
        deserializeTypeFromStream typ stream
