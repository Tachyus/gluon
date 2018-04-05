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

/// Options for adapting Gluon services to OWIN using HTTP transport.
[<Sealed>]
type Options =

    /// The Gluon service used by the adapter.
    member Service : Service

    /// The URL prefix.
    member UrlPrefix : string

    /// Constructs a new OwinAdapter for a given service.
    /// The URL prefix defaults to `/gluon-api`.
    static member Create : Service * ?urlPrefix: string -> Options

[<Obsolete("Use Gluon.Options instead.")>]
type OwinOptions = Options

/// Owin-related types.
module Owin =

    /// Formalizes OWIN Application Function.
    type AppFunc = Func<IDictionary<string, obj>, Task>

    /// Formalizes OWIN Middleware Function.
    type MidFunc = Func<AppFunc, AppFunc>

    /// Handles a web request by dispatching it to a service method.
    val middleware : options: Options -> MidFunc
