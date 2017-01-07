// Copyright 2015-2016 Tachyus Corp.
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
module Gluon.Schema {
  
    export type HttpMethod = "Delete" | "Get" | "Post" | "Put";

    export interface HttpCallingConvention {
        tag: "HttpCallingConvention";
        Item1: Gluon.Schema.HttpMethod;
        path: string;
    }
    export type CallingConvention = HttpCallingConvention;

    export interface ArrayType {
        tag: "ArrayType";
        Item: Gluon.Schema.DataType;
    }
    export interface BooleanType {
        tag: "BooleanType";
    }
    export interface BytesType {
        tag: "BytesType";
    }
    export interface DateTimeType {
        tag: "DateTimeType";
    }
    export interface DoubleType {
        tag: "DoubleType";
    }
    export interface IntType {
        tag: "IntType";
    }
    export interface JsonType {
        tag: "JsonType";
    }
    export interface ListType {
        tag: "ListType";
        Item: Gluon.Schema.DataType;
    }
    export interface OptionType {
        tag: "OptionType";
        Item: Gluon.Schema.DataType;
    }
    export interface SequenceType {
        tag: "SequenceType";
        Item: Gluon.Schema.DataType;
    }
    export interface StringDictType {
        tag: "StringDictType";
        Item: Gluon.Schema.DataType;
    }
    export interface StringType {
        tag: "StringType";
    }
    export interface TupleType {
        tag: "TupleType";
        Item: Gluon.Schema.DataType[];
    }
    export interface TypeReference {
        tag: "TypeReference";
        Item: string;
    }
    export type DataType = ArrayType | BooleanType | BytesType | DateTimeType | DoubleType | IntType | JsonType | ListType | OptionType | SequenceType | StringDictType | StringType | TupleType | TypeReference;

    export class Parameter {
        ParameterName: string;
        ParameterType: Gluon.Schema.DataType;
    }
    export class Method {
        CallingConvention: Gluon.Schema.CallingConvention;
        MethodName: string;
        MethodParameters: Gluon.Schema.Parameter[];
        MethodReturnType: Gluon.Option<Gluon.Schema.DataType>;
    }
    export class EnumCase {
        EnumCaseName: string;
        EnumCaseValue: number;
    }
    export class Enum {
        EnumName: string;
        EnumCases: Gluon.Schema.EnumCase[];
    }
    export class Field {
        FieldName: string;
        FieldType: Gluon.Schema.DataType;
    }
    export class Record {
        RecordName: string;
        RecordFields: Gluon.Schema.Field[];
    }
    export class UnionCase {
        CaseName: string;
        CaseFields: Gluon.Schema.Field[];
    }
    export class Union {
        UnionName: string;
        UnionCases: Gluon.Schema.UnionCase[];
    }

    export interface DefineEnum {
        tag: "DefineEnum";
        Item: Gluon.Schema.Enum;
    }
    export interface DefineRecord {
        tag: "DefineRecord";
        Item: Gluon.Schema.Record;
    }
    export interface DefineUnion {
        tag: "DefineUnion";
        Item: Gluon.Schema.Union;
    }
    export type TypeDefinition = DefineEnum | DefineRecord | DefineUnion;

    export interface Service {
        Methods: Gluon.Schema.Method[];
        TypeDefinitions: Gluon.Schema.TypeDefinition[];
    }
}
// </BOOTSTRAP-DEFS>

/** Implements the client side of the Gluon connector. */
module Gluon {

    import S = Gluon.Schema;

    // Option<T> support ------------------------------------------------------

    export interface Some<T> {
        isSome: true;
        value: T;
    }
    export interface None<T> {
        isSome: false;
    }
    /** Represents optional values, just as F# does. */
    export type Option<T> = Some<T> | None<T>;

    /** Option operators. */
    export module Option {
        /** Constructs a Some(value) option. */
        export function some<T>(value: T): Option<T> {
            return { isSome: true, value };
        }

        /** Constructs a None option. */
        export function none<T>(): Option<T> {
            return { isSome: false };
        }

        /** Recovers an Option<T> from JSON object representation. */
        export function fromJSON<T>(json: any) {
            if (json === null) {
                return none<T>();
            }
        }

        /** Converts to a JSON representation. */
        export function toJSON<T>(option: Option<T>): any {
            if (option.isSome) {
                return [option.value];
            } else {
                return null;
            }
        }

