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

type Service =
    {
        serviceSchema : Schema.Service
        serviceMethods : list<Method>
    }

    member this.IOTypes =
        seq {
            for m in this.serviceMethods do
                yield! m.IOTypes.All
        }
        |> Seq.distinct

    member this.Methods = Seq.ofList this.serviceMethods
    member this.Schema = this.serviceSchema

    static member BuildSchema(methods: list<Method>) =
        let typeDefs =
            [
                for m in methods do
                    yield! m.IOTypes.All
            ]
            |> Reflect.walkTypes
            |> Seq.choose (Reflect.(|TypeDef|_|))
            |> Seq.toList
        let schema : Schema.Service =
            {
                Methods = [for m in methods -> m.Schema]
                TypeDefinitions = typeDefs
            }
        schema.Check()
        schema

    static member FromMethods(ms: seq<Method>) =
        let ms = Seq.toList ms
        {
            serviceMethods = ms
            serviceSchema = Service.BuildSchema(ms)
        }

    static member FromMethod(m: Method) =
        Service.FromMethods([m])

    static member Merge(services) =
        [
            for s in services do
                yield! s.serviceMethods
        ]
        |> Seq.distinctBy (fun m -> m.Schema.MethodName)
        |> Service.FromMethods

    static member FromAssembly(asm) =
        Reflect.findRemoteMethods asm
        |> Seq.map Method.Reflect
        |> Service.FromMethods
