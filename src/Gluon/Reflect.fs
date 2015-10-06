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
open System.IO
open System.Linq
open System.Linq.Expressions
open System.Reflection

module Reflect =

    type E = Expression
    type FT = Reflection.FSharpType
    type FV = Reflection.FSharpValue

    let flags = Utility.bindingFlags

    type FieldShape =
        {
            FieldName :string
            FieldType : Type
        }

    type RecordShape =
        {
            RecordFields : list<FieldShape>
        }

    type CaseShape =
        {
            CaseName : string
            CaseFields : list<FieldShape>
        }

    type UnionShape =
        {
            Cases : list<CaseShape>
        }

    type TypeShape =
        | ArrayType of Type
        | BooleanType
        | BytesType
        | DateTimeType
        | DoubleType
        | EnumType
        | IntType
        | JsonType
        | ListType of Type
        | OptionType of Type
        | RecordType of RecordShape
        | SequenceType of Type
        | StringDictType of Type
        | StringType
        | TupleType of list<Type>
        | UnionType of UnionShape
        | UnknownType

    let ( => ) a b = (a, b)

    let tyTable =
        dict [
            typeof<bool> => BooleanType
            typeof<byte[]> => BytesType
            typeof<DateTime> => DateTimeType
            typeof<double> => DoubleType
            typeof<int> => IntType
            typeof<Json> => JsonType
            typeof<string> => StringType
        ]

    let tyCon1Table =
        dict [
            typedefof<list<_>> => ListType
            typedefof<option<_>> => OptionType
            typedefof<seq<_>> => SequenceType
        ]

    let reflectRecord ty =
        RecordType {
            RecordFields =
                [
                    for field in FT.GetRecordFields(ty, flags) ->
                        {
                            FieldName = field.Name
                            FieldType = field.PropertyType
                        }
                ]
        }

    let reflectUnion ty =
        let cases =
            [
                for case in FT.GetUnionCases(ty, flags) ->
                    let caseFields =
                        [
                            for field in case.GetFields() ->
                                {
                                    FieldName = field.Name
                                    FieldType = field.PropertyType
                                }
                        ]
                    { CaseName = case.Name; CaseFields = caseFields }
            ]
        UnionType { Cases = cases }

    let (|GenericType|_|) (t: Type) =
        if t.IsGenericType then
            let args = t.GetGenericArguments() |> List.ofArray
            Some (t.GetGenericTypeDefinition(), args)
        else None

    let typeShape t =
        match tyTable.TryGetValue(t) with
        | true, s -> s
        | _ ->
            match t with
            | t when t.IsEnum && Enum.GetUnderlyingType(t) = typeof<int> ->
                EnumType
            | t when t.IsArray && t.GetArrayRank() = 1 ->
                ArrayType (t.GetElementType())
            | t when FT.IsTuple(t) ->
                let el = FT.GetTupleElements(t)
                if el.Length > 1
                    then TupleType (List.ofArray el)
                    else UnknownType
            | GenericType (d, [str; t]) when
                str = typeof<string>
                && d = typedefof<IDictionary<_,_>> ->
                    StringDictType t
            | GenericType (d, [x]) ->
                match tyCon1Table.TryGetValue(d) with
                | true, def -> def x
                | _ -> UnknownType
            | GenericType _ -> UnknownType
            | t when FT.IsUnion(t, flags) -> reflectUnion t
            | t when FT.IsRecord(t, flags) -> reflectRecord t
            | _ -> UnknownType

    let typeChildren shape =
        match shape with
        | ArrayType t | OptionType t | SequenceType t
        | ListType t | StringDictType t -> [t]
        | TupleType ts -> ts
        | RecordType sh -> [for f in sh.RecordFields -> f.FieldType]
        | UnionType u ->
            [
                for c in u.Cases do
                    for f in c.CaseFields do
                        yield f.FieldType
            ]
        | BytesType
        | EnumType
        | JsonType
        | StringType
        | UnknownType
        | BooleanType
        | DateTimeType
        | DoubleType
        | IntType -> []

    let typeId (ty: Type) =
        ty.FullName.Replace('+', '.')

    type DataTypeContext =
        | TopLevelContext of Type
        | GenericArgumentContext of DataTypeContext * Type * position: int

    let rec explain ctx =
        match ctx with
        | TopLevelContext t -> [sprintf ".. while reflecting %O" t]
        | GenericArgumentContext (ctx, ty, p) ->
            sprintf ".. in position %i of type %O" p ty :: explain ctx

    type DataTypeKind =
        | SupportedDataType of Schema.DataType
        | IllegalDataType of Type * DataTypeContext

    let classifyDataType (ty: Type) : DataTypeKind =
        let rec classify (k: Schema.DataType -> DataTypeKind) (ctx: DataTypeContext) (t: Type) : DataTypeKind =
            let gen n = GenericArgumentContext (ctx, t, position = n)
            match typeShape t with
            | ArrayType ty -> classify (Schema.ArrayType >> k) (gen 0) ty
            | BooleanType -> k Schema.BooleanType
            | BytesType -> k Schema.BytesType
            | DateTimeType -> k Schema.DateTimeType
            | DoubleType -> k Schema.DoubleType
            | IntType -> k Schema.IntType
            | JsonType -> k Schema.JsonType
            | ListType ty -> classify (Schema.ListType >> k) (gen 0) ty
            | OptionType ty -> classify (Schema.OptionType >> k) (gen 0) ty
            | SequenceType ty -> classify (Schema.SequenceType >> k) (gen 0) ty
            | StringType -> k Schema.StringType
            | StringDictType ty -> classify (Schema.StringDictType >> k) (gen 1) ty
            | TupleType ts ->
                let args = ts |> List.mapi (fun i ty -> classify SupportedDataType (gen i) ty)
                let supported = args |> List.choose (function SupportedDataType t -> Some t | _ -> None)
                if supported.Length = args.Length
                    then k (Schema.TupleType supported)
                    else args |> List.pick (function IllegalDataType _ as r -> Some r | _ -> None)
            | EnumType _ | RecordType _ | UnionType _ ->
                k (Schema.TypeReference (typeId t))
            | UnknownType -> IllegalDataType (t, ctx)
        classify SupportedDataType (TopLevelContext ty) ty

    [<Sealed>]
    type DatatypeNotSupportedException(t: Type, ctx: DataTypeContext) =
        inherit exn(
            sprintf "Not a supported datatype: %s. Context: %s"
                t.AssemblyQualifiedName
                (String.concat Environment.NewLine (explain ctx))
        )

    let getDataType t =
        match classifyDataType t with
        | SupportedDataType t -> t
        | IllegalDataType (t, ctx) -> raise (DatatypeNotSupportedException(t, ctx))

    let (|VoidType|AsyncType|OtherType|) (t: Type) =
        if t = typeof<Void> || t = typeof<unit> then
            VoidType
        elif t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Async<_>> then
            AsyncType (t.GetGenericArguments().[0])
        else
            OtherType

    let getMethodNameForRuntimeMethod (info: MethodInfo) =
        let dt = info.DeclaringType.FullName.Replace('+', '.')
        sprintf "%s.%s" dt info.Name

    let getParameterSchema (p: ParameterInfo) : Schema.Parameter =
        {
            ParameterName = p.Name
            ParameterType = getDataType p.ParameterType
        }

    let getReturnTypeSchema (info: MethodInfo) : option<Schema.DataType> =
        match info.ReturnType with
        | AsyncType VoidType -> None
        | AsyncType t -> Some (getDataType t)
        | VoidType -> None
        | OtherType as t -> Some (getDataType t)

    let getRemoteAttr (info: MethodInfo) =
        if Attribute.IsDefined(info, typeof<RemoteAttribute>) then
            Attribute.GetCustomAttribute(info, typeof<RemoteAttribute>)
            :?> RemoteAttribute |> Some
        else None

    let pickPathBasedOnName (info: MethodInfo) =
        sprintf "%s/%s" info.DeclaringType.Name info.Name

    let getCallingConvention info =
        let attr = getRemoteAttr info
        let verb =
            match attr with
            | None -> Schema.HttpMethod.Post
            | Some attr -> Schema.HttpMethod.Parse(attr.Verb)
        let path =
            match attr with
            | Some attr when attr.Path <> null -> attr.Path
            | _ -> pickPathBasedOnName info
        Schema.HttpCallingConvention(verb, path)

    let hasContextParameter (runtimeMethod: MethodInfo) =
        let ps = runtimeMethod.GetParameters()
        ps.Length > 0 && ps.[0].ParameterType = typeof<Context>

    let getParametersWithoutContext (runtimeMethod: MethodInfo) =
        let ps = runtimeMethod.GetParameters()
        if ps.Length > 0 && ps.[0].ParameterType = typeof<Context>
            then Seq.toArray (Seq.skip 1 ps)
            else ps

    let getSchemaFromRuntimeMethod (info: MethodInfo) : Schema.Method =
        if info.IsConstructor || not info.IsStatic then
            failwith "Instance methods and constructors are not supported"
        if info.ContainsGenericParameters then
            failwith "Methods with free generic parameters are not supported"
        if info.IsGenericMethod then
            failwith "Generic methods are not supported"
        try
            {
                CallingConvention = getCallingConvention info
                MethodName = getMethodNameForRuntimeMethod info
                MethodParameters = [for p in getParametersWithoutContext info -> getParameterSchema p]
                MethodReturnType = getReturnTypeSchema info
            }
        with :? DatatypeNotSupportedException as e ->
            failwithf "Cannot reflect method %O: %O" info e

    let extractFromTuple (tupleExpr: E) (position: int) : E =
        let rec loop (tupleType: Type) (tupleExpr: E) (pos: int) =
            let (prop, rest) = FV.PreComputeTuplePropertyInfo(tupleType, position)
            let tupleExpr = E.Property(tupleExpr, prop) :> E
            match rest with
            | Some (tupleType, pos) -> loop tupleType tupleExpr pos
            | None -> tupleExpr
        loop tupleExpr.Type tupleExpr position

    type BoxedMethod =
        {
            Invoke : Context * obj -> Async<obj>
        }

    type IReturnAdapter =
        abstract Adapt : rawFunc: obj -> BoxedMethod

    [<Sealed>]
    type ActionAdapter() =
        interface IReturnAdapter with
            member this.Adapt(rawFunc) =
                let f = rawFunc :?> Action<Context,obj>
                { Invoke = fun (c, x) -> async {
                    do f.Invoke(c, x)
                    return box () }}

    [<Sealed>]
    type FuncAdapter<'T>() =
        interface IReturnAdapter with
            member this.Adapt(rawFunc) =
                let f = rawFunc :?> Func<Context,obj,'T>
                { Invoke = fun (c, x) -> async {
                    let result = f.Invoke(c, x)
                    return box result }}

    [<Sealed>]
    type AsyncFuncAdapter<'T>() =
        interface IReturnAdapter with
            member this.Adapt(rawFunc) =
                let f = rawFunc :?> Func<Context,obj,Async<'T>>
                { Invoke = fun (c, x) -> async {
                    let! result = f.Invoke(c, x)
                    return box result }}

    let callWithContext (m: MethodInfo) (ctx: E) (args: list<E>) =
        if hasContextParameter m
            then E.Call(m, ctx :: args) :> E
            else E.Call(m, args) :> E

    let buildExpression runtimeMethod =
        let ps = getParametersWithoutContext runtimeMethod
        let context = E.Parameter(typeof<Context>, "context")
        let input = E.Parameter(typeof<obj>, "input")
        match ps.Length with
        | 0 ->
            let body = callWithContext runtimeMethod context []
            E.Lambda(body, [| context; input |])
        | 1 ->
            let t = ps.[0].ParameterType
            let arg =
                if t.IsValueType
                    then E.Unbox(input, t) :> E
                    else E.TypeAs(input, t) :> E
            let body = callWithContext runtimeMethod context [arg]
            E.Lambda(body, [| context; input |])
        | n ->
            let tupleType = FT.MakeTupleType [| for p in ps -> p.ParameterType |]
            let tupleVar = E.Variable(tupleType, "tupleVar")
            let args = [for i in 0 .. n - 1 -> extractFromTuple tupleVar i]
            let body =
                E.Block([tupleVar],
                    [
                        E.RuntimeVariables([tupleVar]) :> E
                        E.Assign(tupleVar, E.TypeAs(input, tupleType)) :> E
                        callWithContext runtimeMethod context args
                    ])
            E.Lambda(body, [| context; input |])

    let adaptRuntimeMethod (runtimeMethod: MethodInfo) =
        let e = buildExpression runtimeMethod
        let adapter =
            match e.ReturnType with
            | AsyncType t ->
                typedefof<AsyncFuncAdapter<_>>.MakeGenericType(t)
                |> Activator.CreateInstance :?> IReturnAdapter
            | VoidType ->
                ActionAdapter() :> IReturnAdapter
            | OtherType as t ->
                typedefof<FuncAdapter<_>>.MakeGenericType(t)
                |> Activator.CreateInstance :?> IReturnAdapter
        e.Compile()
        |> adapter.Adapt

    type IOTypes =
        {
            InputType : option<Type>
            OutputType : option<Type>
        }

        member this.All =
            match this.InputType, this.OutputType with
            | Some i, Some o -> [i; o]
            | Some x, None | None, Some x -> [x]
            | None, None-> []

        static member Create<'In,'Out>() =
            let c t =
                if t = typeof<unit> then None else Some t
            {
                InputType = c typeof<'In>
                OutputType = c typeof<'Out>
            }

    let ioTypes (info: MethodInfo) =
        let inType =
            match getParametersWithoutContext info with
            | a when a.Length = 0 -> None
            | a when a.Length = 1 -> Some a.[0].ParameterType
            | ps ->
                let ts = [| for p in ps -> p.ParameterType |]
                FT.MakeTupleType(ts) |> Some
        let outType =
            match info.ReturnType with
            | AsyncType VoidType -> None
            | AsyncType t -> Some t
            | OtherType as t -> Some t
            | VoidType -> None
        { InputType = inType; OutputType = outType }

    let walkTypes (seedTypes: seq<Type>) =
        let dependsOn (t: Type) =
            typeChildren (typeShape t)
            |> Seq.ofList
        Utility.topSort dependsOn seedTypes

    let makeField f : Schema.Field =
        { FieldName = f.FieldName; FieldType = getDataType f.FieldType }

    let recordSchema (t: Type) (r: RecordShape) : Schema.Record =
        try
            {
                RecordName = typeId t
                RecordFields = [for f in r.RecordFields -> makeField f]
            }
        with :? DatatypeNotSupportedException as e ->
            failwithf "Cannot serialize %s: %O" t.FullName e

    let unionSchema (t: Type) (u: UnionShape) : Schema.Union =
        try
            {
                UnionName = typeId t
                UnionCases =
                    [
                        for c in u.Cases ->
                            {
                                CaseName = c.CaseName
                                CaseFields = [for f in c.CaseFields -> makeField f]
                            }
                    ]
            }
        with :? DatatypeNotSupportedException as e ->
            failwithf "Cannot serialize %s: %O" t.FullName e

    let enumSchema (t: Type) : Schema.Enum =
        {
            EnumName = typeId t
            EnumCases =
                let names = Enum.GetNames(t)
                let values = Enum.GetValues(t) |> Seq.cast<int> |> Seq.toArray
                (names, values)
                ||> Array.map2 (fun n v ->
                    let c : Schema.EnumCase =
                        {
                            EnumCaseName = n
                            EnumCaseValue = v
                        }
                    c)
                |> Array.toList
        }

    let (|TypeDef|_|) ty =
        match typeShape ty with
        | UnionType u -> Schema.DefineUnion (unionSchema ty u) |> Some
        | RecordType r -> Schema.DefineRecord (recordSchema ty r) |> Some
        | EnumType -> Schema.DefineEnum (enumSchema ty) |> Some
        | _ -> None

    let findRemoteMethods (asm: Assembly) =
        let flags =
            BindingFlags.Static
            |||BindingFlags.Public
            |||BindingFlags.NonPublic
        seq {
            for t in asm.GetTypes() do
                for m in t.GetMethods(flags) do
                    let isRemote =
                        not m.IsConstructor
                        && Attribute.IsDefined(m, typeof<RemoteAttribute>)
                    if isRemote then
                        yield m
        }