        /** Unpacks with a default value. */
        export function withDefault<T>(option: Option<T>, defaultValue: T): T {
            if (option.isSome) {
                return option.value;
            } else {
                return defaultValue;
            }
        }
    }

    // Dict<T> support --------------------------------------------------------

    export class Dict<T> {
        private data: {[key: string]: T} = {};

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

        toJSON(): {[key: string]: T} {
            return this.data;
        }
    }

    // Schema -----------------------------------------------------------------

    module DataType {
        export function children(d: S.DataType): S.DataType[] {
            switch (d.tag) {
                case "ArrayType": return [d.Item];
                case "ListType": return [d.Item];
                case "OptionType": return [d.Item];
                case "SequenceType": return [d.Item];
                case "StringDictType": return [d.Item];
                case "TupleType": return d.Item;
                default: return [];
            }
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
            switch (td.tag) {
                case "DefineUnion": return visitUnion(td.Item);
                case "DefineRecord": return visitRecord(td.Item);
                case "DefineEnum": return visitEnum(td.Item)
                default: throw new Error("match failed");
            }
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
            switch (dataType.tag) {
                case "ArrayType": return [":array", key(dataType.Item)];
                case "BooleanType": return ":bool";
                case "BytesType": return ":bytes";
                case "DateTimeType": return ":datetime";
                case "DoubleType": return ":double";
                case "IntType": return ":int";
                case "JsonType": return ":json";
                case "ListType": return [":list", key(dataType.Item)];
                case "OptionType": return [":option", key(dataType.Item)];
                case "SequenceType": return [":seq", key(dataType.Item)];
                case "StringDictType": return [":sdict", key(dataType.Item)];
                case "StringType": return ":str";
                case "TupleType": return [":tup"].concat(dataType.Item.map(i => key(i)));
                case "TypeReference": return dataType.Item;
                default: throw new Error("match failed");
            }
        }
        return JSON.stringify(key(dataType));
    }

    function typeDefName(td: S.TypeDefinition) {
        switch (td.tag) {
            case "DefineEnum": return td.Item.EnumName;
            case "DefineRecord": return td.Item.RecordName;
            case "DefineUnion": return td.Item.UnionName;
            default: throw new Error("match failed");
        }
    }

    function findTypeDefinition(svc: S.Service, name: string) {
        return svc.TypeDefinitions.filter(x => typeDefName(x) === name)[0];
    }

    // Serialization ----------------------------------------------------------

    interface SerializerFactory {
        getSerializer<T>(dataType: S.DataType): Serializer<T>;
    }

    interface Serializer<T> {
        init(factory: SerializerFactory): any;
        toJSON(value: T): any;
        fromJSON(json: any): T;
    }

    var booleanSerializer: Serializer<boolean> =
        {
            init: f => { },
            toJSON: x => x,
            fromJSON: x => x
        };

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
        {
            init: f => { },
            toJSON: x => x,
            fromJSON: x => x
        };

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
            var result: {[key: string]: any} = {};
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
        switch (dt.tag) {
            case "ArrayType": return new ArraySerializer(dt.Item);
            case "ListType": return new ArraySerializer(dt.Item);
            case "SequenceType": return new ArraySerializer(dt.Item);
            case "BooleanType": return booleanSerializer;
            case "BytesType": return bytesSerializer;
            case "DateTimeType": return dateSerializer;
            case "DoubleType": return numberSerializer;
            case "IntType": return numberSerializer;
            case "JsonType": return rawJsonSerializer;
            case "OptionType": return new OptionSerializer(dt.Item);
            case "StringDictType": return new DictSerializer(dt.Item);
            case "StringType": return stringSerializer;
            case "TupleType": return new TupleSerializer(dt.Item);
            default: throw new Error("Invalid DataType");
        }
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
            var result: {[key: string]: any} = {};
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

        findCase(name: string): CaseInfo | undefined {
            for (var i = 0; i < this.cases.length; i++) {
                var c = this.cases[i];
                if (c.caseName === name) {
                    return c;
                }
            }
        }

        toJSON(value: any): any[] | null {
            const isStringLiteralUnion = typeof value === "string";
            var tag: string = isStringLiteralUnion ? value : value.tag;
            var uCase = this.findCase(tag);
            console.log("Union.toJSON", tag, uCase, isStringLiteralUnion);
            if (uCase !== undefined) {
                if (isStringLiteralUnion) {
                    console.log("Union.toJSON result", [tag]);
                    return [tag];
                } else {
                    var res = new Array(uCase.fields.length + 1);
                    res[0] = tag;
                    for (var i = 0; i < uCase.fields.length; i++) {
                        var f = uCase.fields[i];
                        var v = value[f.fieldName];
                        res[i + 1] = f.fieldSerializer.toJSON(v);
                    }
                    console.log("Union.toJSON result", res);
                    return res;
                }
            }
            console.log("Union.toJSON result", null);
            return null;
        }

