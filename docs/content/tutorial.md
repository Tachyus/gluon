# Tutorial

The setup is a little tricky, but see the `samples` folder. You would
typically use two projects, one for the F# APIs and web setup, and one
for the C# or TypeScript web application.

## F# Project

In the F# project, reference `Gluon` NuGet package. Also reference
`Owin` and `Microsoft.Owin`. Mark some methods as remote:

```fsharp
module MyApp.Arithmetic

[<Gluon.Remote>]
let incr x = x + 1
```

In your OWIN startup code, add Gluon to the pipeline, for example:

```fsharp
namespace MyApp

open Owin
open Microsoft.Owin
open Gluon

type Startup() =
    member x.Configuration(app: IAppBuilder) =
        app.MapGluon() |> ignore

[<assembly: OwinStartup(typeof<Startup>)>]
do ()
```

Lastly, you need to make sure Gluon added a target to your `fsproj` file to get the `Gluon.CLI.exe` to copy the generated TypeScript client to your web project:

```xml
<PropertyGroup>
  <GluonToolPath>..\packages\Gluon\tools\Gluon.CLI.exe</GluonToolPath>
</PropertyGroup>
<Target Name="GluonCompiler" AfterTargets="Build">
  <Exec Command="&quot;$(GluonToolPath)&quot; --out &quot;..\WebProject\Scripts\$(Name).ts&quot; --reflect &quot;$(TargetPath)&quot;" ContinueOnError="false" WorkingDirectory="$(MSBuildThisFileDirectory)" />
</Target>
```

See sample projects for an example using `Gluon.targets` files to manage these targets.

## Web Project

Reference a bunch of packages, namely:

    Microsoft.Owin
    Microsoft.Owin.Host.SystemWeb
    Microsoft.Owin.Hosting
    Owin
    Gluon.Client
    jQuery
    jQuery.TypeScript.DefinitelyTyped

Add TypeScript compilation to the project, if it does not have it
already.  This typically takes this form in `.csproj`:

```xml
<Import Project="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\TypeScript\Microsoft.TypeScript.targets"
        Condition="Exists('$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\TypeScript\Microsoft.TypeScript.targets')" />
```

In your `Scripts/app.ts`, write something like this:

```typescript
/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/Gluon.d.ts" />
/// <reference path="Gluon.Generated.ts" />

jQuery(() => {
    console.log('start');
    var cli = new Gluon.Client();

    MyApp.Arithmetic.incr(cli)(1).then(x => {
        console.log('incr(1) = ', x);
    });
});
```

## Workflow

Certain error conditions such as name conflicts on methods, or using a
type that is not a DataType as either method parameter or return type,
are detected statically at compile time via code generation.

The code generator takes a service descriptor, validates it, and
provide TypeScript types for the matching client.  The user workflow is:

* edit F# service files
* compile
* obtain the re-generated TypeScript code and adjust TypeScript call
  sites as necessary, guided by the TypeScript compiler

Gluon ships with MSBuild automation and API to roll your own.

