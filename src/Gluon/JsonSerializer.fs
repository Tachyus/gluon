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
    open Microsoft.FSharpLu.Json
    
    type private S = Compact.TupleAsArraySettings

    /// Serialize an object to Json with the specified converter
    let serialize x = Compact.serialize x
    /// Serialize an object to Json with the specified converter and save the result to a stream
    let serializeToStream (stream:System.IO.Stream) obj = Compact.serializeToStream stream obj
    /// Serialize an object to Json with the specified converter
    let serializeType typ (x:obj) =
        Newtonsoft.Json.JsonConvert.SerializeObject(x, typ, S.formatting, S.settings)
    /// Serialize an object to Json with the specified converter and save the result to a stream
    let serializeTypeToStream (typ:System.Type) (stream:System.IO.Stream) (obj:obj) =
        let serializer = Newtonsoft.Json.JsonSerializer.Create(S.settings)
        serializer.Formatting <- S.formatting
        // Leave stream open after writing
        let DefaultStreamWriterEncoding = System.Text.UTF8Encoding.UTF8
        let DefaultStreamWriterBufferSize = 1024
        use streamWriter = new System.IO.StreamWriter(stream, DefaultStreamWriterEncoding, DefaultStreamWriterBufferSize, true)
        use jsonTextWriter = new Newtonsoft.Json.JsonTextWriter(streamWriter)
        serializer.Serialize(jsonTextWriter, obj, typ)
    /// Deserialize a Json to an object of type typ
    let deserializeType typ json =
        Newtonsoft.Json.JsonConvert.DeserializeObject(json, typ, S.settings)
    /// Deserialize a stream to an object of type typ
    let deserializeTypeFromStream (typ:System.Type) (stream:System.IO.Stream) =
        let serializer = Newtonsoft.Json.JsonSerializer.Create(S.settings)
        use streamReader = new System.IO.StreamReader(stream)
        use jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader)
        serializer.Deserialize(jsonTextReader, typ)
