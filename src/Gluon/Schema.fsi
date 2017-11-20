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

/// Defines metadata types for describing a Gluon service schema.
namespace Gluon.Schema

open System

/// Describes types that can travel over Gluon transport - the DataType set.
type DataType =
    | ArrayType of DataType
    | BooleanType
    | BytesType
    | DateTimeType
    | DateTimeOffsetType
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

/// Describes a record or a union case field.
type Field =
    {
        FieldName : string
        FieldType : DataType
    }

/// Named type definition corresponding to a record.
type Record =
    {
        RecordName : string
        RecordFields : list<Field>
    }

/// Discriminated union case definition.
type UnionCase =
    {
        CaseName : string
        CaseFields : list<Field>
    }

/// Named type definition corresponding to a discriminated union.
type Union =
    {
        UnionName : string
        UnionCases : list<UnionCase>
    }

/// Enumeration case definition.
type EnumCase =
    {
        EnumCaseName : string
        EnumCaseValue : int
    }

/// Enumeration type definition.
type Enum =
    {
        EnumName : string
        EnumCases : list<EnumCase>
    }

/// Named type definitions.
type TypeDefinition =
    | DefineEnum of Enum
    | DefineRecord of Record
    | DefineUnion of Union

/// Method parameter descriptor.
type Parameter =
    {
        ParameterName : string
        ParameterType : DataType
    }

/// HTTP methods such as GET, POST, etc.
type HttpMethod =
    | Delete
    | Get
    | Post
    | Put

    /// Parses a string representation.
    static member Parse : string -> HttpMethod

/// Information on how to call the method.
type CallingConvention =
    /// The method is called over HTTP, using specified verb and path.
    /// Example of a path: "/FooController/Bar".
    /// For GET verb, parameters are passed in URL.
    /// For other verbs, they are passed in body as JSON.
    | HttpCallingConvention of HttpMethod * path: string

/// Remote method descriptor.
type Method =
    {
        CallingConvention : CallingConvention
        MethodName : string
        MethodParameters : list<Parameter>
        MethodReturnType : option<DataType>
    }

/// Describes a complete Gluon service.
type Service =
    {
        Methods : list<Method>
        TypeDefinitions : list<TypeDefinition>
    }

    /// Lists the top-level namespaces.
    member Namespaces : string list

    /// Validates absense of common errors: invalid names,
    /// name clashes, dangling references.
    member Check : unit -> unit
