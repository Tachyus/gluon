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

module Gluon.TypeScript.PrettyPrinter

open System
open System.Web
open Gluon
module PP = PrettyPrint
open PP.Operators
module S = Syntax

let t x =
    PP.text x

let many combine layouts =
    let layouts = Seq.toList layouts
    match layouts with
    | [] -> PP.empty
    | [x] -> x
    | _ -> List.reduce combine layouts

let blockLike openParen closeParen layout =
    PP.group (
        openParen +.
        PP.indent 2 (PP.line "" +. layout) +.
        PP.line "" +.
        closeParen
    )

let parens layout =
    PP.group (PP.text "(" +. layout +. PP.text ")")

let angular layout =
    PP.group (PP.text "<" +. layout +. PP.text ">")

let braces layout =
    blockLike (PP.text "{") (PP.text "}") layout

let brackets layout =
    blockLike (PP.text "[") (PP.text "]") layout
    
let commas layouts =
    let combine x y = x +. PP.text "," +/ y
    PP.group (many combine layouts)

let commasTight layouts =
    let combine x y = x +. t "," ++ y
    many combine layouts

let block x =
    PP.group (PP.indent 2 (PP.line " " +. x))

let vertical layouts =
    let combine x y = x +/ y
    many combine layouts

let horizontal layouts =
    let combine x y = x ++ y
    many combine layouts

let spaces layouts =
    let combine x y = x +/ y
    PP.group (many combine layouts)

let rec expression e =
    let ( ! ) x = expression x
    let ( !! ) xs = xs |> Seq.map expression |> Seq.toArray
    match e with
    | S.GetField (a, S.Var v) -> !a +. t "." +. t v
    | S.GetField (a, b) -> !a +. brackets !b
    | S.New (x, args) -> t "new" ++ !x +. parens (commasTight !!args)
    | S.InstanceOf (a, b) -> !a ++ t "instanceof" ++ !b
    | S.Call (func, [], args) -> !func +. parens (commasTight !!args)
    | S.Call (func, gen, args) -> !func +. angular (commasTight [for g in gen -> typeLiteral g]) +. parens (commasTight !!args)
    | S.Invoke (o, m, args) -> !o +. t "." +. t m +. parens (commasTight !!args)
    | S.LiteralString str -> HttpUtility.JavaScriptStringEncode(str, addDoubleQuotes = true) |> t
    | S.Var v -> t v
    | S.LiteralJson x -> t x
    | S.LiteralObject fields -> braces (commas [for (n, v) in fields -> !(S.LiteralString n) +. t ":" ++ !v])
    | S.SimpleLambda (args, body) -> parens (commasTight [for (a, v) in args -> t a +. t ":" ++ typeLiteral v]) ++ t "=>" ++ !body
    | S.This -> t "this"
    | S.Cast (a, b) -> t "<" +. expression a +. t ">" +. expression b

and typeLiteral tl =
    match tl with
    | S.LiteralStringType str ->
        expression (S.LiteralString str)
    | S.FunctionType ft ->
        let argTypes =
            commas [
                for (n, ty) in ft.ParameterTypes ->
                    t n +. t ": " ++ typeLiteral ty
            ]
        parens (parens argTypes ++ t "=>" ++ typeLiteral ft.ReturnType)
    | S.ObjectType xs ->
        [for (a, b) in xs -> t a +. t ": " +. typeLiteral b]
        |> commas
        |> braces 
    | S.TypeReference (id, []) -> t id
    | S.TypeReference (id, args) ->
        t id +. angular (horizontal [for a in args -> typeLiteral a])
    | S.ArrayType e -> typeLiteral e ++ t "[]"
    | S.UnionType ts ->
        let combine a b = a ++ t "|" ++ b
        many combine (List.map typeLiteral ts)
    | S.TupleType ts ->
        brackets (commas (List.map typeLiteral ts))

