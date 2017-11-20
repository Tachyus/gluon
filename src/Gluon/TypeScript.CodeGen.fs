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

module Gluon.TypeScript.CodeGen

open Gluon
module S = Syntax

let splitName (name: string) =
    if name.Contains(".") then
        let i = name.LastIndexOf('.')
        let prefix = name.Substring(0, i)
        let proper = name.Substring(i + 1)
        (Some prefix, proper)
    else
        (None, name)

let makeType t =
    S.TypeReference (t, [])

let makeOptionType t =
    S.TypeReference ("Gluon.Option", [t])

let rec typeLiteralNs ns sch =
    let ( ! ) t = typeLiteralNs ns t
    match sch with
    | Schema.ArrayType t | Schema.ListType t | Schema.SequenceType t -> S.ArrayType !t
    | Schema.BooleanType -> makeType "boolean"
    | Schema.BytesType -> makeType "Uint8Array"
    | Schema.DateTimeType -> makeType "Date"
    | Schema.DateTimeOffsetType -> makeType "Date"
    | Schema.DoubleType | Schema.IntType -> makeType "number"
    | Schema.JsonType -> makeType "any"
    | Schema.StringType -> makeType "string"
    | Schema.OptionType t -> makeOptionType (!t)
    | Schema.StringDictType t -> S.TypeReference ("Gluon.Dict", [!t])
    | Schema.TypeReference n ->
        let isGenerated = ns |> List.exists (fun x -> n.StartsWith x)
        makeType (if isGenerated then "_"+n else n)
    | Schema.TupleType ts -> S.TupleType (List.map (!) ts)

let typeLiteral sch = typeLiteralNs [] sch

let rec nestNamespaces defs cont namespaces =
    match namespaces with
    | [] -> cont defs
    | ns::rest -> nestNamespaces defs (fun inner -> cont (S.InNamespace (ns, inner))) rest

let inNamespace name defs =
    match splitName name with
    | (Some ns, _) ->
        ns.Split('.') |> Array.toList |> nestNamespaces defs id
    | _ -> defs

let promiseOf x =
    S.TypeReference ("Promise", [makeOptionType x])

let generateSignature ns (m: Schema.Method) =
    let formals = [for par in m.MethodParameters -> (par.ParameterName, typeLiteralNs ns par.ParameterType)]
    let out =
        match m.MethodReturnType with
        | None -> makeType "void"
        | Some ty -> typeLiteralNs ns ty
        |> promiseOf
    S.TypeLiteral.FunctionType {
        ParameterTypes = formals
        ReturnType = out
    }

let generateMethodStub namespaces (m: Schema.Method) =
    let (ns, name) = splitName m.MethodName
    let signature = generateSignature namespaces m
    let body = S.Call (S.Var "Gluon.Internals.remoteMethod", [signature], [S.LiteralString m.MethodName])
    let main = S.DeclareVar (name, body)
    match ns with
    | None -> main
    | Some _ -> inNamespace m.MethodName main

