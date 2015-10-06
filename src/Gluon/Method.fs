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

type Method =
    {
        execute : Context -> obj -> Async<obj>
        ioTypes : Reflect.IOTypes
        methodSchema : Schema.Method
    }

    member this.Invoke(ctx, arg) =
        this.execute ctx arg

    member this.IOTypes = this.ioTypes
    member this.Schema = this.methodSchema

    static member Reflect(info: MethodInfo) =
        if info.IsGenericMethod then
            failwith "Cannot use generic methods with Gluon"
        let m = Reflect.adaptRuntimeMethod(info)
        {
            execute = fun ctx v -> m.Invoke(ctx, v)
            ioTypes = Reflect.ioTypes info
            methodSchema = Reflect.getSchemaFromRuntimeMethod info
        }

    static member Create(execute, ioTypes, schema) =
        {
            execute = execute
            ioTypes = ioTypes
            methodSchema = schema
        }

    static member CreateParam(name, ty) : Schema.Parameter =
        {
            ParameterName = name
            ParameterType = Reflect.getDataType ty
        }

    static member CreateSchema(name, ps, ret) : Schema.Method =
        let ps = [for (name, ty) in ps -> Method.CreateParam(name, ty)]
        {
            CallingConvention = Schema.HttpCallingConvention (Schema.Get, name)
            MethodName = name
            MethodParameters = ps
            MethodReturnType =
                if ret = typeof<unit>
                    then None
                    else Some (Reflect.getDataType ret)
        }

    static member Create(name: string, body: unit -> Async<'A>) =
        let execute (ctx: Context) (v: obj) =
            async {
                let! v = body ()
                return box v
            }
        Method.Create(execute,
            Reflect.IOTypes.Create<unit,'A>(),
            Method.CreateSchema(name, [], typeof<'A>))

    static member Create(name, argName, body: 'A -> Async<'B>) =
        let execute (ctx: Context) (v: obj) =
            async {
                let! res = body (unbox v)
                return box res
            }
        let ps = [(argName, typeof<'A>)]
        Method.Create(execute,
            Reflect.IOTypes.Create<'A,'B>(),
            Method.CreateSchema(name, ps, typeof<'B>))

    static member Create(name, a1, a2, body: 'A -> 'B -> Async<'C>) =
        let execute (ctx: Context) (v: obj) =
            async {
                let (a, b) = unbox v
                let! res = body a b
                return box res
            }
        let ps = [(a1, typeof<'A>); (a2, typeof<'B>)]
        Method.Create(execute,
            Reflect.IOTypes.Create<'A * 'B,'C>(),
            Method.CreateSchema(name, ps, typeof<'B>))