let rec statement (s: S.Statement) : PP.Layout =
    match s with
    | S.Conditionals (many, last) ->                
        let main =
            many
            |> Seq.mapi (fun i (c, b) ->
                let head = if i = 0 then t "if" else (t "else" ++ t "if")
                head ++ parens (expression c) ++ braces (block (statement b)))
            |> Seq.toArray
            |> vertical
        match last with
        | None -> main
        | Some b -> main ++ t "else" ++ braces (block (statement b))
    | S.Return e -> t "return" ++ expression e +. t ";"
    | S.Throw e -> t "throw" ++ expression e +. t ";"
    | S.EmptyStatement -> PP.empty

let methodLike (f: S.FunctionDefinition) =
    let generics =
        if not (Seq.isEmpty f.Generics) then
            angular (commas [for g in f.Generics -> t g])
        else
            PP.empty
    let argsig =
        commas [
            for (n, ty) in f.Parameters ->
                t n +. t ":" ++ typeLiteral ty
        ]
        |> parens
    let body = braces (block (statement f.Body))
    let rsig = t ":" ++ typeLiteral f.ReturnType
    t f.Name +. generics +. argsig +. rsig ++ body

let functionDefinition f =
    t "function" ++ methodLike f

let layoutTag (tag: string) : PP.Layout =
    t "tag" +. t ":" ++ t (HttpUtility.JavaScriptStringEncode(tag, addDoubleQuotes = true)) +. t ";"

let unionCaseDefinition (u: S.UnionCaseDefinition) : PP.Layout =
    let body =
        vertical [
            yield layoutTag u.UnionCaseName
            for { FieldName = a; FieldType = b } in u.Fields do
                yield t a +. t ": " +. typeLiteral b
        ]
    t "interface" ++ t u.UnionCaseName ++ braces (block body)

let layoutConstructor (c: S.Constructor) : PP.Layout =
    match c with
    | S.SimpleConstructor (fields, body) ->
        let argsig =
            commas [
                for f in fields ->
                    let modif = if f.IsPublic then t "public" else PP.empty
                    modif ++ t f.FieldName +. t ":" ++ typeLiteral f.FieldType
            ]
            |> parens
        t "constructor" +. argsig ++ braces (block (statement body))

let layoutClassMethod (cm: S.ClassMethod) =
    if cm.IsStatic
    then t "static" ++ methodLike cm.FunctionDefinition
    else methodLike cm.FunctionDefinition

let classDefinition (cl: S.ClassDefinition) =
    let body =
        vertical [
            yield layoutConstructor cl.Constructor
            for m in cl.Methods do
                yield layoutClassMethod m
        ]
    t "class" ++ t cl.ClassName ++ braces (block body)

let enumDefinition (enu: S.EnumDefinition) =
    let body =
        commas [
            for (c, v) in enu.EnumCases ->
                t c ++ t "=" ++ t (string v)
        ]
    t "enum" ++ t enu.EnumName ++ braces (block body)

let definitions (defs: S.Definitions) =
    let defs = defs.GroupModules()
    let rec definitions inModule defs =
        let prefix = if inModule then t "export" else PP.empty
        match defs with
        | S.Action e -> expression e +. t ";"
        | S.Comment s -> t (sprintf "// %s" s)
        | S.DefinitionSequence xs -> vertical [for d in xs -> definitions inModule d]
        | S.DeclareVar (name, expr) -> prefix ++ t "var" ++ t name ++ t "=" ++ expression expr +. t ";"
        | S.DefineTypeAlias (name, lit) -> prefix ++ t "type" ++ t name ++ t "=" ++ typeLiteral lit +. t ";"
        | S.InModule (mname, defs) -> prefix ++ t "module" ++ t mname ++ braces (block (definitions true defs))
        | S.DefineClass c -> prefix ++ classDefinition c
        | S.DefineEnum e -> prefix ++ enumDefinition e
        | S.DefineFunction f -> prefix ++ functionDefinition f
        | S.DefineUnionCase u -> prefix ++ unionCaseDefinition u
    definitions false defs
