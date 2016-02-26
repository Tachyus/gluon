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

let rec typeLiteral sch =
    let ( ! ) t = typeLiteral t
    match sch with
    | Schema.ArrayType t | Schema.ListType t | Schema.SequenceType t -> S.ArrayType !t
    | Schema.BooleanType -> makeType "boolean"
    | Schema.BytesType -> makeType "Uint8Array"
    | Schema.DateTimeType -> makeType "Date"
    | Schema.DoubleType | Schema.IntType -> makeType "number"
    | Schema.JsonType -> makeType "any"
    | Schema.StringType -> makeType "string"
    | Schema.OptionType t -> S.TypeReference ("Gluon.Option", [!t])
    | Schema.StringDictType t -> S.TypeReference ("Gluon.Dict", [!t])
    | Schema.TypeReference n -> makeType n
    | Schema.TupleType ts -> S.TupleType (List.map (!) ts)

let generateMatchMethod (unionDef: Schema.Union) =
    let t = "T"
    let value = "value"
    let cont = "cont"
    let tType = makeType t
    let uType = makeType unionDef.UnionName
    let cases =
        [
            for c in unionDef.UnionCases do
                let test = S.InstanceOf(S.Var value, S.Var c.CaseName)
                let args = [for x in c.CaseFields -> S.GetField (S.Var value, S.Var x.FieldName)]
                let body = S.Invoke (S.Var cont, c.CaseName, args)
                yield (test, S.Return body)
        ]
    let errorCase = S.Throw (S.New (S.Var "Error", [S.LiteralString "match failed"]))
    let contType =
        S.TypeLiteral.ObjectType [
            for c in unionDef.UnionCases ->
                let ft =
                    S.FunctionType {
                        ParameterTypes = [for v in c.CaseFields -> (v.FieldName, typeLiteral v.FieldType)]
                        ReturnType = tType
                    }
                (c.CaseName, ft)
        ]
    let body = S.Conditionals (cases, Some errorCase)
    let parameters = [(value, uType); (cont, contType)]
    S.FunctionDefinition.Create("match", body, tType, generics = [t], parameters = parameters)

let inModule name defs =
    match splitName name with
    | (Some ns, _) -> S.InModule (ns, defs)
    | _ -> defs

let promiseOf x =
    S.TypeReference ("JQueryPromise", [x])

let generateSignature (m: Schema.Method) =
    let formals = [for par in m.MethodParameters -> (par.ParameterName, typeLiteral par.ParameterType)]
    let out =
        match m.MethodReturnType with
        | None -> makeType "void"
        | Some ty -> typeLiteral ty
        |> promiseOf
    S.TypeLiteral.FunctionType {
        ParameterTypes = formals
        ReturnType = out
    }

let generateMethodStub (m: Schema.Method) =
    let (ns, name) = splitName m.MethodName
    let signature = generateSignature m
    let body = S.Call (S.Var "Gluon.Internals.remoteMethod", [signature], [S.LiteralString m.MethodName])
    let main = S.DeclareVar (name, body)
    match ns with
    | None -> main
    | Some _ -> inModule m.MethodName main

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

let generateFromJsonMethod typeRef : S.FunctionDefinition =
    let json = "json"
    let body = S.Return (S.Call (S.Var "Gluon.Internals.fromJSON", [], [S.LiteralString typeRef; S.Var json]))
    let rt = makeType (properName typeRef)
    S.FunctionDefinition.Create("fromJSON", body, rt, parameters = [(json, makeType "any")])

let generateRecordLike tRef name (fields: list<Schema.Field>) : S.ClassDefinition =
    let name = properName name
    let fields = [for f in fields -> S.ClassField.Create(f.FieldName, typeLiteral f.FieldType)]
    let ctor = S.SimpleConstructor (fields, S.EmptyStatement)
    let toJson =
        let body = S.Return (S.Call (S.Var "Gluon.Internals.toJSON", [], [S.LiteralString tRef; S.This]))
        S.FunctionDefinition.Create("toJSON", body, makeType "any")
    let tag =
        let body = S.Return (S.LiteralString name)
        S.FunctionDefinition.Create("tag", body, S.LiteralStringType name)
    let methods =
        [
            S.ClassMethod.Create(tag)
            S.ClassMethod.Create(toJson)
        ]
    S.ClassDefinition.Create(name, ctor = ctor, methods = methods)

let generateRecord (recordDef: Schema.Record) : S.Definitions =
    let n = recordDef.RecordName
    let c = generateRecordLike n n recordDef.RecordFields    
    c.WithMethod(S.ClassMethod.Create(generateFromJsonMethod n).Static())
    |> S.DefineClass

let generateUnion (unionDef: Schema.Union) : S.Definitions =
    let tRef = unionDef.UnionName
    let name = properName tRef
    let unionCases = unionDef.UnionCases
    S.DefinitionSequence [
        for c in unionCases do
            yield S.DefineClass (generateRecordLike tRef c.CaseName c.CaseFields)
        yield S.DefineTypeAlias (name, S.UnionType [for c in unionCases -> makeType c.CaseName])
        yield S.InModule (name,
                S.DefinitionSequence [
                    S.DefineFunction (generateFromJsonMethod tRef)
                    S.DefineFunction (generateMatchMethod unionDef)
                ])
    ]

let generateEnum (enumDef: Schema.Enum) =
    let eRef = enumDef.EnumName
    let name = properName eRef
    let cases = [for c in enumDef.EnumCases -> (c.EnumCaseName, c.EnumCaseValue)]
    S.EnumDefinition.Create(name, cases)
    |> S.DefineEnum

let typeDef def =
    match def with
    | Schema.DefineEnum enu ->
        generateEnum enu
        |> inModule enu.EnumName
    | Schema.DefineRecord re ->
        generateRecord re
        |> inModule re.RecordName
    | Schema.DefineUnion u ->
        generateUnion u
        |> inModule u.UnionName

let builderLambda (name: string) (n: int) =
    let alphabet = "abcdefghijklmnopqrstuvwxyz"
    let letter i = string alphabet.[i]
    let letters = [for i in 0 .. n - 1 -> letter i]
    S.SimpleLambda (letters, S.New (S.Var name, [for l in letters -> S.Var l]))

let registerActivators typeDefs =
    let args =
        S.LiteralObject [
            for ty in typeDefs do
                match ty with
                | Schema.DefineEnum enu -> ()
                | Schema.DefineRecord def ->
                    let name = def.RecordName
                    let fields = def.RecordFields
                    yield (name, builderLambda name fields.Length)
                | Schema.DefineUnion def ->
                    let (ns, _) = splitName def.UnionName
                    let cases = def.UnionCases
                    for c in cases do
                        let name =
                            match ns with
                            | None -> c.CaseName
                            | Some ns -> sprintf "%s.%s" ns c.CaseName
                        yield (name, builderLambda name c.CaseFields.Length)
        ]
    S.Call (S.Var "Gluon.Internals.registerActivators", [], [args])
    |> S.Action

let typeDefinitions typeDefs =
    S.DefinitionSequence [for t in typeDefs -> typeDef t]

let methodStubs (svc: Schema.Service) =
    S.DefinitionSequence [for m in svc.Methods -> generateMethodStub m]