let literalJson (value: 'T) =
    let ser = JsonSerializer.Create([typeof<'T>])
    S.LiteralJson (ser.ToJsonString(value))

let registerTypeDefinitions (typeDefs: seq<Schema.TypeDefinition>) =
    let typeDefs = Seq.toArray typeDefs
    S.Call (S.Var "Gluon.Internals.registerTypeDefinitions", [], [literalJson typeDefs])
    |> S.Action

let registerService (svc: Schema.Service) =
    S.Call (S.Var "Gluon.Internals.registerService", [], [literalJson svc])
    |> S.Action

let properName name =
    snd (splitName name)

let rec onlyProperName (name: string) =
    match splitName name with
    | Some _, proper -> onlyProperName proper
    | None, proper -> proper

let generateFromJsonMethod typeRef : S.FunctionDefinition =
    let json = "json"
    let body = S.Return (S.Call (S.Var "Gluon.Internals.fromJSON", [], [S.LiteralString typeRef; S.Var json]))
    let rt = makeType (properName typeRef)
    S.FunctionDefinition.Create("fromJSON", body, rt, parameters = [(json, makeType "any")])

let generateRecordLike ns tRef name (fields: list<Schema.Field>) : S.ClassDefinition =
    let name = properName name
    let fields = [for f in fields -> S.ClassField.Create(f.FieldName, typeLiteralNs ns f.FieldType)]
    let ctor = S.SimpleConstructor (fields, S.EmptyStatement)
    let toJson =
        let body = S.Return (S.Call (S.Var "Gluon.Internals.toJSON", [], [S.LiteralString tRef; S.This]))
        S.FunctionDefinition.Create("toJSON", body, makeType "any")
    let methods =
        [
            S.ClassMethod.Create(toJson)
        ]
    S.ClassDefinition.Create(name, ctor = ctor, methods = methods)

let generateRecord ns (recordDef: Schema.Record) : S.Definitions =
    let n = recordDef.RecordName
    let c = generateRecordLike ns n n recordDef.RecordFields    
    c.WithMethod(S.ClassMethod.Create(generateFromJsonMethod n).Static())
    |> S.DefineClass

let generateUnionCase ns name (fields: Schema.Field list) =
    let unionFields =
        [ for f in fields -> S.UnionCaseField.Create(f.FieldName, typeLiteralNs ns f.FieldType) ]
    S.UnionCaseDefinition.Create(name, unionFields)

let generateUnion ns (unionDef: Schema.Union) : S.Definitions =
    let tRef = unionDef.UnionName
    let name = properName tRef
    let unionCases = unionDef.UnionCases
    let shouldGenerateStringLiterals =
        unionCases |> List.forall (fun c -> List.isEmpty c.CaseFields)
    S.DefinitionSequence [
        if shouldGenerateStringLiterals then
            yield S.DefineTypeAlias (name, S.UnionType [for c in unionCases -> S.LiteralStringType c.CaseName])
            yield S.InNamespace (name,
                    S.DefinitionSequence [
                        S.DefineFunction (generateFromJsonMethod tRef)
                    ])
        else
            for c in unionCases do
                yield S.DefineUnionCase (generateUnionCase ns c.CaseName c.CaseFields)
            yield S.DefineTypeAlias (name, S.UnionType [for c in unionCases -> makeType c.CaseName])
            yield S.InNamespace (name,
                    S.DefinitionSequence [
                        S.DefineFunction (generateFromJsonMethod tRef)
                    ])
    ]

let generateEnum (enumDef: Schema.Enum) =
    let eRef = enumDef.EnumName
    let name = properName eRef
    let cases = [for c in enumDef.EnumCases -> (c.EnumCaseName, c.EnumCaseValue)]
    S.EnumDefinition.Create(name, cases)
    |> S.DefineEnum

let typeDef namespaces def =
    match def with
    | Schema.DefineEnum enu ->
        generateEnum enu
        |> inNamespace enu.EnumName
    | Schema.DefineRecord re ->
        generateRecord namespaces re
        |> inNamespace re.RecordName
    | Schema.DefineUnion u ->
        generateUnion namespaces u
        |> inNamespace u.UnionName

let builderLambda (name: string) (fields: Schema.Field list) =
    let n = fields.Length
    let alphabet = "abcdefghijklmnopqrstuvwxyz"
    let letter i =
        let o = i % alphabet.Length
        let n = (i - o) / alphabet.Length
        match n with
        | 0 -> string alphabet.[o]
        | n -> sprintf "%c%i" alphabet.[o] n
    let letters = [for i in 0 .. n - 1 -> letter i]
    let letteredFields = List.zip fields letters
    let parameters = letteredFields |> List.map (fun (f, a) -> a, typeLiteral f.FieldType)
    S.SimpleLambda (parameters, S.New (S.Var name, [for l in letters -> S.Var l]))

let unionCaseLambda (name: string) (fields: Schema.Field list) =
    let alphabet = "abcdefghijklmnopqrstuvwxyz"
    let letter i =
        let o = i % alphabet.Length
        let n = (i - o) / alphabet.Length
        match n with
        | 0 -> string alphabet.[o]
        | n -> sprintf "%c%i" alphabet.[o] n
    let letters = [for i in 0 .. fields.Length - 1 -> letter i]
    let letteredFields = List.zip fields letters
    if List.isEmpty letteredFields then
        // This is a string literal union
        let body = S.LiteralString (onlyProperName name)
        S.SimpleLambda ([], body)
    else
        let parameters = letteredFields |> List.map (fun (f, a) -> a, typeLiteral f.FieldType)
        let body =
            S.LiteralObject [
                yield "tag", S.LiteralString (onlyProperName name)
                for f, l in letteredFields do
                    yield f.FieldName, S.Var l
            ]
        S.SimpleLambda (parameters, S.Cast (S.Var name, body))

let registerActivators typeDefs =
    let args =
        S.LiteralObject [
            for ty in typeDefs do
                match ty with
                | Schema.DefineEnum enu -> ()
                | Schema.DefineRecord def ->
                    let name = def.RecordName
                    let fields = def.RecordFields
                    yield (name, builderLambda name fields)
                | Schema.DefineUnion def ->
                    let (ns, _) = splitName def.UnionName
                    let cases = def.UnionCases
                    for c in cases do
                        let name =
                            match ns with
                            | None -> c.CaseName
                            | Some ns -> sprintf "%s.%s" ns c.CaseName
                        yield (name, unionCaseLambda name c.CaseFields)
        ]
    S.Call (S.Var "Gluon.Internals.registerActivators", [], [args])
    |> S.Action

let typeDefinitions (svc: Schema.Service) =
    S.DefinitionSequence [for t in svc.TypeDefinitions -> typeDef svc.Namespaces t]

let methodStubs (svc: Schema.Service) =
    S.DefinitionSequence [for m in svc.Methods -> generateMethodStub svc.Namespaces m]
