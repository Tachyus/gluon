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

// <BOOTSTRAP-DEFS>
module Tachyus.Gluon.Schema {

    export class Delete {

        constructor() { }
        tag(): string { return "Delete"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.HttpMethod", this);
        }
    }
    export class Get {

        constructor() { }
        tag(): string { return "Get"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.HttpMethod", this);
        }
    }
    export class Post {

        constructor() { }
        tag(): string { return "Post"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.HttpMethod", this);
        }
    }
    export class Put {

        constructor() { }
        tag(): string { return "Put"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.HttpMethod", this);
        }
    }
    export type HttpMethod = Delete | Get | Post | Put;
    export class HttpCallingConvention {

        constructor(public Item1: Tachyus.Gluon.Schema.HttpMethod,
            public path: string) { }
        tag(): string { return "HttpCallingConvention"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.CallingConvention", this);
        }
    }
    export type CallingConvention = HttpCallingConvention;
    export class ArrayType {

        constructor(public Item: Tachyus.Gluon.Schema.DataType) { }
        tag(): string { return "ArrayType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class BooleanType {

        constructor() { }
        tag(): string { return "BooleanType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class BytesType {

        constructor() { }
        tag(): string { return "BytesType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class DateTimeType {

        constructor() { }
        tag(): string { return "DateTimeType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class DoubleType {

        constructor() { }
        tag(): string { return "DoubleType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class IntType {

        constructor() { }
        tag(): string { return "IntType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class JsonType {

        constructor() { }
        tag(): string { return "JsonType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class ListType {

        constructor(public Item: Tachyus.Gluon.Schema.DataType) { }
        tag(): string { return "ListType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class OptionType {

        constructor(public Item: Tachyus.Gluon.Schema.DataType) { }
        tag(): string { return "OptionType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class SequenceType {

        constructor(public Item: Tachyus.Gluon.Schema.DataType) { }
        tag(): string { return "SequenceType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class StringDictType {

        constructor(public Item: Tachyus.Gluon.Schema.DataType) { }
        tag(): string { return "StringDictType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class StringType {

        constructor() { }
        tag(): string { return "StringType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class TupleType {

        constructor(public Item: Tachyus.Gluon.Schema.DataType[]) { }
        tag(): string { return "TupleType"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export class TypeReference {

        constructor(public Item: string) { }
        tag(): string { return "TypeReference"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.DataType", this);
        }
    }
    export type DataType = ArrayType | BooleanType | BytesType | DateTimeType | DoubleType | IntType | JsonType | ListType | OptionType | SequenceType | StringDictType | StringType | TupleType | TypeReference;
    export class Parameter {

        constructor(public ParameterName: string,
            public ParameterType: Tachyus.Gluon.Schema.DataType) { }
        static fromJSON(json: any): Parameter {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.Parameter", json);
        }
        tag(): string { return "Parameter"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.Parameter", this);
        }
    }
    export class Method {

        constructor(public CallingConvention: Tachyus.Gluon.Schema.CallingConvention,
            public MethodName: string,
            public MethodParameters: Tachyus.Gluon.Schema.Parameter[],
            public MethodReturnType: Tachyus.Gluon.Option<Tachyus.Gluon.Schema.DataType>) {

        }
        static fromJSON(json: any): Method {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.Method", json);
        }
        tag(): string { return "Method"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.Method", this);
        }
    }
    export class EnumCase {

        constructor(public EnumCaseName: string,
            public EnumCaseValue: number) { }
        static fromJSON(json: any): EnumCase {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.EnumCase", json);
        }
        tag(): string { return "EnumCase"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.EnumCase", this);
        }
    }
    export class Enum {

        constructor(public EnumName: string,
            public EnumCases: Tachyus.Gluon.Schema.EnumCase[]) { }
        static fromJSON(json: any): Enum {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.Enum", json);
        }
        tag(): string { return "Enum"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.Enum", this);
        }
    }
    export class Field {

        constructor(public FieldName: string,
            public FieldType: Tachyus.Gluon.Schema.DataType) { }
        static fromJSON(json: any): Field {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.Field", json);
        }
        tag(): string { return "Field"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.Field", this);
        }
    }
    export class Record {

        constructor(public RecordName: string,
            public RecordFields: Tachyus.Gluon.Schema.Field[]) { }
        static fromJSON(json: any): Record {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.Record", json);
        }
        tag(): string { return "Record"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.Record", this);
        }
    }
    export class UnionCase {

        constructor(public CaseName: string,
            public CaseFields: Tachyus.Gluon.Schema.Field[]) { }
        static fromJSON(json: any): UnionCase {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.UnionCase", json);
        }
        tag(): string { return "UnionCase"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.UnionCase", this);
        }
    }
    export class Union {

        constructor(public UnionName: string,
            public UnionCases: Tachyus.Gluon.Schema.UnionCase[]) { }
        static fromJSON(json: any): Union {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.Union", json);
        }
        tag(): string { return "Union"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.Union", this);
        }
    }
    export class DefineEnum {

        constructor(public Item: Tachyus.Gluon.Schema.Enum) { }
        tag(): string { return "DefineEnum"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.TypeDefinition", this);
        }
    }
    export class DefineRecord {

        constructor(public Item: Tachyus.Gluon.Schema.Record) { }
        tag(): string { return "DefineRecord"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.TypeDefinition", this);
        }
    }
    export class DefineUnion {

        constructor(public Item: Tachyus.Gluon.Schema.Union) { }
        tag(): string { return "DefineUnion"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.TypeDefinition", this);
        }
    }
    export type TypeDefinition = DefineEnum | DefineRecord | DefineUnion;
    export class Service {

        constructor(public Methods: Tachyus.Gluon.Schema.Method[],
            public TypeDefinitions: Tachyus.Gluon.Schema.TypeDefinition[]) { }
        static fromJSON(json: any): Service {

            return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.Service", json);
        }
        tag(): string { return "Service"; }
        toJSON(): any {

            return Tachyus.Gluon.Internals.toJSON("Tachyus.Gluon.Schema.Service", this);
        }
    }
}
module Tachyus.Gluon.Schema.HttpMethod {

    export function fromJSON(json: any): HttpMethod {

        return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.HttpMethod", json);
    }
    export function match<T>(value: Tachyus.Gluon.Schema.HttpMethod,
        cont: {
            Delete: (() => T), Get: (() => T), Post: (() => T), Put: (() => T)
        }): T {

        if (value instanceof Delete) { return cont.Delete(); }
        else if (value instanceof Get) { return cont.Get(); }
        else if (value instanceof Post) { return cont.Post(); }
        else if (value instanceof Put) { return cont.Put(); } else {
            throw new Error("match failed");
        }
    }
}
module Tachyus.Gluon.Schema.CallingConvention {

    export function fromJSON(json: any): CallingConvention {

        return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.CallingConvention", json);
    }
    export function match<T>(value: Tachyus.Gluon.Schema.CallingConvention,
        cont: {
            HttpCallingConvention: ((Item1: Tachyus.Gluon.Schema.HttpMethod,
                path: string) => T)
        }): T {

        if (value instanceof HttpCallingConvention) {
            return cont.HttpCallingConvention(value.Item1, value.path);
        } else { throw new Error("match failed"); }
    }
}
module Tachyus.Gluon.Schema.DataType {

    export function fromJSON(json: any): DataType {

        return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.DataType", json);
    }
    export function match<T>(value: Tachyus.Gluon.Schema.DataType,
        cont: {
            ArrayType: ((Item: Tachyus.Gluon.Schema.DataType) => T),
            BooleanType: (() => T),
            BytesType: (() => T),
            DateTimeType: (() => T),
            DoubleType: (() => T),
            IntType: (() => T),
            JsonType: (() => T),
            ListType: ((Item: Tachyus.Gluon.Schema.DataType) => T),
            OptionType: ((Item: Tachyus.Gluon.Schema.DataType) => T),
            SequenceType: ((Item: Tachyus.Gluon.Schema.DataType) => T),
            StringDictType: ((Item: Tachyus.Gluon.Schema.DataType) => T),
            StringType: (() => T),
            TupleType: ((Item: Tachyus.Gluon.Schema.DataType[]) => T),
            TypeReference: ((Item: string) => T)
        }): T {

        if (value instanceof ArrayType) { return cont.ArrayType(value.Item); }
        else if (value instanceof BooleanType) { return cont.BooleanType(); }
        else if (value instanceof BytesType) { return cont.BytesType(); }
        else if (value instanceof DateTimeType) { return cont.DateTimeType(); }
        else if (value instanceof DoubleType) { return cont.DoubleType(); }
        else if (value instanceof IntType) { return cont.IntType(); }
        else if (value instanceof JsonType) { return cont.JsonType(); }
        else if (value instanceof ListType) {
            return cont.ListType(value.Item);
        }
        else if (value instanceof OptionType) {
            return cont.OptionType(value.Item);
        }
        else if (value instanceof SequenceType) {
            return cont.SequenceType(value.Item);
        }
        else if (value instanceof StringDictType) {
            return cont.StringDictType(value.Item);
        }
        else if (value instanceof StringType) { return cont.StringType(); }
        else if (value instanceof TupleType) {
            return cont.TupleType(value.Item);
        }
        else if (value instanceof TypeReference) {
            return cont.TypeReference(value.Item);
        } else { throw new Error("match failed"); }
    }
}
module Tachyus.Gluon.Schema.TypeDefinition {

    export function fromJSON(json: any): TypeDefinition {

        return Tachyus.Gluon.Internals.fromJSON("Tachyus.Gluon.Schema.TypeDefinition", json);
    }
    export function match<T>(value: Tachyus.Gluon.Schema.TypeDefinition,
        cont: {
            DefineEnum: ((Item: Tachyus.Gluon.Schema.Enum) => T),
            DefineRecord: ((Item: Tachyus.Gluon.Schema.Record) => T),
            DefineUnion: ((Item: Tachyus.Gluon.Schema.Union) => T)
        }): T {

        if (value instanceof DefineEnum) { return cont.DefineEnum(value.Item); }
        else if (value instanceof DefineRecord) {
            return cont.DefineRecord(value.Item);
        }
        else if (value instanceof DefineUnion) {
            return cont.DefineUnion(value.Item);
        } else { throw new Error("match failed"); }
    }
}
// </BOOTSTRAP-DEFS>

/** Implements the client side of the Gluon connector. */
module Tachyus.Gluon {

    import S = Tachyus.Gluon.Schema;

    // Option<T> support ------------------------------------------------------

    /** Represents optional values, just as F# does. */
    export class Option<T> {

        /** Constructor for internal use. */
        constructor(
            public isSome: boolean,
            public value: T) { }

        /** Unpacks with a default value. */
        withDefault(defaultValue: T): T {
            if (this.isSome) {
                return this.value;
            } else {
                return defaultValue;
            }
        }

        /** Converts to a JSON representation. */
        toJSON(): any {
            if (this.isSome) {
                return [this.value];
            } else {
                return null;
            }
        }
    }

    /** Option operators. */
    export module Option {

        /** Constructs a Some(value) option. */
        export function some<T>(value: T): Option<T> {
            return new Option<T>(true, value);
        }

        /** Constructs a None option. */
        export function none<T>(): Option<T> {
            return new Option(false, undefined);
        }

        /** Recovers an Option<T> from JSON object representation. */
        export function fromJSON<T>(json: any) {
            if (json === null) {
                return none<T>();
            }
        }
    }

    // Dict<T> support --------------------------------------------------------

    export class Dict<T> {
        private data: any = {};

        private check(key: string) {
            if (typeof key !== "string") {
                throw new Error("Invalid or null key");
            }
        }

        containsKey(key: string): boolean {
            this.check(key);
            return this.data.hasOwnProperty(key);
        }

        forEach(visit: (key: string, element: T) => void): void {
            for (var prop in this.data) {
                if (this.data.hasOwnProperty(prop)) {
                    visit(prop, this.data[prop]);
                }
            }
        }

        copy(): Dict<T> {
            var result = new Dict<T>();
            this.forEach((key, el) => result.setAt(key, el));
            return result;
        }

        at(key: string): T {
            this.check(key);
            if (this.data.hasOwnProperty(key)) {
                return this.data[key];
            } else {
                throw new Error("Missing key: " + key);
            }
        }

        tryFind(key: string): Option<T> {
            this.check(key);
            if (this.data.hasOwnProperty(key)) {
                return Option.some(this.data[key]);
            } else {
                return Option.none<T>();
            }
        }

        setAt(key: string, value: T): void {
            this.check(key);
            this.data[key] = value;
        }

        toJSON(): any {
            return this.data;
        }
    }

    // Schema -----------------------------------------------------------------

    module DataType {
        export function defaultMatch<T>(def: T) {
            return {
                ArrayType: (t: S.DataType) => def,
                BooleanType: () => def,
                BytesType: () => def,
                DateTimeType: () => def,
                DoubleType: () => def,
                IntType: () => def,
                JsonType: () => def,
                ListType: (t: S.DataType) => def,
                OptionType: (t: S.DataType) => def,
                SequenceType: (t: S.DataType) => def,
                StringDictType: (t: S.DataType) => def,
                StringType: () => def,
                TupleType: (elements: S.DataType[]) => def,
                TypeReference: (tref: string) => def
            }
        }

        export function children(d: S.DataType): S.DataType[] {
            var m = defaultMatch<S.DataType[]>([]);
            m.ArrayType = x => [x];
            m.ListType = x => [x];
            m.OptionType = x => [x];
            m.SequenceType = x => [x];
            m.StringDictType = x => [x];
            m.TupleType = xs => xs;
            return S.DataType.match(d, m);
        }
    }

    interface Visitor {
        visitDataType(dt: S.DataType): void;
        visitRecord(r: S.Record): void;
        visitUnion(u: S.Union): void;
        visitEnum(e: S.Enum): void;
    }

    function defaultVisitor(): Visitor {
        return {
            visitDataType: t => { },
            visitRecord: r => { },
            visitUnion: u => { },
            visitEnum: e => { }
        };
    }

    function visitDataType(dt: S.DataType, visitor: Visitor) {
        visitor.visitDataType(dt);
        DataType.children(dt).forEach(x => visitDataType(x, visitor));
    }

    function visitTypes(types: S.TypeDefinition[], visitor: Visitor) {
        function visitField(f: S.Field) {
            visitDataType(f.FieldType, visitor);
        }
        function visitRecord(r: S.Record) {
            visitor.visitRecord(r);
            r.RecordFields.forEach(visitField);
        }
        function visitCase(c: S.UnionCase) {
            c.CaseFields.forEach(visitField);
        }
        function visitUnion(u: S.Union) {
            visitor.visitUnion(u);
            u.UnionCases.forEach(visitCase);
        }
        function visitEnum(e: S.Enum) {
            visitor.visitEnum(e);
        }
        function visitTD(td: S.TypeDefinition) {
            S.TypeDefinition.match(td, {
                DefineUnion: u => visitUnion(u),
                DefineRecord: r => visitRecord(r),
                DefineEnum: e => visitEnum(e)
            });
        }
        types.forEach(visitTD);
    }

    function visitServiceMethods(methods: S.Method[], visitor: Visitor) {
        function visitParam(p: S.Parameter) {
            visitDataType(p.ParameterType, visitor);
        }
        function visitMethod(m: S.Method) {
            m.MethodParameters.forEach(visitParam);
            if (m.MethodParameters.length > 1) {
                var t = tupleType(m.MethodParameters.map(p => p.ParameterType));
                visitDataType(t, visitor);
            }
            if (m.MethodReturnType.isSome) {
                visitDataType(m.MethodReturnType.value, visitor);
            }
        }
        methods.forEach(visitMethod);
    }

    function dataTypeKey(dataType: S.DataType): string {
        function key(dataType: S.DataType): any {
            return S.DataType.match<any>(dataType, {
                ArrayType: t => [":array", key(t)],
                BooleanType: () => ":bool",
                BytesType: () => ":bytes",
                DateTimeType: () => ":datetime",
                DoubleType: () => ":double",
                IntType: () => ":int",
                JsonType: () => ":json",
                ListType: t => [":list", key(t)],
                OptionType: t => [":option", key(t)],
                SequenceType: t => [":seq", key(t)],
                StringDictType: t => [":sdict", key(t)],
                StringType: () => ":str",
                TupleType: ts => [":tup"].concat(ts.map(key)),
                TypeReference: t => t
            });
        }
        return JSON.stringify(key(dataType));
    }

    function typeDefName(td: S.TypeDefinition) {
        return S.TypeDefinition.match(td, {
            DefineEnum: e => e.EnumName,
            DefineRecord: r => r.RecordName,
            DefineUnion: u => u.UnionName
        });
    }

    function findTypeDefinition(svc: S.Service, name: string) {
        return svc.TypeDefinitions.filter(x => typeDefName(x) === name)[0];
    }

    // Serialization ----------------------------------------------------------

    interface SerializerFactory {
        getSerializer(dataType: S.DataType): Serializer<any>;
    }

    interface Serializer<T> {
        init(factory: SerializerFactory);
        toJSON(value: T): any;
        fromJSON(json: any): T;
    }

    function idSerializer(): Serializer<any> {
        return {
            init: f => { },
            toJSON: v => v,
            fromJSON: v => v
        };
    }

    var booleanSerializer: Serializer<boolean> =
        idSerializer();

    function serializeNumber(n: number): any {
        if (isFinite(n)) {
            return n;
        } else {
            return String(n);
        }
    }

    function deserializeNumber(json: any): number {
        return Number(json);
    }

    var numberSerializer: Serializer<number> =
        {
            init: f => { },
            toJSON: serializeNumber,
            fromJSON: deserializeNumber
        };

    var dateSerializer: Serializer<Date> =
        {
            init: f => { },
            toJSON: date => {
                var str = date.toISOString();
                // if .unspecified marker set before by Gluon ..
                if ((<any>date).unspecified) {
                    // we remove the timezone marker (trailing "Z")
                    str = str.substring(0, str.length - 1);
                }
                return str;
            },
            fromJSON: (str: string) => {
                // check if timezone marker is given ..
                var unspecified = str.charAt(str.length - 1).toLowerCase() != "z";
                var d = new Date(str);
                // propagate the timezone marker info to allow turnaround
                (<any>d).unspecified = unspecified;
                return d;
            }
        };

    var rawJsonSerializer: Serializer<any> =
        {
            init: f => { },
            toJSON: x => x,
            fromJSON: x => x
        };

    function b64encode(bytes: Uint8Array): string {
        var s: string = "";
        for (var i = 0; i < bytes.length; i++) {
            s = s + String.fromCharCode(bytes[i]);
        }
        return btoa(s);
    }

    function b64decode(b64: string): Uint8Array {
        var input = atob(b64);
        var r = new Uint8Array(input.length);
        for (var i = 0; i < r.length; i++) {
            r[i] = input.charCodeAt(i);
        }
        return r;
    }

    var bytesSerializer: Serializer<Uint8Array> =
        {
            init: f => { },
            toJSON: x => b64encode(x),
            fromJSON: x => b64decode(x)
        };

    var stringSerializer: Serializer<string> =
        idSerializer();

    class ArraySerializer {
        private inner: Serializer<any>;

        constructor(public element: S.DataType) {
        }

        init(factory: SerializerFactory) {
            this.inner = factory.getSerializer(this.element);
        }

        toJSON(value: any[]) {
            return value.map(x => this.inner.toJSON(x));
        }

        fromJSON(json: any[]): any[] {
            return json.map(x => this.inner.fromJSON(x));
        }
    }

    class DictSerializer {
        private inner: Serializer<any>;

        constructor(public element: S.DataType) { }

        init(factory: SerializerFactory) {
            this.inner = factory.getSerializer(this.element);
        }

        toJSON(dict: Dict<any>): any {
            var result = {};
            dict.forEach((key, value) => {
                result[key] = this.inner.toJSON(value);
            });
            return result;
        }

        fromJSON(json: any): Dict<any> {
            var result = new Dict<any>();
            for (var key in json) {
                result.setAt(key, this.inner.fromJSON(json[key]));
            }
            return result;
        }
    }

    class OptionSerializer {
        private inner: Serializer<any>;

        constructor(public element: S.DataType) { }

        init(factory: SerializerFactory) {
            this.inner = factory.getSerializer(this.element);
        }

        toJSON(opt: Option<any>): any {
            if (opt.isSome) {
                return [this.inner.toJSON(opt.value)];
            } else {
                return null;
            }
        }

        fromJSON(json: any[]): Option<any> {
            if (json === null) {
                return Option.none();
            } else {
                return Option.some(this.inner.fromJSON(json[0]));
            }
        }
    }

    class TupleSerializer {
        private inner: Serializer<any>[];

        constructor(public elements: S.DataType[]) {
        }

        length(): number {
            return this.elements.length;
        }

        init(factory: SerializerFactory) {
            this.inner = this.elements.map(x => factory.getSerializer(x));
        }

        toJSON(tup: any[]): any[] {
            var n = this.length();
            var res = new Array(n);
            for (var i = 0; i < n; i++) {
                res[i] = this.inner[i].toJSON(tup[i]);
            }
            return res;
        }

        fromJSON(json: any[]): any[] {
            var n = this.length();
            var res = new Array(n);
            for (var i = 0; i < n; i++) {
                res[i] = this.inner[i].fromJSON(json[i]);
            }
            return res;
        }
    }

    function buildDataTypeSerializer(dt: S.DataType): Serializer<any> {
        function arrayLike(t) {
            return new ArraySerializer(t);
        }
        function tref(x: string): Serializer<any> {
            throw new Error("Invalid DataType");
        }
        return S.DataType.match<Serializer<any>>(dt, {
            ArrayType: arrayLike,
            ListType: arrayLike,
            SequenceType: arrayLike,
            BooleanType: () => booleanSerializer,
            BytesType: () => bytesSerializer,
            DateTimeType: () => dateSerializer,
            DoubleType: () => numberSerializer,
            IntType: () => numberSerializer,
            JsonType: () => rawJsonSerializer,
            OptionType: (t) => new OptionSerializer(t),
            StringDictType: (t) => new DictSerializer(t),
            StringType: () => stringSerializer,
            TupleType: (ts) => new TupleSerializer(ts),
            TypeReference: tref
        });
    }

    /// Builds instances of a specific type based on a boxed argument list.
    export interface IActivator {
        createInstance(args: any[]): any;
        typeId: string;
    }

    class TypeRegistry {

        /// Activators indexed by type identity.
        private activators: Dict<IActivator>;

        constructor() {
            this.activators = new Dict<IActivator>();
        }

        registerActivators(activators: IActivator[]) {
            activators.forEach(a => {
                this.activators.setAt(a.typeId, a);
            });
        }

        fullCaseName(typeId: string, caseName: string) {
            var i = typeId.lastIndexOf('.');
            if (i === -1) {
                return caseName;
            } else {
                return typeId.substr(0, i) + '.' + caseName;
            }
        }

        createRecord(typeId: string, args: any[]): any {
            return this.activators.at(typeId).createInstance(args);
        }

        createUnion(typeId: string, caseName: string, args: any[]): any {
            return this.activators.at(this.fullCaseName(typeId, caseName)).createInstance(args);
        }
    }

    class EnumSerializer {
        constructor() { }
        init(factory: SerializerFactory) { }
        toJSON(value: any) { return value; }
        fromJSON(json: any) { return json; }
    }

    class RecordSerializer {
        fields: { name: string; ser: Serializer<any> }[];

        constructor(
            public record: S.Record,
            public typeRegistry: TypeRegistry) { }

        init(factory: SerializerFactory) {
            this.fields = this.record.RecordFields.map(f => {
                return {
                    name: f.FieldName,
                    ser: factory.getSerializer(f.FieldType)
                }
            });
        }

        toJSON(value: any) {
            var result = {};
            this.fields.forEach(fld => {
                result[fld.name] = fld.ser.toJSON(value[fld.name]);
            });
            return result;
        }

        fromJSON(json: any) {
            var len = this.fields.length;
            var args = new Array(len);
            for (var i = 0; i < len; i++) {
                var fld = this.fields[i];
                args[i] = fld.ser.fromJSON(json[fld.name]);
            }
            return this.typeRegistry.createRecord(
                this.record.RecordName, args);
        }
    }

    class FieldInfo {
        fieldName: string;
        fieldSerializer: Serializer<any>
    }

    class CaseInfo {
        caseName: string;
        fields: FieldInfo[];
    }

    class UnionSerializer {
        cases: CaseInfo[];

        constructor(public union: S.Union, public typeRegistry: TypeRegistry) {
        }

        init(factory: SerializerFactory) {
            this.cases = this.union.UnionCases.map(c => {
                return {
                    caseName: c.CaseName,
                    fields: c.CaseFields.map(f => {
                        return {
                            fieldName: f.FieldName,
                            fieldSerializer: factory.getSerializer(f.FieldType)
                        };
                    })
                };
            });
        }

        findCase(name: string): CaseInfo {
            for (var i = 0; i < this.cases.length; i++) {
                var c = this.cases[i];
                if (c.caseName === name) {
                    return c;
                }
            }
        }

        toJSON(value: any): any {
            var tag: string = value.tag();
            var uCase = this.findCase(tag);
            var res = new Array(uCase.fields.length + 1);
            res[0] = tag;
            for (var i = 0; i < uCase.fields.length; i++) {
                var f = uCase.fields[i];
                var v = value[f.fieldName];
                res[i + 1] = f.fieldSerializer.toJSON(v);
            }
            return res;
        }

        fromJSON(json: any): any {
            var c = this.findCase(json[0]);
            var args = new Array(json.length - 1);
            for (var i = 0; i < args.length; i++) {
                var fld = c.fields[i];
                args[i] = fld.fieldSerializer.fromJSON(json[i + 1]);
            }
            return this.typeRegistry.createUnion(
                this.union.UnionName,
                c.caseName,
                args);
        }
    }

    function typeReference(typeId: string): S.DataType {
        return new S.TypeReference(typeId);
    }

    function tupleType(dataTypes: S.DataType[]): S.DataType {
        return new S.TupleType(dataTypes);
    }

    class SerializerService {
        private dict: Dict<Serializer<any>>;
        private registry: TypeRegistry;

        constructor() {
            this.dict = new Dict<Serializer<any>>();
            this.registry = new TypeRegistry();
        }

        private add(dt: S.DataType, ser: Serializer<any>) {
            var key = dataTypeKey(dt);
            this.dict.setAt(key, ser);
        }

        getSerializer(dt: S.DataType): Serializer<any> {
            var key = dataTypeKey(dt);
            return this.dict.at(key);
        }

        private contains(dt: S.DataType) {
            var key = dataTypeKey(dt);
            return this.dict.containsKey(key);
        }

        private init() {
            this.dict.forEach((k, ser) => {
                ser.init(this);
            });
        }

        registerActivators(activators: IActivator[]) {
            this.registry.registerActivators(activators);
        }

        private createVisitor() {
            var vis = defaultVisitor();
            var add = (dt: S.DataType) => {
                if (!this.contains(dt)) {
                    this.add(dt, buildDataTypeSerializer(dt));
                }
            }
            vis.visitDataType = dt => {
                if (!(dt instanceof S.TypeReference)) {
                    add(dt);
                }
            };
            vis.visitRecord = r => {
                var dt = typeReference(r.RecordName);
                this.add(dt, new RecordSerializer(r, this.registry));
            };
            vis.visitUnion = u => {
                var dt = typeReference(u.UnionName);
                this.add(dt, new UnionSerializer(u, this.registry));
            };
            vis.visitEnum = e => {
                var dt = typeReference(e.EnumName);
                console.log("visitEnum", e, dt);
                this.add(dt, new EnumSerializer());
            };
            return vis;
        }

        registerTypes(types: S.TypeDefinition[]) {
            visitTypes(types, this.createVisitor());
            this.init();
        }

        registerServiceMethods(methods: S.Method[]) {
            visitServiceMethods(methods, this.createVisitor());
            this.init();
        }
    }

    /** Client for the HTTP transport. */
    export class Client {

        /** URL prefix to direct method calls to. */
        public prefix: string;

        /** HTTP client to use, defaults to JQuery. */
        public httpClient: IHttpClient;

        /** Constructs a client, with an optional URL prefix. */
        constructor(prefix?: string) {
            if (!prefix) {
                this.prefix = "/gluon-api";
            } else {
                this.prefix = prefix;
            }
            this.httpClient = new JQueryClient();
        }
    }

    /** Proxies a remote method. */
    export interface RemoteMethod<T> {
        (client: Client): T;
    }

    interface IHttpClient {
        httpGet(url: string, queryParams: any, parseJsonResponse: (json: any) => any): JQueryPromise<any>;
        httpCall(httpMethod: string, url: string, jsonRequest: string, parseJsonResponse: (json: any) => any): JQueryPromise<any>;
    }

    class JQueryClient implements IHttpClient {
        constructor() { }

        httpGet(url, queryParams, parseJsonResponse) {
            return jQuery.ajax({
                "url": url,
                "type": "get",
                "data": queryParams
            }).then(x => parseJsonResponse(x));
        }

        httpCall(httpMethod, url, jsonRequest, parseJsonResponse) {
            var ajaxParams: any = { "url": url, "type": httpMethod };
            if (jsonRequest !== null) {
                ajaxParams.data = jsonRequest;
                ajaxParams.dataType = "json";
                ajaxParams.contentType = "application/json";
            }
            return jQuery.ajax(ajaxParams).then(x => parseJsonResponse(x));
        }
    }

    module Remoting {

        function verbName(m: S.HttpMethod) {
            return S.HttpMethod.match(m, {
                Get: () => "get",
                Delete: () => "delete",
                Post: () => "post",
                Put: () => "put"
            });
        }

        function verb(conv: S.CallingConvention): S.HttpMethod {
            return S.CallingConvention.match(conv, {
                HttpCallingConvention: (m, path) => m
            });
        }

        function localPath(conv: S.CallingConvention): string {
            return S.CallingConvention.match(conv, {
                HttpCallingConvention: (m, path) => path
            });
        }

        function buildUrl(cli: Client, m: S.Method) {
            return cli.prefix + "/" + localPath(m.CallingConvention);
        }

        function buildQueryParams(cli: Client, proxy: RemoteMethodProxy, args: any[]): any {
            var query = {};
            proxy.innerMethod.MethodParameters.forEach((p, i) => {
                query[p.ParameterName] = JSON.stringify(proxy.parameterSerializers[i].toJSON(args[i]));
            });
            return query;
        }

        function buildJsonRequest(cli: Client, proxy: RemoteMethodProxy, args: any[]) {
            var data;
            if (proxy.arity == 0) {
                return null;
            } else if (proxy.arity == 1) {
                data = args[0];
            } else {
                data = args;
            }
            return JSON.stringify(proxy.jointParametersSerializer.toJSON(data));
        }

        export function remoteCall(cli: Client, proxy: RemoteMethodProxy, args: any[]): JQueryPromise<any> {
            function parseJsonResponse(resp: any) {
                if (proxy.doesReturn) {
                    var out = proxy.returnTypeSerializer.fromJSON(resp);
                    return out;
                } else {
                    return resp;
                }
            }
            var url = buildUrl(cli, proxy.innerMethod);
            var httpMethod = verb(proxy.innerMethod.CallingConvention);
            if (httpMethod instanceof S.Get) {
                var queryParams = buildQueryParams(cli, proxy, args);
                return cli.httpClient.httpGet(url, queryParams, parseJsonResponse);
            } else {
                var jsonRequest = buildJsonRequest(cli, proxy, args);
                return cli.httpClient.httpCall(verbName(httpMethod), url, jsonRequest, parseJsonResponse);
            }
        }

    }

    class RemoteMethodProxy {
        public arity: number;
        public doesReturn: boolean;
        public parameterSerializers: Serializer<any>[];
        public jointParametersSerializer: Serializer<any>;
        public returnTypeSerializer: Serializer<any>;
        public innerMethod: S.Method;

        constructor(factory: SerializerFactory, m: S.Method) {
            this.innerMethod = m;
            this.arity = m.MethodParameters.length;
            switch (this.arity) {
                case 0:
                    break;
                case 1:
                    var s0 = factory.getSerializer(m.MethodParameters[0].ParameterType);
                    this.jointParametersSerializer = s0;
                    this.parameterSerializers = [s0];
                    break;
                default:
                    this.parameterSerializers = m.MethodParameters.map(p =>
                        factory.getSerializer(p.ParameterType));
                    this.jointParametersSerializer = factory.getSerializer(tupleType(m.MethodParameters.map(p =>
                        p.ParameterType)));
                    break;
            }
            if (m.MethodReturnType.isSome) {
                this.doesReturn = true;
                this.returnTypeSerializer = factory.getSerializer(m.MethodReturnType.value);
            } else {
                this.doesReturn = false;
            }
        }

        call(cli: Client, args: any[]): any {
            return Remoting.remoteCall(cli, this, args);
        }
    }

    class MethodBuilder {
        private table: Dict<RemoteMethodProxy>;

        constructor(public factory: SerializerFactory) {
            this.table = new Dict<RemoteMethodProxy>();
        }

        registerService(service: S.Service) {
            service.Methods.forEach(m => {
                var proxy = new RemoteMethodProxy(this.factory, m);
                this.table.setAt(m.MethodName, proxy);
            });
        }

        getProxy(name: string): RemoteMethodProxy {
            return this.table.at(name);
        }

        remoteMethod<T>(name: string): RemoteMethod<T> {
            return (client: Client) => {
                var proxy = this.getProxy(name);
                function call() {
                    var args: any = arguments;
                    return proxy.call(client, args);
                }
                return <any>call;
            };
        }
    }

    module RawSchemaJsonParser {

        function at(json: any, pos: number) {
            return json[pos + 1];
        }

        function rawCaseFields(json: any): any[] {
            var j: any[] = json;
            return j.slice(1);
        }

        function tag(json: any) {
            return json[0];
        }

        function dataType(json: any): S.DataType {
            switch (tag(json)) {
                case "ArrayType":
                    return new S.ArrayType(dataType(at(json, 0)))
                case "BooleanType":
                    return new S.BooleanType();
                case "BytesType":
                    return new S.BytesType();
                case "DateTimeType":
                    return new S.DateTimeType();
                case "DoubleType":
                    return new S.DoubleType();
                case "IntType":
                    return new S.IntType();
                case "JsonType":
                    return new S.JsonType();
                case "ListType":
                    return new S.ListType(dataType(at(json, 0)));
                case "OptionType":
                    return new S.OptionType(dataType(at(json, 0)))
                case "SequenceType":
                    return new S.SequenceType(dataType(at(json, 0)));
                case "StringDictType":
                    return new S.StringDictType(dataType(at(json, 0)));
                case "StringType":
                    return new S.StringType();
                case "TupleType":
                    return new S.TupleType(at(json, 0).map(dataType));
                case "TypeReference":
                    return new S.TypeReference(at(json, 0));
                default:
                    throw new Error("failed to parse a data type");
            }
        }

        function field(json: any): S.Field {
            return new S.Field(json.FieldName, dataType(json.FieldType));
        }

        function record(json: any): S.Record {
            return new S.Record(json.RecordName, json.RecordFields.map(field));
        }

        function unionCase(json: any): S.UnionCase {
            return new S.UnionCase(json.CaseName, json.CaseFields.map(field));
        }

        function union(json: any): S.Union {
            return new S.Union(json.UnionName, json.UnionCases.map(unionCase));
        }

        function enumCase(json: any): S.EnumCase {
            return new S.EnumCase(json.EnumCaseName, json.EnumCaseValue);
        }

        function parseEnum(json: any): S.Enum {
            return new S.Enum(json.EnumName, json.EnumCases.map(enumCase));
        }

        export function parseTypeDefinition(json: any): S.TypeDefinition {
            switch (tag(json)) {
                case "DefineRecord": return new S.DefineRecord(record(at(json, 0)));
                case "DefineUnion": return new S.DefineUnion(union(at(json, 0)));
                case "DefineEnum": return new S.DefineEnum(parseEnum(at(json, 0)));
                default: throw new Error("error parsing type definition");
            }
        }

        function parameter(json: any): S.Parameter {
            return new S.Parameter(json.ParameterName, dataType(json.ParameterType));
        }

        function httpMethod(json: any): S.HttpMethod {
            switch (tag(json)) {
                case "Delete": return new S.Delete();
                case "Get": return new S.Get();
                case "Post": return new S.Post();
                case "Put": return new S.Put();
                default: throw new Error("error pasring http method");
            }
        }

        function callingConvention(json: any): S.CallingConvention {
            switch (tag(json)) {
                case "HttpCallingConvention": return new S.HttpCallingConvention(httpMethod(at(json, 0)), at(json, 1));
                default: throw new Error("error parsing calling convention");
            }
        }

        function opt<T>(json: any, parse: (json: any) => T): Option<T> {
            if (json === null) {
                return Option.none<T>();
            } else {
                return Option.some<T>(parse(json[0]));
            }
        }

        function method(json: any): S.Method {
            var cc = callingConvention(json.CallingConvention);
            var methodName = json.MethodName;
            var methodParameters = json.MethodParameters.map(parameter);
            var methodReturnType = opt<S.DataType>(json.MethodReturnType, dataType);
            return new S.Method(cc, methodName, methodParameters, methodReturnType);
        }

        export function parseServiceSchema(json: any): S.Service {
            return new S.Service(json.Methods.map(method), json.TypeDefinitions.map(parseTypeDefinition));
        }
    }

    /** Infrastructure methods called by generated code. */
    export module Internals {

        // TODO: remove this global state.
        var serializerService = new SerializerService();
        var methodBuilder = new MethodBuilder(serializerService);

        export function toJSON(typeRef: string, value: any): any {
            return serializerService.getSerializer(new S.TypeReference(typeRef)).toJSON(value);
        }

        export function fromJSON(typeRef: string, json: any): any {
            return serializerService.getSerializer(new S.TypeReference(typeRef)).fromJSON(json);
        }

        export function registerActivators(raw: any) {
            var activators: IActivator[] = [];
            function addActivator(typeId: string, func: any) {
                activators.push({
                    typeId: key,
                    createInstance: args => func.apply(null, args)
                });
            }
            for (var key in raw) {
                addActivator(key, raw[key]);
            }
            serializerService.registerActivators(activators);
        }

        export function registerTypeDefinitions(rawTypeDefJson: any[]): void {
            var typeDefs = rawTypeDefJson.map(RawSchemaJsonParser.parseTypeDefinition);
            serializerService.registerTypes(typeDefs);
        }

        export function registerService(rawServiceJson: any): void {
            var service = RawSchemaJsonParser.parseServiceSchema(rawServiceJson);
            serializerService.registerTypes(service.TypeDefinitions);
            serializerService.registerServiceMethods(service.Methods);
            methodBuilder.registerService(service);
        }

        export function remoteMethod<T>(name: string): RemoteMethod<T> {
            return methodBuilder.remoteMethod<T>(name);
        }
    }
}

// <BOOTSTRAP-INIT>
Tachyus.Gluon.Internals.registerActivators({
    "Tachyus.Gluon.Schema.Delete": () => new Tachyus.Gluon.Schema.Delete(),
    "Tachyus.Gluon.Schema.Get": () => new Tachyus.Gluon.Schema.Get(),
    "Tachyus.Gluon.Schema.Post": () => new Tachyus.Gluon.Schema.Post(),
    "Tachyus.Gluon.Schema.Put": () => new Tachyus.Gluon.Schema.Put(),
    "Tachyus.Gluon.Schema.HttpCallingConvention": (a, b) => new Tachyus.Gluon.Schema.HttpCallingConvention(a, b),
    "Tachyus.Gluon.Schema.ArrayType": (a) => new Tachyus.Gluon.Schema.ArrayType(a),
    "Tachyus.Gluon.Schema.BooleanType": () => new Tachyus.Gluon.Schema.BooleanType(),
    "Tachyus.Gluon.Schema.BytesType": () => new Tachyus.Gluon.Schema.BytesType(),
    "Tachyus.Gluon.Schema.DateTimeType": () => new Tachyus.Gluon.Schema.DateTimeType(),
    "Tachyus.Gluon.Schema.DoubleType": () => new Tachyus.Gluon.Schema.DoubleType(),
    "Tachyus.Gluon.Schema.IntType": () => new Tachyus.Gluon.Schema.IntType(),
    "Tachyus.Gluon.Schema.JsonType": () => new Tachyus.Gluon.Schema.JsonType(),
    "Tachyus.Gluon.Schema.ListType": (a) => new Tachyus.Gluon.Schema.ListType(a),
    "Tachyus.Gluon.Schema.OptionType": (a) => new Tachyus.Gluon.Schema.OptionType(a),
    "Tachyus.Gluon.Schema.SequenceType": (a) => new Tachyus.Gluon.Schema.SequenceType(a),
    "Tachyus.Gluon.Schema.StringDictType": (a) => new Tachyus.Gluon.Schema.StringDictType(a),
    "Tachyus.Gluon.Schema.StringType": () => new Tachyus.Gluon.Schema.StringType(),
    "Tachyus.Gluon.Schema.TupleType": (a) => new Tachyus.Gluon.Schema.TupleType(a),
    "Tachyus.Gluon.Schema.TypeReference": (a) => new Tachyus.Gluon.Schema.TypeReference(a),
    "Tachyus.Gluon.Schema.Parameter": (a, b) => new Tachyus.Gluon.Schema.Parameter(a, b),
    "Tachyus.Gluon.Schema.Method": (a, b, c, d) => new Tachyus.Gluon.Schema.Method(a, b, c, d),
    "Tachyus.Gluon.Schema.EnumCase": (a, b) => new Tachyus.Gluon.Schema.EnumCase(a, b),
    "Tachyus.Gluon.Schema.Enum": (a, b) => new Tachyus.Gluon.Schema.Enum(a, b),
    "Tachyus.Gluon.Schema.Field": (a, b) => new Tachyus.Gluon.Schema.Field(a, b),
    "Tachyus.Gluon.Schema.Record": (a, b) => new Tachyus.Gluon.Schema.Record(a, b),
    "Tachyus.Gluon.Schema.UnionCase": (a, b) => new Tachyus.Gluon.Schema.UnionCase(a, b),
    "Tachyus.Gluon.Schema.Union": (a, b) => new Tachyus.Gluon.Schema.Union(a, b),
    "Tachyus.Gluon.Schema.DefineEnum": (a) => new Tachyus.Gluon.Schema.DefineEnum(a),
    "Tachyus.Gluon.Schema.DefineRecord": (a) => new Tachyus.Gluon.Schema.DefineRecord(a),
    "Tachyus.Gluon.Schema.DefineUnion": (a) => new Tachyus.Gluon.Schema.DefineUnion(a),
    "Tachyus.Gluon.Schema.Service": (a, b) => new Tachyus.Gluon.Schema.Service(a, b)
});
Tachyus.Gluon.Internals.registerTypeDefinitions([["DefineUnion", { "UnionName": "Tachyus.Gluon.Schema.HttpMethod", "UnionCases": [{ "CaseName": "Delete", "CaseFields": [] }, { "CaseName": "Get", "CaseFields": [] }, { "CaseName": "Post", "CaseFields": [] }, { "CaseName": "Put", "CaseFields": [] }] }], ["DefineUnion", { "UnionName": "Tachyus.Gluon.Schema.CallingConvention", "UnionCases": [{ "CaseName": "HttpCallingConvention", "CaseFields": [{ "FieldName": "Item1", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.HttpMethod"] }, { "FieldName": "path", "FieldType": ["StringType"] }] }] }], ["DefineUnion", { "UnionName": "Tachyus.Gluon.Schema.DataType", "UnionCases": [{ "CaseName": "ArrayType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.DataType"] }] }, { "CaseName": "BooleanType", "CaseFields": [] }, { "CaseName": "BytesType", "CaseFields": [] }, { "CaseName": "DateTimeType", "CaseFields": [] }, { "CaseName": "DoubleType", "CaseFields": [] }, { "CaseName": "IntType", "CaseFields": [] }, { "CaseName": "JsonType", "CaseFields": [] }, { "CaseName": "ListType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.DataType"] }] }, { "CaseName": "OptionType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.DataType"] }] }, { "CaseName": "SequenceType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.DataType"] }] }, { "CaseName": "StringDictType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.DataType"] }] }, { "CaseName": "StringType", "CaseFields": [] }, { "CaseName": "TupleType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.DataType"]] }] }, { "CaseName": "TypeReference", "CaseFields": [{ "FieldName": "Item", "FieldType": ["StringType"] }] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.Parameter", "RecordFields": [{ "FieldName": "ParameterName", "FieldType": ["StringType"] }, { "FieldName": "ParameterType", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.DataType"] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.Method", "RecordFields": [{ "FieldName": "CallingConvention", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.CallingConvention"] }, { "FieldName": "MethodName", "FieldType": ["StringType"] }, { "FieldName": "MethodParameters", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.Parameter"]] }, { "FieldName": "MethodReturnType", "FieldType": ["OptionType", ["TypeReference", "Tachyus.Gluon.Schema.DataType"]] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.EnumCase", "RecordFields": [{ "FieldName": "EnumCaseName", "FieldType": ["StringType"] }, { "FieldName": "EnumCaseValue", "FieldType": ["IntType"] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.Enum", "RecordFields": [{ "FieldName": "EnumName", "FieldType": ["StringType"] }, { "FieldName": "EnumCases", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.EnumCase"]] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.Field", "RecordFields": [{ "FieldName": "FieldName", "FieldType": ["StringType"] }, { "FieldName": "FieldType", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.DataType"] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.Record", "RecordFields": [{ "FieldName": "RecordName", "FieldType": ["StringType"] }, { "FieldName": "RecordFields", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.Field"]] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.UnionCase", "RecordFields": [{ "FieldName": "CaseName", "FieldType": ["StringType"] }, { "FieldName": "CaseFields", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.Field"]] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.Union", "RecordFields": [{ "FieldName": "UnionName", "FieldType": ["StringType"] }, { "FieldName": "UnionCases", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.UnionCase"]] }] }], ["DefineUnion", { "UnionName": "Tachyus.Gluon.Schema.TypeDefinition", "UnionCases": [{ "CaseName": "DefineEnum", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.Enum"] }] }, { "CaseName": "DefineRecord", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.Record"] }] }, { "CaseName": "DefineUnion", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Tachyus.Gluon.Schema.Union"] }] }] }], ["DefineRecord", { "RecordName": "Tachyus.Gluon.Schema.Service", "RecordFields": [{ "FieldName": "Methods", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.Method"]] }, { "FieldName": "TypeDefinitions", "FieldType": ["ListType", ["TypeReference", "Tachyus.Gluon.Schema.TypeDefinition"]] }] }]]);
// </BOOTSTRAP-INIT>
