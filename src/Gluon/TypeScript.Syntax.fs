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
    | SimpleLambda of list<string * TypeLiteral> * Expression
    | This
    | Var of string
    | Cast of Expression * Expression

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

type UnionCaseField =
    {
        FieldName : string
        FieldType : TypeLiteral
        IsOptional : bool
    }

    static member Create(name, ty, ?isOptional) =
        { FieldName = name; FieldType = ty; IsOptional = defaultArg isOptional false }

type UnionCaseDefinition =
    {
        UnionCaseName : string
        Fields : UnionCaseField list
    }

    static member Create(name, ?fields) =
        { UnionCaseName = name; Fields = defaultArg fields [] }

type Definitions =
    | Action of Expression
    | Comment of string
    | DefinitionSequence of list<Definitions>
    | DeclareVar of string * Expression
    | DefineClass of ClassDefinition
    | DefineEnum of EnumDefinition
    | DefineFunction of FunctionDefinition
    | DefineTypeAlias of string * TypeLiteral
    | DefineUnionCase of UnionCaseDefinition
    | InNamespace of string * Definitions

    member this.GroupNamespaces() =
        let rec merge (a:Definitions) (b:Definitions) : Definitions =
            match a, b with
            | InNamespace(nsA, defA), InNamespace(nsB, defB) when nsA = nsB ->
                InNamespace(nsA, merge defA defB)
            | DefinitionSequence a, InNamespace(nsB, defB) ->
                let matched, notMatched = 
                    a |> List.partition (function InNamespace(nsA, _) when nsA = nsB -> true | _ -> false)
                match matched with
                | [InNamespace(_, defA)] ->
                    DefinitionSequence(InNamespace(nsB, merge defA defB) :: notMatched)
                | _ -> DefinitionSequence(b::a)
            | DefinitionSequence a, DefinitionSequence b ->
                DefinitionSequence(a @ b)
            | DefinitionSequence a, b ->
                DefinitionSequence(b::a)
            | a, DefinitionSequence b ->
                DefinitionSequence(a::b)
            | _, _ ->
                DefinitionSequence [a; b]

        let rec step (acc:Definitions list) (def:Definitions) : Definitions list =
            match def with
            | DefinitionSequence [] ->
                // Skip the value.
                acc
            | DefinitionSequence (def'::[]) ->
                // Recursively call step until Definitions are found.
                step acc def'
            | DefinitionSequence defs ->
                // Fold the results of calling step for each Definitions instance.
                // NOTE: This could potentially result in a StackOverflowException.
                defs |> List.fold step acc
            | _ ->
                // Process the provided Definitions.
                match acc with
                | [] -> [def]
                | _ ->
                    let res =
                        acc
                        |> List.map (fun a ->
                            let merged = merge a def
                            match merged with
                            | InNamespace _ -> Choice1Of2 merged
                            | DefinitionSequence _ -> Choice2Of2 a
                            | _ -> failwith "Invalid option"
                        )
                    let wasMerged =
                        res |> List.exists (function Choice1Of2 _ -> true | Choice2Of2 _ -> false)
                    if wasMerged then
                        res |> List.map (function Choice1Of2 def -> def | Choice2Of2 def -> def)
                    else def::acc

        match this with
        | DefinitionSequence defs ->
            let xs = defs |> Seq.fold step []
            xs |> DefinitionSequence
        | _ -> this
