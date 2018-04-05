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
open System.Collections.Generic
open System.IO
open System.Net
open System.Threading.Tasks
open Owin
open Microsoft.Owin

module OwinSupport =

    type MethodKey =
        {
            httpMethod : Schema.HttpMethod
            localPath : string
        }

        override this.ToString() =
            sprintf "%O %s" this.httpMethod this.localPath

    type Server =
        {
            json : JsonSerializer
            methods : IDictionary<MethodKey,Method>
            prefix : string
            schemaJson : string
            schemaJsonPrefix : string
            service : Service
        }

    let getMethodKey (m: Method) =
        match m.Schema.CallingConvention with
        | Schema.HttpCallingConvention (verb, path) ->
            { localPath = path; httpMethod = verb }

    let getMethodKeyFromRequest server (req: IOwinRequest) =
        let uri = req.Uri
        if uri.LocalPath.StartsWith(server.prefix) then
            let localPath = uri.LocalPath.Substring(server.prefix.Length)
            {
                httpMethod = Schema.HttpMethod.Parse(req.Method)
                localPath = localPath
            }
        else
            failwithf "The given URI [%O] does not have the expected prefix %s"
                uri server.prefix

    let pickMethod server (req: MethodKey) =
        let mutable out = Unchecked.defaultof<_>
        if server.methods.TryGetValue(req, &out)
            then out
            else failwithf "No method found with the key [%O]" req

    let computeSchemaJson (svc: Service) =
        let ser = JsonSerializer.Create([typeof<Schema.Service>])
        ser.ToJsonString<Schema.Service>(svc.Schema)

    let prepare (svc: Service) (prefix: string) =
        {
            json = JsonSerializer.Create(svc.IOTypes)
            methods =
                seq {
                    for m in svc.Methods ->
                        (getMethodKey m, m)
                }
                |> dict
            prefix = prefix + "/"
            schemaJson = computeSchemaJson svc
            schemaJsonPrefix = prefix + "/Schema"
            service = svc
        }

    let getParamJson (ctx: IOwinContext) (p: Schema.Parameter) =
        ctx.Request.Query
        |> Seq.tryFind (fun (KeyValue(key, value)) -> key = p.ParameterName && value.Length = 1)
        |> Option.map (fun (KeyValue(_, value)) -> value.[0])
        |> Option.toObj

    let readJsonString server (m: Method) json =
        use reader = new StringReader(json)
        server.json.ReadJson(m.IOTypes.InputType.Value, reader)

    let parseGetInput server ctx (m: Method) =
        match m.Schema.MethodParameters with
        | [] -> null
        | [p] -> getParamJson ctx p |> readJsonString server m
        | _ ->
            [for p in m.Schema.MethodParameters -> getParamJson ctx p]
            |> String.concat ","
            |> sprintf "[%s]"
            |> readJsonString server m

    let wrapMethodInvocation server (ctx: IOwinContext) (req: MethodKey) (m: Method) =
        async {
            let context = Context(ctx)
            let input =
                match m.IOTypes.InputType with
                | None -> null
                | Some t ->
                    match req.httpMethod with
                    | Schema.HttpMethod.Get -> parseGetInput server ctx m
                    | _ ->
                        use reader = new StreamReader(ctx.Request.Body)
                        server.json.ReadJson(t, reader)
            let! output = m.Invoke(context, input)
            return
                use writer = new StreamWriter(ctx.Response.Body) in
                match m.IOTypes.OutputType with
                | None ->
                    // if the status code is unchanged, set it to 204 No Content;
                    // otherwise, leave it alone.
                    if ctx.Response.StatusCode = int HttpStatusCode.OK then
                        ctx.Response.StatusCode <- int HttpStatusCode.NoContent
                    else ()
                | Some t ->
                    ctx.Response.ContentType <- "application/json"
                    server.json.WriteJson(t, writer, output)
        }

    let serveSchema server (ctx: IOwinContext) =
        async {
            let resp = ctx.Response
            do resp.StatusCode <- 200
            do resp.ContentType <- "text/json"
            return!
                resp.WriteAsync(server.schemaJson).ContinueWith(ignore)
                |> Async.AwaitTask
        }

    let (|SchemaRequest|_|) server (req: IOwinRequest) =
        let isMatch =
            req.Method.ToLower() = "get"
            && req.Uri.LocalPath = server.schemaJsonPrefix
        if isMatch then Some () else None

    let handleRequestAsync server (ctx: IOwinContext) =
        async {
            match ctx.Request with
            | SchemaRequest server () ->
                return! serveSchema server ctx
            | _ ->
                let req = getMethodKeyFromRequest server ctx.Request
                let m = pickMethod server req
                return! wrapMethodInvocation server ctx req m
        }

    let handleRequest server ctx =
        let work = handleRequestAsync server ctx
        let ct = ctx.Request.CallCancelled
        Async.StartAsTask(work, cancellationToken = ct) :> Task

[<Sealed>]
type OwinOptions(prefix: string, server: OwinSupport.Server) =

    override this.ToString() =
        seq { for KeyValue(k, v) in server.methods -> string k }
        |> String.concat "\r\n"

    member this.Server = server
    member this.Service = server.service
    member this.UrlPrefix = prefix

    static member Create(svc: Service, ?prefix) =
        let prefix = defaultArg prefix "/gluon-api"
        OwinOptions(prefix, OwinSupport.prepare svc prefix)

module Owin =

    type AppFunc = Func<IDictionary<string, obj>, Task>
    type MidFunc = Func<AppFunc, AppFunc>

    let middleware (options: OwinOptions) =
        MidFunc(fun next ->
            AppFunc(fun env ->
                let ctx = OwinContext(env)
                OwinSupport.handleRequest options.Server ctx))

[<AutoOpen>]
module OwinExtensions =

    type IAppBuilder with

        member app.MapGluon(options: OwinOptions) =
            app.Map(options.UrlPrefix, fun ctx -> ctx.Use(Owin.middleware options) |> ignore)

        member app.MapGluon(?service: Service, ?prefix: string) =
            let service = defaultArg service (Service.FromAssembly(Reflection.Assembly.GetCallingAssembly()))
            let options = OwinOptions.Create(service, ?prefix = prefix)
            app.MapGluon(options)
