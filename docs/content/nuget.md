# NuGet

There are currently two NuGet packages involved.

## Gluon

Ships the main assembly, `Gluon.dll`, for use in the OWIN application, the command-line helper utility, `Gluon.CLI.exe`, and MSBuild automation to run this utility on every build.

## Gluon.Client

Ships the TypeScript client library, including `typings/Gluon.d.ts`, `Gluon.js`, `Gluon.min.js`, and `Gluon.min.js.map`. This package is intended to be used in web projects and will put all the above files into the `Scripts` folder.
