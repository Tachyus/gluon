Gluon provides a type-safe remoting connector between an F# backend
and a TypeScript client. For example:

```fsharp
module My.Service

[<Gluon.Remote(Verb="GET")>]
let increment (x: int) =
    x + 1
```

Gluon can take this and build plumbing that lets you call this code
from TypeScript:

```typescript
var c = new Gluon.Client();
My.Service.increment(c)(1).then(x => console.log(x));
```

## Installing

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Gluon library can be <a href="http://www.nuget.org/packages/Gluon">installed from NuGet</a>:
      <pre>PM> Install-Package Gluon</pre>
    </div>
  </div>
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Gluon.Client library can be <a href="http://www.nuget.org/packages/Gluon.Client">installed from NuGet</a>:
      <pre>PM> Install-Package Gluon.Client</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

## Building

Checkout the project from [GitHub](https://github.com/Tachyus/gluon) and then run the `build.ps1` (Windows) or `build.sh` (*nix) script to build the projects. You will need .NET 4.5 or Mono and node.js installed.

## Contributing

The project is at an early stage but is actively used at Tachyus. We welcome contributions. Please use the [GitHub issue tracker](https://github.com/Tachyus/gluon/issues) to coordinate.
