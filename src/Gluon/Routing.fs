// Copyright 2019 Tachyus Corp.
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

[<System.Runtime.CompilerServices.Extension>]
module Gluon.Routing

open System.Collections.Generic
open System.IO
open System.Net
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Routing.Constraints
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.ContextInsensitive

type Server =
    {
        json : JsonSerializer
        methods : seq<Method>
        prefix : string
        schemaJson : string
        service : Service
    }

let computeSchemaJson (svc: Service) =
    let ser = JsonSerializer.Create([typeof<Schema.Service>])
    ser.ToJsonString<Schema.Service>(svc.Schema)

let prepare (svc: Service) (prefix: string) =
    {
        json = JsonSerializer.Create(svc.IOTypes)
        methods = svc.Methods
        prefix = prefix
        schemaJson = computeSchemaJson svc
        service = svc
    }

let getParamJson (ctx: HttpContext) (p: Schema.Parameter) =
    ctx.Request.Query
    |> Seq.tryFind (fun (KeyValue(key, value)) -> key = p.ParameterName && value.Count = 1)
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

let serveRequest server (m: Method) (ctx: HttpContext) =
    task {
        let input =
            match m.IOTypes.InputType with
            | None -> null
            | Some t ->
                match m.Key.httpMethod with
                | Schema.HttpMethod.Get -> parseGetInput server ctx m
                | _ ->
                    use reader = new StreamReader(ctx.Request.Body)
                    server.json.ReadJson(t, reader)
        let! output = m.Invoke(ctx, input)
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
    } :> Task

let serveSchema server (ctx: HttpContext) =
    task {
        let resp = ctx.Response
        do resp.StatusCode <- 200
        do resp.ContentType <- "text/json"
        return! resp.WriteAsync(server.schemaJson).ContinueWith(ignore)
    } :> Task

let build (server:Server) inlineConstraintResolver =
    let routes = RouteCollection()
    for m in server.methods do
        let route =
            Route(
                target=RouteHandler (fun ctx -> serveRequest server m ctx),
                routeTemplate=m.Key.localPath,
                defaults=null,
                constraints=dict [|"httpMethod", box(HttpMethodRouteConstraint([|m.Key.httpMethod.ToString()|]))|],
                dataTokens=null,
                inlineConstraintResolver=inlineConstraintResolver)
        routes.Add(route)
    let schemaRoute =
            Route(
                target=RouteHandler (fun ctx -> serveSchema server ctx),
                routeName="Schema",
                routeTemplate="Schema",
                defaults=null,
                constraints=dict [|"httpMethod", box(HttpMethodRouteConstraint([|"GET"|]))|],
                dataTokens=null,
                inlineConstraintResolver=inlineConstraintResolver)
    routes.Add(schemaRoute)
    routes :> IRouter

[<Sealed>]
type Options(prefix: string, server: Server) =

    override __.ToString() =
        [| for m in server.methods -> string m.Key |]
        |> String.concat "\r\n"

    member __.Server = server
    member __.Service = server.service
    member __.UrlPrefix = prefix

    static member Create(svc: Service, ?prefix) =
        let prefix = defaultArg prefix "gluon-api"
        Options(prefix, prepare svc prefix)

[<System.Runtime.CompilerServices.Extension>]
[<CompiledName("Map")>]
let map (app:IApplicationBuilder, options: Options) =
    app.Map(PathString(options.UrlPrefix), fun builder ->
        let inlineConstraintResolver = builder.ApplicationServices.GetRequiredService<IInlineConstraintResolver>()
        let router = build options.Server inlineConstraintResolver
        builder.UseRouter(router) |> ignore)
