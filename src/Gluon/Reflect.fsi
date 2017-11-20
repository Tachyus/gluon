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
open System.Reflection

/// Reflection facilities.
module internal Reflect =

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
        | DateTimeOffsetType
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

    val typeChildren : TypeShape -> list<Type>
    val typeShape : Type -> TypeShape
    val walkTypes : seedTypes: seq<Type> -> seq<Type>

    // Method adapters --------------------------------------------------------

    type BoxedMethod =
        {
            Invoke : Context * obj -> Async<obj>
        }

    val adaptRuntimeMethod : MethodInfo -> BoxedMethod

    type IOTypes =
        {
            InputType : option<Type>
            OutputType : option<Type>
        }

        member All : list<Type>

        static member Create<'In,'Out> : unit -> IOTypes

    val ioTypes : MethodInfo -> IOTypes

    // Schema-related ---------------------------------------------------------

    val (|TypeDef|_|) : Type -> option<Schema.TypeDefinition>
    val getDataType : Type -> Schema.DataType
    val getSchemaFromRuntimeMethod : MethodInfo -> Schema.Method

    // Assembly reflection ----------------------------------------------------

    val findRemoteMethods : Assembly -> seq<MethodInfo>
