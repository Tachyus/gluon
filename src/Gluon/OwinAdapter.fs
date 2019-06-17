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
open System.Net
open System.Threading.Tasks
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
        Microsoft.FSharpLu.Json.Compact.serialize(svc.Schema)

    let prepare (svc: Service) (prefix: string) =
        {
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

    let readJsonString (m: Method) json =
        JsonSerializer.deserializeType m.IOTypes.InputType.Value json

    let parseGetInput ctx (m: Method) =
        match m.Schema.MethodParameters with
        | [] -> null
        | [p] -> getParamJson ctx p |> readJsonString m
        | _ ->
            [for p in m.Schema.MethodParameters -> getParamJson ctx p]
            |> String.concat ","
            |> sprintf "[%s]"
            |> readJsonString m

    let wrapMethodInvocation (ctx: IOwinContext) (req: MethodKey) (m: Method) =
        async {
            let context = Context(ctx.Environment)
            let input =
                match m.IOTypes.InputType with
                | None -> null
                | Some t ->
                    match req.httpMethod with
                    | Schema.HttpMethod.Get -> parseGetInput ctx m
                    | _ -> JsonSerializer.deserializeTypeFromStream t ctx.Request.Body
            let! output = m.Invoke(context, input)
            return
                match m.IOTypes.OutputType with
                | None ->
                    // if the status code is unchanged, set it to 204 No Content;
                    // otherwise, leave it alone.
                    if ctx.Response.StatusCode = int HttpStatusCode.OK then
                        ctx.Response.StatusCode <- int HttpStatusCode.NoContent
                    else ()
                | Some t ->
                    ctx.Response.ContentType <- "application/json"
                    JsonSerializer.serializeTypeToStream t ctx.Response.Body output
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
                return! wrapMethodInvocation ctx req m
        }

    let handleRequest server ctx =
        let work = handleRequestAsync server ctx
        let ct = ctx.Request.CallCancelled
        Async.StartAsTask(work, cancellationToken = ct) :> Task

[<Sealed>]
type Options(prefix: string, server: OwinSupport.Server) =

    override __.ToString() =
        seq { for KeyValue(k, _) in server.methods -> string k }
        |> String.concat "\r\n"

    member __.Server = server
    member __.Service = server.service
    member __.UrlPrefix = prefix

    static member Create(svc: Service, ?prefix) =
        let prefix = defaultArg prefix "/gluon-api"
        Options(prefix, OwinSupport.prepare svc prefix)

[<Obsolete("Use Gluon.Options instead.")>]
type OwinOptions = Options

module Owin =

    let middleware (options: Options) =
        MidFunc(fun _ ->
            AppFunc(fun env ->
                let ctx = OwinContext(env)
                OwinSupport.handleRequest options.Server ctx))
