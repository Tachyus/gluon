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
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Linq
open System.Linq.Expressions
open System.Reflection
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module JsonUtility =

    type FT = Reflection.FSharpType
    type FV = Reflection.FSharpValue

    let flags = Utility.bindingFlags

    let fail () =
        failwith "invalid json data"

    let ( =! ) a e =
        if a <> e then
            fail ()

    type ISerializerFactory =
        abstract CanSerialize : Type -> bool
        abstract GetSerializer<'T> : unit -> ISerializer<'T>
        abstract GetSerializer : Type -> ISerializer

    and ISerializer =
        abstract Init : ISerializerFactory -> unit
        abstract TryReadObject : JsonReader * value: byref<obj> -> bool
        abstract WriteObject : JsonWriter * value: obj -> unit
        abstract SerializedType : Type
        abstract TypedSerializer : obj

    and ISerializer<'T> =
        inherit ISerializer
        abstract TryRead : JsonReader * value: byref<'T> -> bool
        abstract Write : JsonWriter * value: 'T -> unit

    let unboxSerializer (s: ISerializer) : ISerializer<'T> =
        match s with
        | :? ISerializer<'T> as result -> result
        | _ -> unbox s.TypedSerializer

    let next (r: JsonReader) =
        r.Read() |> ignore

    let tryReadFloat (r: JsonReader) (out: byref<float>) =
        match r.TokenType with
        | JsonToken.String ->
            match unbox r.Value with
            | "Infinity" -> out <- infinity; next r; true
            | "-Infinity" -> out <- -infinity; next r; true
            | "NaN" -> out <- nan; next r; true
            | _ -> false
        | JsonToken.Float ->
            out <- unbox r.Value
            next r
            true
        | JsonToken.Integer ->
            out <- float (unbox r.Value : int64)
            next r
            true
        | _ -> false

    let tryReadInt64 (r: JsonReader) (out: byref<int64>) =
        match r.TokenType with
        | JsonToken.Integer -> out <- unbox r.Value; next r; true
        | _ -> false

    let tryReadInt32 r (out: byref<int>) =
        let mutable slot = 0L
        if tryReadInt64 r &slot then out <- int slot; true else false

    let tryReadBool (r: JsonReader) (out: byref<bool>) =
        match r.TokenType with
        | JsonToken.Boolean -> out <- unbox r.Value; next r; true
        | _ -> false

    let tryReadString (r: JsonReader) (out: byref<string>) =
        match r.TokenType with
        | JsonToken.Null -> out <- null; next r; true
        | JsonToken.String -> out <- unbox r.Value; next r; true
        | _ -> false

    let tryReadOption (elem: ISerializer<'T>) (r: JsonReader) (out: byref<option<'T>>) =
        match r.TokenType with
        | JsonToken.Null -> next r; out <- None; true
        | JsonToken.StartArray ->
            next r
            let mutable slot = Unchecked.defaultof<_>
            if elem.TryRead(r, &slot)
                then out <- Some slot
                else fail ()
            r.TokenType =! JsonToken.EndArray
            next r
            true
        | _ -> false

    let tryReadArray (elem: ISerializer<'T>) (r: JsonReader) (out: byref<'T[]>) =
        match r.TokenType with
        | JsonToken.StartArray ->
            next r
            let arr = ResizeArray()
            let mutable slot = Unchecked.defaultof<_>
            let mutable loop = true
            while loop do
                if elem.TryRead(r, &slot)
                    then arr.Add(slot)
                    else loop <- false
            r.TokenType = JsonToken.EndArray && (out <- arr.ToArray(); next r; true)
        | _ -> false

    [<AbstractClass>]
    type TypedSerializer<'T>() =

        abstract Init : ISerializerFactory -> unit
        abstract TryRead : JsonReader * value: byref<'T> -> bool
        abstract Write : JsonWriter * value: 'T -> unit

        default this.Init(_) =
            ()

        interface ISerializer<'T> with

            member this.TryRead(r, out) =
                this.TryRead(r, &out)

            member this.Write(w, x) =
                this.Write(w, x)

        interface ISerializer with

            member this.Init(factory) =
                this.Init(factory)

            member this.TryReadObject(r, value) =
                let mutable x = Unchecked.defaultof<_>
                this.TryRead(r, &x) && (value <- box x; true)

            member this.WriteObject(w, value) =
                this.Write(w, unbox value)

            member this.SerializedType = typeof<'T>
            member this.TypedSerializer = failwith "Invalid TypedSerializer invocation"

    let bytesSerializer =
        {
            new TypedSerializer<byte[]>() with
                member this.TryRead(r, out) =
                    match r.TokenType with
                    | JsonToken.String ->
                        out <- r.Value :?> string |> Convert.FromBase64String
                        next r
                        true
                    | _ -> false
                member this.Write(w, x) =
                    w.WriteValue(Seq.toArray x)
        }

    let rawJsonSerializer =
        {
            new TypedSerializer<Json>() with
                member this.TryRead(r, out) =
                    match r.TokenType with
                    | JsonToken.EndArray -> false
                    | _ ->
                        let token = JToken.ReadFrom(r)
                        out <- Json(token)
                        next r
                        true
                member this.Write(w, x) =
                    x.JToken.WriteTo(w)
        }

    let floatSerializer =
        {
            new TypedSerializer<float>() with
                member this.TryRead(r, out) = tryReadFloat r &out
                member this.Write(w, x) =
                    if Double.IsNaN(x) then
                        w.WriteValue("NaN")
                    elif Double.IsInfinity(x) then
                        if Double.IsPositiveInfinity(x) then
                            w.WriteValue("Infinity")
                        else
                            w.WriteValue("-Infinity")
                    else
                        w.WriteValue(x)
        }

    let int32Serializer =
        {
            new TypedSerializer<int>() with
                member this.TryRead(r, out) = tryReadInt32 r &out
                member this.Write(w, x) = w.WriteValue(x)
        }

    let int64Serializer =
        {
            new TypedSerializer<int64>() with
                member this.TryRead(r, out) = tryReadInt64 r &out
                member this.Write(w, x) = w.WriteValue(x)
        }

    let boolSerializer =
        {
            new TypedSerializer<bool>() with
                member this.TryRead(r, out) = tryReadBool r &out
                member this.Write(w, x) = w.WriteValue(x)
        }

    let stringSerializer =
        {
            new TypedSerializer<string>() with
                member this.TryRead(r, out) = tryReadString r &out
                member this.Write(w, x) = w.WriteValue(x)
        }

    [<Sealed>]
    type OptionSerializer<'T>() =
        inherit TypedSerializer<option<'T>>()
        let mutable inner : ISerializer<'T> = Unchecked.defaultof<_>

        override this.Init(factory) =
            inner <- factory.GetSerializer<'T>()

        override this.TryRead(r, out) =
            tryReadOption inner r &out

        override this.Write(w, x) =
            match x with
            | None -> w.WriteNull()
            | Some e ->
                w.WriteStartArray()
                inner.Write(w, e)
                w.WriteEndArray()

    [<Sealed>]
    type ArraySerializer<'T>() =
        inherit TypedSerializer<'T[]>()
        let mutable inner : ISerializer<'T> = Unchecked.defaultof<_>

        override this.Init(factory) =
            inner <- factory.GetSerializer<'T>()

        override this.TryRead(r, out) =
            tryReadArray inner r &out

        override this.Write(w, x) =
            w.WriteStartArray()
            for e in x do
                inner.Write(w, e)
            w.WriteEndArray()

    let forceRead (s: ISerializer) (json: JsonReader) =
        let mutable out = Unchecked.defaultof<obj>
        if s.TryReadObject(json, &out) then out else fail ()

    [<Sealed>]
    type StringDictSerializer<'T>() =
        inherit TypedSerializer<IDictionary<string,'T>>()
        let mutable inner : ISerializer<'T> = Unchecked.defaultof<_>

        override this.Init(factory) =
            inner <- factory.GetSerializer<'T>()

        override this.TryRead(r, out) =
            if r.TokenType = JsonToken.StartObject then
                next r
                let d = Dictionary<string,'T>()
                let rec loop () =
                    match r.TokenType with
                    | JsonToken.PropertyName ->
                        let name = r.Value :?> string
                        if name = null then
                            fail ()
                        next r
                        d.[name] <- forceRead inner r :?> 'T
                        loop ()
                    | JsonToken.EndObject ->
                        next r
                        true
                    | _ -> false
                let ok = loop ()
                if ok then
                    out <- d
                ok
            else false

        override this.Write(w, x) =
            w.WriteStartObject()
            for KeyValue(k, v) in x do
                if k = null then
                    nullArg "key"
                w.WritePropertyName(k, escape=true)
                inner.Write(w, v)
            w.WriteEndObject()

    type ConvertSerializer<'A,'B>(forward: 'A -> 'B, backward: 'B -> 'A) =
        inherit TypedSerializer<'B>()
        let mutable inner : ISerializer<'A> = Unchecked.defaultof<_>

        override this.Init(factory) =
            inner <- factory.GetSerializer<'A>()

        override this.TryRead(r, out) =
            let mutable slot = Unchecked.defaultof<_>
            if inner.TryRead(r, &slot) then out <- forward slot; true else false

        override this.Write(w, v) =
            inner.Write(w, backward v)

    let parseDateTime (txt: string) =
        DateTime.Parse(txt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)

    let printDateTime (dt: DateTime) =
        match dt.Kind with
        | DateTimeKind.Unspecified ->
            let s = dt.ToString("r", CultureInfo.InvariantCulture)
            s.Remove(s.Length - 4)
        | _ ->
            dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)

    [<Sealed>]
    type DateTimeSerializer() =
        inherit ConvertSerializer<string,DateTime>(parseDateTime, printDateTime)

    let parseDateTimeOffset (txt: string) =
        DateTimeOffset.Parse(txt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
    
    let printDateTimeOffset (ds: DateTimeOffset) =
        let time = ds.ToString("HH:mm:ss.fffffffzzz")
        let date = ds.ToString("yyyy-MM-dd")
        let full = date + "T" + time
        full

    [<Sealed>]
    type DateTimeOffsetSerializer() =
        inherit ConvertSerializer<string,DateTimeOffset>(parseDateTimeOffset, printDateTimeOffset)

    let parseGuid (txt: string) =
        Guid.Parse txt

    let printGuid (guid: Guid) =
        guid.ToString()

    [<Sealed>]
    type GuidSerializer() =
        inherit ConvertSerializer<string, Guid>(parseGuid, printGuid)

    [<Sealed>]
    type ListSerializer<'T>() =
        inherit ConvertSerializer<array<'T>,list<'T>>(List.ofArray, List.toArray)

    [<Sealed>]
    type SeqSerializer<'T>() =
        inherit ConvertSerializer<array<'T>,seq<'T>>(Seq.ofArray, Seq.toArray)

    type RecordFieldInfo =
        {
            field : PropertyInfo
            reader : obj -> obj
        }

    let getRecordFieldInfo (field: PropertyInfo) =
        let reader = FV.PreComputeRecordFieldReader(field)
        { field = field; reader = reader }

    type RecordInfo =
        {
            builder : obj[] -> obj
            fields : RecordFieldInfo[]
        }

    let getRecordInfo t =
        let fields = FT.GetRecordFields(t, flags)
        let builder = FV.PreComputeRecordConstructor(t, flags)
        { builder = builder; fields = [| for f in fields -> getRecordFieldInfo f |] }

    [<Sealed>]
    type RecordSerializer<'T>() =
        inherit TypedSerializer<'T>()
        static let info = getRecordInfo typeof<'T>
        static let arity = info.fields.Length
        let serializers : ISerializer[] = Array.zeroCreate arity

        override this.Init(factory) =
            info.fields
            |> Array.iteri (fun i f ->
                serializers.[i] <- factory.GetSerializer(f.field.PropertyType))

        override this.TryRead(r, out) =
            if r.TokenType = JsonToken.StartObject then
                next r
                let arr = Array.zeroCreate arity
                for i in 0 .. arity - 1 do
                    r.TokenType =! JsonToken.PropertyName
                    let name = r.Value :?> string
                    let j = info.fields |> Array.findIndex (fun f -> f.field.Name = name)
                    next r
                    arr.[j] <- forceRead serializers.[j] r
                out <- info.builder arr :?> 'T
                if r.TokenType = JsonToken.EndObject
                    then next r
                    else fail ()
                true
            else false

        override this.Write(w, v) =
            w.WriteStartObject()
            let value = box v
            let mutable i = 0
            for f in info.fields do
                w.WritePropertyName(f.field.Name, escape=true)
                serializers.[i].WriteObject(w, f.reader value)
                i <- i + 1
            w.WriteEndObject()

    [<Sealed>]
    type EnumSerializer<'T>() =
        inherit ConvertSerializer<int,'T>(
            (fun text -> box text :?> 'T),
            (fun enum -> box enum :?> int))

    type TupleInfo =
        {
            builder : obj [] -> obj
            elements : Type[]
            reader : obj -> obj []
        }

    let getTupleInfo (t: Type) =
        {
            builder = FV.PreComputeTupleConstructor(t)
            elements = FT.GetTupleElements(t)
            reader = FV.PreComputeTupleReader(t)
        }

    [<Sealed>]
    type TupleSerializer<'T>() =
        inherit TypedSerializer<'T>()
        static let info = getTupleInfo typeof<'T>
        static let arity = info.elements.Length
        let serializers : ISerializer[] = Array.zeroCreate arity

        override this.Init(factory) =
            info.elements
            |> Array.iteri (fun i f ->
                serializers.[i] <- factory.GetSerializer(f))

        override this.TryRead(r, out) =
            if r.TokenType = JsonToken.StartArray then
                next r
                let arr = Array.zeroCreate arity
                for i in 0 .. arity - 1 do
                    arr.[i] <- forceRead serializers.[i] r
                out <- info.builder arr :?> 'T
                if r.TokenType = JsonToken.EndArray
                    then next r
                    else fail ()
                true
            else false

        override this.Write(w, v) =
            w.WriteStartArray()
            let value = box v
            let mutable i = 0
            let values = info.reader value
            for f in info.elements do
                serializers.[i].WriteObject(w, values.[i])
                i <- i + 1
            w.WriteEndArray()

    type Field =
        {
            prop : PropertyInfo
            serializer : ISerializer
        }

    let getField (factory: ISerializerFactory) prop =
        {
            prop = prop
            serializer = factory.GetSerializer(prop.PropertyType)
        }

    type Case =
        {
            build : obj[] -> obj
            info : Reflection.UnionCaseInfo
            fields : Field []
            unpack : obj -> obj[]
        }

        member this.Arity =
            this.info.GetFields

    let getCase factory u =
        {
            build = FV.PreComputeUnionConstructor(u, flags)
            info = u
            fields =
                u.GetFields()
                |> Array.map (getField factory)
            unpack = FV.PreComputeUnionReader(u, flags)
        }

    type Union =
        {
            cases : Case[]
            getTag : obj -> int
        }

        member this.CaseByName(name) =
            this.cases
            |> Array.find (fun c -> c.info.Name = name)

        member this.TagByName(name) =
            this.cases
            |> Array.findIndex (fun c -> c.info.Name = name)

    let getUnion factory t =
        {
            cases = FT.GetUnionCases(t, flags) |> Array.map (getCase factory)
            getTag = FV.PreComputeUnionTagReader(t, flags)
        }

    [<Sealed>]
    type UnionSerializer<'T>() =
        inherit TypedSerializer<'T>()

        let mutable union : Union = Unchecked.defaultof<_>

        override this.Init(factory) =
            union <- getUnion factory typeof<'T>

        override this.TryRead(r, out) =
            match r.TokenType with
            | JsonToken.StartArray ->
                let tagName = r.ReadAsString()
                next r
                let case = union.CaseByName(tagName)
                let values =
                    case.fields
                    |> Array.map (fun f ->
                        forceRead f.serializer r)
                match r.TokenType with
                | JsonToken.EndArray -> next r
                | _ -> fail ()
                out <- case.build values :?> 'T
                true
            | _ -> false

        override this.Write(w, x) =
            let value = box x
            let tag = union.getTag value
            let case = union.cases.[tag]
            w.WriteStartArray()
            w.WriteValue(case.info.Name)
            let objs = case.unpack value
            for i in 0 .. objs.Length - 1 do
                case.fields.[i].serializer.WriteObject(w, objs.[i])
            w.WriteEndArray()

    type Serializer =
        {
            factory : ISerializerFactory
        }

    let makeSerializer<'T> (elementType: Type) =
        typedefof<'T>.MakeGenericType(elementType)
        |> Activator.CreateInstance :?> ISerializer

    let typeDependencies t =
        match Reflect.typeShape t with
        | Reflect.ListType e | Reflect.SequenceType e -> [e.MakeArrayType()]
        | Reflect.DateTimeType -> [typeof<string>]
        | shape -> Reflect.typeChildren shape
        |> Seq.ofList

    let serializerTable =
        let included : list<ISerializer> =
            [
                boolSerializer
                bytesSerializer
                floatSerializer
                int32Serializer
                rawJsonSerializer
                stringSerializer
                DateTimeSerializer()
                DateTimeOffsetSerializer()
                GuidSerializer()
            ]
        dict [for s in included -> (s.SerializedType, s)]

    let buildSerializer t =
        match Reflect.typeShape t with
        | Reflect.ArrayType t -> makeSerializer<ArraySerializer<_>> t
        | Reflect.ListType t -> makeSerializer<ListSerializer<_>> t
        | Reflect.SequenceType t -> makeSerializer<SeqSerializer<_>> t
        | Reflect.OptionType t -> makeSerializer<OptionSerializer<_>> t
        | Reflect.RecordType _ -> makeSerializer<RecordSerializer<_>> t
        | Reflect.UnionType _ -> makeSerializer<UnionSerializer<_>> t
        | Reflect.EnumType -> makeSerializer<EnumSerializer<_>> t
        | Reflect.StringDictType t -> makeSerializer<StringDictSerializer<_>> t
        | Reflect.TupleType _ -> makeSerializer<TupleSerializer<_>> t
        | Reflect.UnknownType -> failwithf "Unsupported type: %O" t
        | Reflect.BooleanType
        | Reflect.BytesType
        | Reflect.DateTimeType
        | Reflect.DateTimeOffsetType
        | Reflect.GuidType
        | Reflect.DoubleType
        | Reflect.IntType
        | Reflect.StringType
        | Reflect.JsonType -> serializerTable.[t]

    let openReader (r: JsonReader) =
        if r.TokenType = JsonToken.None then
            next r

    let read s (r: TextReader) t =
        use jr = new JsonTextReader(r, DateParseHandling = DateParseHandling.None)
        openReader jr
        let ser = s.factory.GetSerializer(t)
        forceRead ser jr

    let write s (w: TextWriter) t (v: obj) =
        use jw = new JsonTextWriter(w)
        let ser= s.factory.GetSerializer(t)
        ser.WriteObject(jw, v)

    let canSerialize s t =
        s.factory.CanSerialize(t)

    [<Sealed>]
    type Factory() =
        let all = Dictionary<Type,ISerializer>()

        member this.AddSerializer(s: ISerializer) =
            all.[s.SerializedType] <- s

        member this.AllSerializers : seq<ISerializer> =
            Seq.cast all.Values

        member this.ForType(t: Type) =
            let mutable res = Unchecked.defaultof<_>
            if all.TryGetValue(t, &res) then res else
                failwithf "No serializer for type %O" t

        interface ISerializerFactory with

            member this.CanSerialize(t: Type) =
                all.ContainsKey(t)

            member this.GetSerializer<'T>() =
                unboxSerializer (this.ForType(typeof<'T>)) : ISerializer<'T>

            member this.GetSerializer(t: Type) =
                this.ForType(t)

    let forTypes (seedTypes: seq<Type>) =
        let factory = Factory()
        for t in Utility.topSort typeDependencies seedTypes do
            let s = buildSerializer t
            factory.AddSerializer(s)
        for s in factory.AllSerializers do
            s.Init(factory)
        { factory = factory }

module Json = JsonUtility

[<Sealed>]
type JsonSerializer(s: Json.Serializer) =

    member this.CanSerialize(t: Type) =
        Json.canSerialize s t

    member this.CanSerialize<'T>() =
        this.CanSerialize(typeof<'T>)

    member this.ReadJson<'T>(r: TextReader) =
        Json.read s r typeof<'T> :?> 'T

    member this.WriteJson<'T>(w: TextWriter, value: 'T) =
        Json.write s w typeof<'T> (box value)

    member this.FromJsonString<'T>(text: string) =
        use r = new StringReader(text)
        this.ReadJson<'T>(r)

    member this.ToJsonString<'T>(value: 'T) =
        use w = new StringWriter()
        this.WriteJson<'T>(w, value)
        w.ToString()

    member this.ReadJson(t: Type, r: TextReader) =
        Json.read s r t

    member this.WriteJson(t: Type, w: TextWriter, value: obj) =
        Json.write s w t value

    member this.FromJsonString(t: Type, text: string) =
        use r = new StringReader(text)
        this.ReadJson(t, r)

    member this.ToJsonString(t: Type, value: obj) =
        use w = new StringWriter()
        this.WriteJson(t, w, value)
        w.ToString()

    static member Create(seedTypes: seq<Type>) =
        JsonSerializer(Json.forTypes seedTypes)
