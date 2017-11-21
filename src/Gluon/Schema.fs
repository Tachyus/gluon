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

namespace Gluon.Schema

open System
open System.Text
open System.Text.RegularExpressions

type DataType =
    | ArrayType of DataType
    | BooleanType
    | BytesType
    | DateTimeType
    | DateTimeOffsetType
    | GuidType
    | DoubleType
    | IntType
    | JsonType
    | ListType of DataType
    | OptionType of DataType
    | SequenceType of DataType
    | StringDictType of DataType
    | StringType
    | TupleType of list<DataType>
    | TypeReference of string

type Field =
    {
        FieldName : string
        FieldType : DataType
    }

type Record =
    {
        RecordName : string
        RecordFields : list<Field>
    }

type UnionCase =
    {
        CaseName : string
        CaseFields : list<Field>
    }

type Union =
    {
        UnionName : string
        UnionCases : list<UnionCase>
    }

type EnumCase =
    {
        EnumCaseName : string
        EnumCaseValue : int
    }

type Enum =
    {
        EnumName : string
        EnumCases : list<EnumCase>
    }

type TypeDefinition =
    | DefineEnum of Enum
    | DefineRecord of Record
    | DefineUnion of Union

type Parameter =
    {
        ParameterName : string
        ParameterType : DataType
    }

type HttpMethod =
    | Delete
    | Get
    | Post
    | Put

    override this.ToString() =
        match this with
        | Delete -> "DELETE"
        | Get -> "GET"
        | Post -> "POST"
        | Put -> "PUT"

    static member Parse(text: string) =
        match text.ToLower() with
        | "delete" -> Delete
        | "get" -> Get
        | "post" -> Post
        | "put" -> Put
        | _ -> failwithf "Not a supported HTTP method: %s" text

type CallingConvention =
    | HttpCallingConvention of HttpMethod * path: string

type Method =
    {
        CallingConvention : CallingConvention
        MethodName : string
        MethodParameters : list<Parameter>
        MethodReturnType : option<DataType>
    }

type Service =
    {
        Methods : list<Method>
        TypeDefinitions : list<TypeDefinition>
    }

type DataType with

    member this.Children =
        match this with
        | ArrayType c -> [c]
        | BooleanType
        | BytesType
        | DateTimeType
        | DateTimeOffsetType
        | GuidType
        | DoubleType
        | IntType
        | JsonType -> []
        | ListType c
        | OptionType c
        | SequenceType c
        | StringDictType c -> [c]
        | StringType -> []
        | TupleType c -> c
        | TypeReference _ -> []

    member this.WithDescendants =
        seq {
            yield this
            for c in this.Children do
                yield! c.WithDescendants
        }

    member this.AllRefs =
        this.WithDescendants
        |> Seq.choose (function
            | TypeReference r -> Some r
            | _ -> None)

type Method with

    member this.DataTypes =
        [
            for p in this.MethodParameters do
                yield p.ParameterType
            match this.MethodReturnType with
            | None -> ()
            | Some dt -> yield dt
        ]

    member this.AllRefs =
        this.DataTypes
        |> Seq.collect (fun dt -> dt.AllRefs)

type Record with
    member this.AllRefs =
        this.RecordFields
        |> Seq.collect (fun f -> f.FieldType.AllRefs)

type UnionCase with
    member this.AllRefs =
        this.CaseFields
        |> Seq.collect (fun f -> f.FieldType.AllRefs)

type Union with
    member this.AllRefs =
        this.UnionCases
        |> Seq.collect (fun c -> c.AllRefs)

type TypeDefinition with

    member this.Name =
        match this with
        | DefineEnum e -> e.EnumName
        | DefineRecord r -> r.RecordName
        | DefineUnion u -> u.UnionName
    
    member this.AllRefs =
        match this with
        | DefineRecord r -> r.AllRefs
        | DefineUnion u -> u.AllRefs
        | DefineEnum e -> Seq.empty

type SchemaError =
    | DanglingRef of string
    | InvalidMethodName of string
    | InvalidTypeName of string
    | MethodNameClash of string
    | TypeNameClash of string

    override this.ToString() =
        match this with
        | DanglingRef r -> sprintf "Dangling type reference: [%s]" r
        | InvalidMethodName n -> sprintf "Invalid method name: [%s]" n
        | InvalidTypeName n -> sprintf "Invalid type name: [%s]" n
        | MethodNameClash n -> sprintf "Method name clash: [%s]" n
        | TypeNameClash n -> sprintf "Type name clash: [%s]" n

module ServiceChecks =

    let qNamePattern =
        Regex(@"^[A-Za-z][_A-Za-z0-9]*([.][A-Za-z][_A-Za-z0-9]*)*$")

    let allRefs svc =
        Seq.concat [
            svc.Methods |> Seq.collect (fun m -> m.AllRefs)
            svc.TypeDefinitions |> Seq.collect (fun m -> m.AllRefs)
        ]

    let danglingRefs svc =
        let toSet xs =
            Seq.distinct xs
            |> Set.ofSeq
        let allRefs = toSet (allRefs svc)
        let usedRefs =
            svc.TypeDefinitions
            |> Seq.map (fun d -> d.Name)
            |> toSet
        Seq.map DanglingRef (allRefs - usedRefs)

    let clashes xs =
        xs
        |> Seq.countBy (fun x -> x)
        |> Seq.filter (fun (_, k) -> k > 1)
        |> Seq.map fst

    let methodNameClashes svc =
        svc.Methods
        |> Seq.map (fun m -> m.MethodName)
        |> clashes
        |> Seq.map MethodNameClash

    let typeNameClashes svc =
        svc.TypeDefinitions
        |> Seq.map (fun t -> t.Name)
        |> clashes
        |> Seq.map TypeNameClash

    let badMehodNames svc =
        svc.Methods
        |> Seq.map (fun t -> t.MethodName)
        |> Seq.filter (qNamePattern.IsMatch >> not)
        |> Seq.map InvalidMethodName

    let badTypeNames svc =
        svc.TypeDefinitions
        |> Seq.map (fun t -> t.Name)
        |> Seq.filter (qNamePattern.IsMatch >> not)
        |> Seq.map InvalidTypeName

    let detectErrors svc =
        seq {
            yield! danglingRefs svc
            yield! methodNameClashes svc
            yield! typeNameClashes svc
            yield! badMehodNames svc
            yield! badTypeNames svc
        }

    let check svc =
        let errors =
            detectErrors svc
            |> Seq.truncate 20
            |> Seq.map string
            |> String.concat Environment.NewLine
        if errors <> "" then
            printfn "SERVICE: %i TD and %i methods" (Seq.length svc.TypeDefinitions) (Seq.length svc.Methods)
            for td in svc.TypeDefinitions do
                printfn "typedef %s" td.Name
            failwith errors

type Service with
    member this.Namespaces =
        this.TypeDefinitions
        |> List.choose (fun def ->
            match def.Name.IndexOf('.') with
            | -1 -> None
            | n -> Some (def.Name.Substring(0, n)))
        |> List.distinct

    member this.Check() =
        ServiceChecks.check this
