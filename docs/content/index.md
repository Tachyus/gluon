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

We will soon publish to http://nuget.org, but for now you can use the
public AppVeyor NuGet feed at https://ci.appveyor.com/nuget/gluon.

## Building

TBD.

## Contributing

The project is at an early stage but is actively used at Tachyus. We
welcome contributions, please use the GitHub issue tracker to
coordinate.
