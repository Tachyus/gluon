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

/// Simple syntax tree to assist with code generation.
module internal Gluon.TypeScript.Syntax

type TypeLiteral =
    | ArrayType of TypeLiteral
    | FunctionType of FunctionTypeLiteral
    | LiteralStringType of string
    | ObjectType of list<string * TypeLiteral>
    | TupleType of list<TypeLiteral>
    | TypeReference of string * list<TypeLiteral>
    | UnionType of list<TypeLiteral>

and FunctionTypeLiteral =
    {
        ParameterTypes : list<string * TypeLiteral>
        ReturnType : TypeLiteral
    }

type Expression =
    | Call of Expression * list<TypeLiteral> * list<Expression>
    | GetField of Expression * Expression
    | LiteralJson of string
    | LiteralObject of list<string * Expression>
    | LiteralString of string
    | InstanceOf of Expression * Expression
    | Invoke of Expression * string * list<Expression>
    | New of Expression * list<Expression>
    | SimpleLambda of list<string> * Expression
    | This
    | Var of string

type Statement =
    | EmptyStatement
    | Conditionals of list<Expression * Statement> * option<Statement>
    | Return of Expression
    | Throw of Expression

type FunctionDefinition =
    {
        Name : string
        Generics : list<string>
        Parameters : list<string * TypeLiteral>
        ReturnType : TypeLiteral
        Body : Statement
    }

    static member Create(name, body, returnType, ?generics, ?parameters) =
        {
            Name = name
            Generics = defaultArg generics []
            Parameters = defaultArg parameters []
            ReturnType = returnType
            Body = body
        }

type ClassMethod =
    {
        FunctionDefinition : FunctionDefinition
        IsStatic : bool
    }

    member this.Static() =
        { this with IsStatic = true }

    static member Create(fdef) =
        { FunctionDefinition = fdef; IsStatic = false }

type ClassField =
    {
        FieldName : string
        FieldType : TypeLiteral
        IsPublic : bool
    }

    static member Create(name, ty, ?pub) =
        { FieldName = name; FieldType = ty; IsPublic = defaultArg pub true }

type Constructor =
    | SimpleConstructor of list<ClassField> * Statement

type EnumDefinition =
    {
        EnumName : string
        EnumCases : list<string * int>
    }

    static member Create(name, cases) =
        {
            EnumName = name
            EnumCases = Seq.toList cases
        }

type ClassDefinition =
    {
        ClassName : string
        Constructor : Constructor
        Methods : list<ClassMethod>
    }

    member this.WithMethod(m) =
        { this with Methods = m :: this.Methods }

    static member Create(name, ctor, ?methods) =
        {
            ClassName = name
            Constructor = ctor
            Methods = defaultArg methods []
        }

type Definitions =
    | Action of Expression
    | Comment of string
    | DefinitionSequence of list<Definitions>
    | DeclareVar of string * Expression
    | DefineClass of ClassDefinition
    | DefineEnum of EnumDefinition
    | DefineFunction of FunctionDefinition
    | DefineTypeAlias of string * TypeLiteral
    | InModule of string * Definitions

    member this.GroupModules() =
        let rec find ctx def =
            match def with
            | InModule (m, def) -> find (m :: ctx) def
            | DefinitionSequence xs -> seq { for x in xs do yield! find ctx x }
            | _ -> Seq.singleton (ctx, def)
        find [] this
        |> Seq.map (fun (m, defs) -> (List.rev m, defs))
        |> Seq.groupBy fst
        |> Seq.collect (fun (m, defs) ->
            let defs = Seq.toList (Seq.map snd defs)
            match m with
            | [] -> defs
            | ms -> [InModule (String.concat "." ms, DefinitionSequence defs)])
        |> Seq.toList
        |> DefinitionSequence
        
