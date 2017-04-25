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

namespace Gluon.TypeScript

open System
open System.IO
open System.Text
open System.Threading
open System.Web
open Gluon

module FileWriter =

    let writeFile (path: string) (text: string) =
        if not (File.Exists(path)) || File.ReadAllText(path) <> text then
            let dir = DirectoryInfo(Path.GetDirectoryName(path))
            if dir.Exists |> not then
                dir.Create()
            let enc = UTF8Encoding(false, true)
            File.WriteAllText(path, text, enc)

[<Sealed>]
type CodeUnit(layout: PrettyPrint.Layout) =

    member this.Write(out: TextWriter) =
        layout
        |> PrettyPrint.writeLayout out

    member this.WriteFile(path: string) =
        this.WriteString()
        |> FileWriter.writeFile path

    member this.WriteString() =
        use out = new StringWriter()
        this.Write(out)
        out.ToString()

[<Sealed>]
type Program(defs: CodeUnit, init: CodeUnit) =
    member this.Definitions = defs
    member this.Initializer = init

[<Sealed>]
type Generator() =

    member this.GenerateServiceCode(service: Service) =
        let defs =
            Syntax.DefinitionSequence [
                CodeGen.typeDefinitions service.Schema.TypeDefinitions
                CodeGen.methodStubs service.Schema
            ]
            |> PrettyPrinter.definitions
        let init =
            Syntax.DefinitionSequence [
                CodeGen.registerActivators service.Schema.TypeDefinitions
                CodeGen.registerService service.Schema
            ]
            |> PrettyPrinter.definitions
        Program(defs = CodeUnit(defs), init = CodeUnit(init))

    member this.GenerateTypeCode(types) =
        let defs =
            CodeGen.typeDefinitions types
            |> PrettyPrinter.definitions
        let init =
            Syntax.DefinitionSequence [
                CodeGen.registerActivators types
                CodeGen.registerTypeDefinitions types
            ]
            |> PrettyPrinter.definitions
        Program(defs = CodeUnit(defs), init = CodeUnit(init))

    member this.Write(service, out) =
        let prog = this.GenerateServiceCode(service)
        prog.Definitions.Write(out)
        prog.Initializer.Write(out)

    member this.WriteString(service) =
        use out = new StringWriter()
        this.Write(service, out)
        out.ToString()

    member this.WriteFile(service: Service, path: string) =
        this.WriteString(service)
        |> FileWriter.writeFile path

    static member Create() =
        Generator()