        fromJSON(json: any): any | null {
            var c = this.findCase(json[0]);
            if (c !== undefined) {
                var args = new Array(json.length - 1);
                for (var i = 0; i < args.length; i++) {
                    var fld = c.fields[i];
                    args[i] = fld.fieldSerializer.fromJSON(json[i + 1]);
                }
                return this.typeRegistry.createUnion(this.union.UnionName, c.caseName, args);
            } else {
                return null;
            }
        }
    }

    function typeReference(typeId: string): S.DataType {
        return { tag: "TypeReference", Item: typeId };
    }

    function tupleType(dataTypes: S.DataType[]): S.DataType {
        return { tag: "TupleType", Item: dataTypes };
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
                if (dt.tag !== "TypeReference") {
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

        /** Constructs a client, with an optional URL prefix. */
        constructor(public httpClient: IHttpClient = new JQueryClient(), prefix?: string) {
            if (!prefix) {
                this.prefix = "/gluon-api";
            } else {
                this.prefix = prefix;
            }
        }
    }

    /** Proxies a remote method. */
    export interface RemoteMethod<T> {
        (client: Client): T;
    }

    export interface IHttpClient {
        httpGet<T>(url: string, queryParams: {[key:string]: string}, parseJsonResponse: (json: any) => T): JQueryPromise<T>;
        httpCall<T>(httpMethod: string, url: string, jsonRequest: any, parseJsonResponse: (json: any) => T): JQueryPromise<T>;
    }

    class JQueryClient implements IHttpClient {
        constructor() { }

        httpGet<T>(url: string, queryParams: {[key: string]: string}, parseJsonResponse: (json: any) => T) {
            return jQuery.ajax({
                url: url,
                type: "get",
                data: queryParams
            }).then(x => parseJsonResponse(x));
        }

        httpCall<T>(httpMethod: string, url: string, jsonRequest: any, parseJsonResponse: (json: any) => T) {
            let ajaxParams: JQueryAjaxSettings = { "url": url, "type": httpMethod };
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
            switch (m) {
                case "Get": return "get";
                case "Delete": return "delete";
                case "Post": return "post";
                case "Put": return "put";
                default: throw new Error("match failed");
            }
        }

        function verb(conv: S.CallingConvention): S.HttpMethod {
            switch (conv.tag) {
                case "HttpCallingConvention": return conv.Item1;
                default: throw new Error("match failed");
            }
        }

        function localPath(conv: S.CallingConvention): string {
            switch (conv.tag) {
                case "HttpCallingConvention": return conv.path;
                default: throw new Error("match failed");
            }
        }

        function buildUrl(cli: Client, m: S.Method) {
            return cli.prefix + "/" + localPath(m.CallingConvention);
        }

        function buildQueryParams(cli: Client, proxy: RemoteMethodProxy, args: any[]): {[key: string]: string} {
            const query: {[key: string]: string} = {};
            proxy.innerMethod.MethodParameters.forEach((p, i) => {
                query[p.ParameterName] = JSON.stringify(proxy.parameterSerializers[i].toJSON(args[i]));
            });
            return query;
        }

        function buildJsonRequest(cli: Client, proxy: RemoteMethodProxy, args: any[]) {
            let data: any;
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
                    const out = proxy.returnTypeSerializer.fromJSON(resp);
                    return out;
                } else {
                    return resp;
                }
            }
            const url = buildUrl(cli, proxy.innerMethod);
            const httpMethod = verb(proxy.innerMethod.CallingConvention);
            switch (httpMethod) {
                case "Get":
                    const queryParams = buildQueryParams(cli, proxy, args);
                    return cli.httpClient.httpGet(url, queryParams, parseJsonResponse);
                default:
                    const jsonRequest = buildJsonRequest(cli, proxy, args);
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
                case "ArrayType": return { tag: "ArrayType", Item: dataType(at(json, 0)) }
                case "BooleanType": return { tag: "BooleanType" };
                case "BytesType": return { tag: "BytesType" };
                case "DateTimeType": return { tag: "DateTimeType" };
                case "DoubleType": return { tag: "DoubleType" };
                case "IntType": return { tag: "IntType" };
                case "JsonType": return { tag: "JsonType" };
                case "ListType": return { tag: "ListType", Item: dataType(at(json, 0)) };
                case "OptionType": return { tag: "OptionType", Item: dataType(at(json, 0)) };
                case "SequenceType": return { tag: "SequenceType", Item: dataType(at(json, 0)) };
                case "StringDictType": return { tag: "StringDictType", Item: dataType(at(json, 0)) };
                case "StringType": return { tag: "StringType" };
                case "TupleType": return { tag: "TupleType", Item: at(json, 0).map(dataType) };
                case "TypeReference": return { tag: "TypeReference", Item: at(json, 0) };
                default: throw new Error("failed to parse a data type");
            }
        }

        function field(json: any): S.Field {
            return { FieldName: json.FieldName, FieldType: dataType(json.FieldType) };
        }

        function record(json: any): S.Record {
            return { RecordName: json.RecordName, RecordFields: json.RecordFields.map(field) };
        }

        function unionCase(json: any): S.UnionCase {
            return { CaseName: json.CaseName, CaseFields: json.CaseFields.map(field) };
        }

        function union(json: any): S.Union {
            return { UnionName: json.UnionName, UnionCases: json.UnionCases.map(unionCase) };
        }

        function enumCase(json: any): S.EnumCase {
            return { EnumCaseName: json.EnumCaseName, EnumCaseValue: json.EnumCaseValue };
        }

        function parseEnum(json: any): S.Enum {
            return { EnumName: json.EnumName, EnumCases: json.EnumCases.map(enumCase) };
        }

        export function parseTypeDefinition(json: any): S.TypeDefinition {
            switch (tag(json)) {
                case "DefineRecord": return { tag: "DefineRecord", Item: record(at(json, 0)) };
                case "DefineUnion": return { tag: "DefineUnion", Item: union(at(json, 0)) };
                case "DefineEnum": return { tag: "DefineEnum", Item: parseEnum(at(json, 0)) };
                default: throw new Error("error parsing type definition");
            }
        }

        function parameter(json: any): S.Parameter {
            return { ParameterName: json.ParameterName, ParameterType: dataType(json.ParameterType) };
        }

        function httpMethod(json: any): S.HttpMethod {
            const httpMethod = tag(json) as S.HttpMethod;
            switch (httpMethod) {
                case "Delete": return httpMethod;
                case "Get": return httpMethod;
                case "Post": return httpMethod;
                case "Put": return httpMethod;
                default: throw new Error("error parsing http method");
            }
        }

        function callingConvention(json: any): S.CallingConvention {
            switch (tag(json)) {
                case "HttpCallingConvention":
                    return {
                        tag: "HttpCallingConvention",
                        Item1: httpMethod(at(json, 0)),
                        path: at(json, 1)
                    };
                default:
                    throw new Error("error parsing calling convention");
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
            return { CallingConvention: cc, MethodName: methodName, MethodParameters: methodParameters, MethodReturnType: methodReturnType };
        }

        export function parseServiceSchema(json: any): S.Service {
            return { Methods: json.Methods.map(method), TypeDefinitions: json.TypeDefinitions.map(parseTypeDefinition) };
        }
    }

    /** Infrastructure methods called by generated code. */
    export module Internals {

        // TODO: remove this global state.
        var serializerService = new SerializerService();
        var methodBuilder = new MethodBuilder(serializerService);

        export function toJSON(typeRef: string, value: any): any {
            return serializerService.getSerializer({ tag: "TypeReference", Item: typeRef }).toJSON(value);
        }

        export function fromJSON(typeRef: string, json: any): any {
            return serializerService.getSerializer({ tag: "TypeReference", Item: typeRef }).fromJSON(json);
        }

        export function registerActivators(raw: {[key: string]: Function}) {
            var activators: IActivator[] = [];
            function addActivator(typeId: string, func: Function) {
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
Gluon.Internals.registerActivators({
  "Gluon.Schema.Delete": () => <Gluon.Schema.HttpMethod>"Delete",
  "Gluon.Schema.Get": () => <Gluon.Schema.HttpMethod>"Get",
  "Gluon.Schema.Post": () => <Gluon.Schema.HttpMethod>"Post",
  "Gluon.Schema.Put": () => <Gluon.Schema.HttpMethod>"Put",
  "Gluon.Schema.HttpCallingConvention": (httpMethod: Gluon.Schema.HttpMethod, path: string) => <Gluon.Schema.HttpCallingConvention>{ tag: "HttpCallingConvention", Item1: httpMethod, path: path },
  "Gluon.Schema.ArrayType": (a: Gluon.Schema.DataType) => <Gluon.Schema.ArrayType>{ tag: "ArrayType", Item: a },
  "Gluon.Schema.BooleanType": () => <Gluon.Schema.BooleanType>{ tag: "BooleanType" },
  "Gluon.Schema.BytesType": () => <Gluon.Schema.BytesType>{ tag: "BytesType" },
  "Gluon.Schema.DateTimeType": () => <Gluon.Schema.DateTimeType>{ tag: "DateTimeType" },
  "Gluon.Schema.DoubleType": () => <Gluon.Schema.DoubleType>{ tag: "DoubleType" },
  "Gluon.Schema.IntType": () => <Gluon.Schema.IntType>{ tag: "IntType" },
  "Gluon.Schema.JsonType": () => <Gluon.Schema.JsonType>{ tag: "JsonType" },
  "Gluon.Schema.ListType": (a: Gluon.Schema.DataType) => <Gluon.Schema.ListType>{ tag: "ListType", Item: a },
  "Gluon.Schema.OptionType": (a: Gluon.Schema.DataType) => <Gluon.Schema.OptionType>{ tag: "OptionType", Item: a },
  "Gluon.Schema.SequenceType": (a: Gluon.Schema.DataType) => <Gluon.Schema.SequenceType>{ tag: "SequenceType", Item: a },
  "Gluon.Schema.StringDictType": (a: Gluon.Schema.DataType) => <Gluon.Schema.StringDictType>{ tag: "StringDictType", Item: a },
  "Gluon.Schema.StringType": () => <Gluon.Schema.StringType>{ tag: "StringType" },
  "Gluon.Schema.TupleType": (a: Gluon.Schema.DataType[]) => <Gluon.Schema.TupleType>{ tag: "TupleType", Item: a },
  "Gluon.Schema.TypeReference": (a: string) => <Gluon.Schema.TypeReference>{ tag: "TypeReference", Item: a },
  "Gluon.Schema.Parameter": (a: string, b: Gluon.Schema.DataType) => <Gluon.Schema.Parameter>{ ParameterName: a, ParameterType: b },
  "Gluon.Schema.Method": (a: Gluon.Schema.HttpCallingConvention, b: string, c: Gluon.Schema.Parameter[], d: Gluon.Option<Gluon.Schema.DataType>) => <Gluon.Schema.Method>{ CallingConvention: a, MethodName: b, MethodParameters: c, MethodReturnType: d },
  "Gluon.Schema.EnumCase": (a: string, b: number) => <Gluon.Schema.EnumCase>{ EnumCaseName: a, EnumCaseValue: b },
  "Gluon.Schema.Enum": (a: string, b: Gluon.Schema.EnumCase[]) => <Gluon.Schema.Enum>{ EnumName: a, EnumCases: b },
  "Gluon.Schema.Field": (a: string, b: Gluon.Schema.DataType) => <Gluon.Schema.Field>{ FieldName: a, FieldType: b },
  "Gluon.Schema.Record": (a: string, b: Gluon.Schema.Field[]) => <Gluon.Schema.Record>{ RecordName: a, RecordFields: b },
  "Gluon.Schema.UnionCase": (a: string, b: Gluon.Schema.Field[]) => <Gluon.Schema.UnionCase>{ CaseName: a, CaseFields: b },
  "Gluon.Schema.Union": (a: string, b: Gluon.Schema.UnionCase[]) => <Gluon.Schema.Union>{ UnionName: a, UnionCases: b },
  "Gluon.Schema.DefineEnum": (a: Gluon.Schema.Enum) => <Gluon.Schema.DefineEnum>{ tag: "DefineEnum", Item: a },
  "Gluon.Schema.DefineRecord": (a: Gluon.Schema.Record) => <Gluon.Schema.DefineRecord>{ tag: "DefineRecord", Item: a},
  "Gluon.Schema.DefineUnion": (a: Gluon.Schema.Union) => <Gluon.Schema.DefineUnion>{ tag: "DefineUnion", Item: a },
  "Gluon.Schema.Service": (a: Gluon.Schema.Method[], b: Gluon.Schema.TypeDefinition[]) => <Gluon.Schema.Service>{ Methods: a, TypeDefinitions: b }
});
Gluon.Internals.registerTypeDefinitions([["DefineUnion",{"UnionName":"Gluon.Schema.HttpMethod","UnionCases":[{"CaseName":"Delete","CaseFields":[]},{"CaseName":"Get","CaseFields":[]},{"CaseName":"Post","CaseFields":[]},{"CaseName":"Put","CaseFields":[]}]}],["DefineUnion",{"UnionName":"Gluon.Schema.CallingConvention","UnionCases":[{"CaseName":"HttpCallingConvention","CaseFields":[{"FieldName":"Item1","FieldType":["TypeReference","Gluon.Schema.HttpMethod"]},{"FieldName":"path","FieldType":["StringType"]}]}]}],["DefineUnion",{"UnionName":"Gluon.Schema.DataType","UnionCases":[{"CaseName":"ArrayType","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.DataType"]}]},{"CaseName":"BooleanType","CaseFields":[]},{"CaseName":"BytesType","CaseFields":[]},{"CaseName":"DateTimeType","CaseFields":[]},{"CaseName":"DoubleType","CaseFields":[]},{"CaseName":"IntType","CaseFields":[]},{"CaseName":"JsonType","CaseFields":[]},{"CaseName":"ListType","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.DataType"]}]},{"CaseName":"OptionType","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.DataType"]}]},{"CaseName":"SequenceType","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.DataType"]}]},{"CaseName":"StringDictType","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.DataType"]}]},{"CaseName":"StringType","CaseFields":[]},{"CaseName":"TupleType","CaseFields":[{"FieldName":"Item","FieldType":["ListType",["TypeReference","Gluon.Schema.DataType"]]}]},{"CaseName":"TypeReference","CaseFields":[{"FieldName":"Item","FieldType":["StringType"]}]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.Parameter","RecordFields":[{"FieldName":"ParameterName","FieldType":["StringType"]},{"FieldName":"ParameterType","FieldType":["TypeReference","Gluon.Schema.DataType"]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.Method","RecordFields":[{"FieldName":"CallingConvention","FieldType":["TypeReference","Gluon.Schema.CallingConvention"]},{"FieldName":"MethodName","FieldType":["StringType"]},{"FieldName":"MethodParameters","FieldType":["ListType",["TypeReference","Gluon.Schema.Parameter"]]},{"FieldName":"MethodReturnType","FieldType":["OptionType",["TypeReference","Gluon.Schema.DataType"]]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.EnumCase","RecordFields":[{"FieldName":"EnumCaseName","FieldType":["StringType"]},{"FieldName":"EnumCaseValue","FieldType":["IntType"]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.Enum","RecordFields":[{"FieldName":"EnumName","FieldType":["StringType"]},{"FieldName":"EnumCases","FieldType":["ListType",["TypeReference","Gluon.Schema.EnumCase"]]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.Field","RecordFields":[{"FieldName":"FieldName","FieldType":["StringType"]},{"FieldName":"FieldType","FieldType":["TypeReference","Gluon.Schema.DataType"]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.Record","RecordFields":[{"FieldName":"RecordName","FieldType":["StringType"]},{"FieldName":"RecordFields","FieldType":["ListType",["TypeReference","Gluon.Schema.Field"]]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.UnionCase","RecordFields":[{"FieldName":"CaseName","FieldType":["StringType"]},{"FieldName":"CaseFields","FieldType":["ListType",["TypeReference","Gluon.Schema.Field"]]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.Union","RecordFields":[{"FieldName":"UnionName","FieldType":["StringType"]},{"FieldName":"UnionCases","FieldType":["ListType",["TypeReference","Gluon.Schema.UnionCase"]]}]}],["DefineUnion",{"UnionName":"Gluon.Schema.TypeDefinition","UnionCases":[{"CaseName":"DefineEnum","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.Enum"]}]},{"CaseName":"DefineRecord","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.Record"]}]},{"CaseName":"DefineUnion","CaseFields":[{"FieldName":"Item","FieldType":["TypeReference","Gluon.Schema.Union"]}]}]}],["DefineRecord",{"RecordName":"Gluon.Schema.Service","RecordFields":[{"FieldName":"Methods","FieldType":["ListType",["TypeReference","Gluon.Schema.Method"]]},{"FieldName":"TypeDefinitions","FieldType":["ListType",["TypeReference","Gluon.Schema.TypeDefinition"]]}]}]]);
// </BOOTSTRAP-INIT>
