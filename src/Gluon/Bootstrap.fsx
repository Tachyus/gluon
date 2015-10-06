// This little scripts generates some definitions in Gluon.ts to dog-food the type projection.

#r "bin/Debug/Microsoft.Owin.dll"
#r "bin/Debug/Newtonsoft.Json.dll"
#r "bin/Debug/Gluon.dll"

open System
open System.IO
open System.Text.RegularExpressions
open Gluon

let substitute (path: string) (tag: string) (code: string) =
    let pattern = sprintf @"[/][/][ ][<]%s[>].*[/][/][ ][<][/]%s[>]" tag tag
    let tagged (s: string) =
        String.concat Environment.NewLine [
            sprintf "// <%s>" tag
            s.Trim()
            sprintf "// </%s>" tag
        ]
    let original = File.ReadAllText(path)
    let replaced = Regex.Replace(original, pattern, tagged code, RegexOptions.Singleline)
    File.WriteAllText(path, replaced)

let bootstrap () =
    let types =
        let main () = async { return Unchecked.defaultof<Schema.Service> }
        let m = Method.Create("dummy", main)
        let svc = Service.FromMethod m
        svc.Schema.TypeDefinitions
    let prog = TypeScript.Generator.Create().GenerateTypeCode(types)
    let path = Path.Combine(__SOURCE_DIRECTORY__, "../Gluon.Client/Gluon.ts")
    prog.Definitions.WriteString()
    |> substitute path "BOOTSTRAP-DEFS"
    prog.Initializer.WriteString()
    |> substitute path "BOOTSTRAP-INIT"
    printfn "written %s" path

bootstrap ()
