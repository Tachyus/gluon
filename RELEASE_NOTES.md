### New in 5.0.4 - (TBD)

### New in 5.0.3 - (Released 2018-05-31)
* Fix dependecnies to use FSharp.Core 4.3.4.

### New in 5.0.2 - (Released 2018-05-28)
* Include netcoreapp and net461 Gluon.CLI in Gluon.Client package.

### New in 5.0.1 - (Released 2018-05-28)
* Match up release versions with npm.

### New in 5.0.0 - (Released 2018-05-28)
* Changed Context.OwinContext to Context.Environment to correctly expose only OWIN abstractions.
* Removed IAppBuilder extensions in order to support netstandard2.0.
* Convert to netstandard2.0 and work with FSharp.Core 4.3.4.

### New in 4.0.6 - (Released 2018-02-09)
* Fix DateTimeOffset serialization round trip.

### New in 4.0.5 - (Released 2017-12-04)
* Fix DateTimeOffset serialization.

### New in 4.0.4 - (Released 2017-11-20)
* Fix issue #32 to support DateTimeOffset and Guid.

### New in 4.0.3 - (Released 2017-11-06)
* Fix issue #49 to resolve client -> server date serialization.

### New in 4.0.2 - (Released 2017-08-22)
* Fix issue #47 by aliasing top-level namespaces to avoid collisions.

### New in 4.0.1 - (Released 2017-05-25)
* Fix issues with gluon-client package

### New in 4.0.0 - (Released 2017-05-25)
* Switch to external and nested module code generation to work with module imports
* Publish gluon-client library to npm
* Gluon.CLI now packaged with the gluon-client library

### New in 3.0.1 - (Released 2017-01-23)
* Change Option type to `T | null | undefined` to work better with optional parameters in interface and function signatures.
* Add `Option.isSome` and `Option.isNone` guard functions to help identify whether a value is null or undefined.
* Change return type of `IHttpClient.httpGet` and `IHttpClient.httpCall` to `JQueryPromise<Option<T>>`, correctly indicating that the result is optional.
* Switch to `namespace` from `module` per recent changes in TypeScript guidance.

### New in 3.0.0 - (Released 2017-01-13)
Change Option type to `T | null` from tagged union.

### New in 2.0.3 - (Released 2017-01-12)
Switch build to use yarn rather than npm and include dist folder in source control. Also, remove console.log statements.

### New in 2.0.2 - (Released 2017-01-07)
Fix issue #26 by generating string literal union types from F# unions with no fields

### New in 2.0.1 - (Released 2017-01-06)
Fix bug in generating union cases with more than one field

### New in 2.0.0 - (Released 2017-01-04)
Update Gluon to generate TypeScript 2.0 compliant outputs, including:

* tagged unions
* noImplicitAny enabled
* strictNullChecks enabled

### New in 1.0.5 - (TBD)
Fix issue with too few letter variables in generated signatures.

### New in 1.0.4 - (Released 2016-02-04)
Fix path in source map file.

### New in 1.0.3 - (Released 2016-02-04)
Include Gluon.min.js and Gluon.min.js.map in Gluon.Client package.

### New in 1.0.2 - (Released 2016-02-02)
Split packages into Gluon (library and CLI) and Gluon.Client (TS and JS files).

### New in 1.0.1 - (Released 2015-12-29)
Include Gluon.js and Gluon.d.ts files.

### New in 1.0.0 - (Released 2015-10-22)
First public release.
