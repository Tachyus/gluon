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
open System.Threading.Tasks
open Owin
open Microsoft.Owin
open Gluon

/// Options for adapting Gluon services to OWIN using HTTP transport.
[<Sealed>]
type OwinOptions =

    /// The Gluon service used by the adapter.
    member Service : Service

    /// The URL prefix.
    member UrlPrefix : string

    /// Constructs a new OwinAdapter for a given service.
    /// The URL prefix defaults to `/gluon-api`.
    static member Create : Service * ?urlPrefix: string -> OwinOptions

/// Owin-related types.
module Owin =

    /// Formalizes OWIN Application Function.
    type AppFunc = Func<IDictionary<string, obj>, Task>

    /// Formalizes OWIN Middleware Function.
    type MidFunc = Func<AppFunc, AppFunc>

    /// Handles a web request by dispatching it to a service method.
    val middleware : options: OwinOptions -> MidFunc

[<AutoOpen>]
module OwinExtensions =

    /// Provides an extension method for use in mapping the Gluon OWIN adapter
    /// into a Katana (Microsoft.Owin) web application.
    type IAppBuilder with

        /// F# type extension to provide UseGluon extension to IAppBuilder.
        member MapGluon : options: OwinOptions -> IAppBuilder

        /// F# type extension to provide UseGluon extension to IAppBuilder.
        member MapGluon : ?service: Service * ?prefix: string -> IAppBuilder
