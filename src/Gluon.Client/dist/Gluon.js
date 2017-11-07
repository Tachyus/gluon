"use strict";
var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
Object.defineProperty(exports, "__esModule", { value: true });
var Schema;
(function (Schema) {
    var Parameter = (function () {
        function Parameter() {
        }
        return Parameter;
    }());
    Schema.Parameter = Parameter;
    var Method = (function () {
        function Method() {
        }
        return Method;
    }());
    Schema.Method = Method;
    var EnumCase = (function () {
        function EnumCase() {
        }
        return EnumCase;
    }());
    Schema.EnumCase = EnumCase;
    var Enum = (function () {
        function Enum() {
        }
        return Enum;
    }());
    Schema.Enum = Enum;
    var Field = (function () {
        function Field() {
        }
        return Field;
    }());
    Schema.Field = Field;
    var Record = (function () {
        function Record() {
        }
        return Record;
    }());
    Schema.Record = Record;
    var UnionCase = (function () {
        function UnionCase() {
        }
        return UnionCase;
    }());
    Schema.UnionCase = UnionCase;
    var Union = (function () {
        function Union() {
        }
        return Union;
    }());
    Schema.Union = Union;
})(Schema = exports.Schema || (exports.Schema = {}));
var Option;
(function (Option) {
    function some(value) {
        return value;
    }
    Option.some = some;
    function isSome(value) {
        return value !== undefined && value !== null;
    }
    Option.isSome = isSome;
    function none() {
        return null;
    }
    Option.none = none;
    function isNone(value) {
        return value === undefined || value === null;
    }
    Option.isNone = isNone;
    function fromJSON(json) {
        return isSome(json) ? (json[0]) : null;
    }
    Option.fromJSON = fromJSON;
    function toJSON(value) {
        return isSome(value) ? [value] : null;
    }
    Option.toJSON = toJSON;
    function withDefault(value, defaultValue) {
        return isSome(value) ? value : defaultValue;
    }
    Option.withDefault = withDefault;
})(Option = exports.Option || (exports.Option = {}));
var Dict = (function () {
    function Dict() {
        this.data = {};
    }
    Dict.prototype.check = function (key) {
        if (typeof key !== "string") {
            throw new Error("Invalid or null key");
        }
    };
    Dict.prototype.containsKey = function (key) {
        this.check(key);
        return this.data.hasOwnProperty(key);
    };
    Dict.prototype.forEach = function (visit) {
        for (var prop in this.data) {
            if (this.data.hasOwnProperty(prop)) {
                visit(prop, this.data[prop]);
            }
        }
    };
    Dict.prototype.copy = function () {
        var result = new Dict();
        this.forEach(function (key, el) { return result.setAt(key, el); });
        return result;
    };
    Dict.prototype.at = function (key) {
        this.check(key);
        if (this.data.hasOwnProperty(key)) {
            return this.data[key];
        }
        else {
            throw new Error("Missing key: " + key);
        }
    };
    Dict.prototype.tryFind = function (key) {
        this.check(key);
        return this.data.hasOwnProperty(key) ? this.data[key] : null;
    };
    Dict.prototype.setAt = function (key, value) {
        this.check(key);
        this.data[key] = value;
    };
    Dict.prototype.toJSON = function () {
        return this.data;
    };
    return Dict;
}());
exports.Dict = Dict;
var DataType;
(function (DataType) {
    function children(d) {
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
    DataType.children = children;
})(DataType || (DataType = {}));
function defaultVisitor() {
    return {
        visitDataType: function (t) { },
        visitRecord: function (r) { },
        visitUnion: function (u) { },
        visitEnum: function (e) { }
    };
}
function visitDataType(dt, visitor) {
    visitor.visitDataType(dt);
    DataType.children(dt).forEach(function (x) { return visitDataType(x, visitor); });
}
function visitTypes(types, visitor) {
    function visitField(f) {
        visitDataType(f.FieldType, visitor);
    }
    function visitRecord(r) {
        visitor.visitRecord(r);
        r.RecordFields.forEach(visitField);
    }
    function visitCase(c) {
        c.CaseFields.forEach(visitField);
    }
    function visitUnion(u) {
        visitor.visitUnion(u);
        u.UnionCases.forEach(visitCase);
    }
    function visitEnum(e) {
        visitor.visitEnum(e);
    }
    function visitTD(td) {
        switch (td.tag) {
            case "DefineUnion": return visitUnion(td.Item);
            case "DefineRecord": return visitRecord(td.Item);
            case "DefineEnum": return visitEnum(td.Item);
            default: throw new Error("match failed");
        }
    }
    types.forEach(visitTD);
}
function visitServiceMethods(methods, visitor) {
    function visitParam(p) {
        visitDataType(p.ParameterType, visitor);
    }
    function visitMethod(m) {
        m.MethodParameters.forEach(visitParam);
        if (m.MethodParameters.length > 1) {
            var t = tupleType(m.MethodParameters.map(function (p) { return p.ParameterType; }));
            visitDataType(t, visitor);
        }
        if (!!m.MethodReturnType) {
            visitDataType(m.MethodReturnType, visitor);
        }
    }
    methods.forEach(visitMethod);
}
function dataTypeKey(dataType) {
    function key(dataType) {
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
            case "TupleType": return [":tup"].concat(dataType.Item.map(function (i) { return key(i); }));
            case "TypeReference": return dataType.Item;
            default: throw new Error("match failed");
        }
    }
    return JSON.stringify(key(dataType));
}
function typeDefName(td) {
    switch (td.tag) {
        case "DefineEnum": return td.Item.EnumName;
        case "DefineRecord": return td.Item.RecordName;
        case "DefineUnion": return td.Item.UnionName;
        default: throw new Error("match failed");
    }
}
function findTypeDefinition(svc, name) {
    return svc.TypeDefinitions.filter(function (x) { return typeDefName(x) === name; })[0];
}
var booleanSerializer = {
    init: function (f) { },
    toJSON: function (x) { return x; },
    fromJSON: function (x) { return x; }
};
function serializeNumber(n) {
    if (isFinite(n)) {
        return n;
    }
    else {
        return String(n);
    }
}
function deserializeNumber(json) {
    return Number(json);
}
var numberSerializer = {
    init: function (f) { },
    toJSON: serializeNumber,
    fromJSON: deserializeNumber
};
var DateFormat = new Intl.DateTimeFormat("en-GB", { weekday: "short", day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit", second: "2-digit", hour12: false });
var dateSerializer = {
    init: function (f) { },
    toJSON: function (date) {
        return date.unspecified ?
            DateFormat.format(date) :
            date.toISOString();
    },
    fromJSON: function (str) {
        var unspecified = str.charAt(str.length - 1).toLowerCase() != "z";
        var d = new Date(str);
        d.unspecified = unspecified;
        return d;
    }
};
var rawJsonSerializer = {
    init: function (f) { },
    toJSON: function (x) { return x; },
    fromJSON: function (x) { return x; }
};
function b64encode(bytes) {
    var s = "";
    for (var i = 0; i < bytes.length; i++) {
        s = s + String.fromCharCode(bytes[i]);
    }
    return btoa(s);
}
function b64decode(b64) {
    var input = atob(b64);
    var r = new Uint8Array(input.length);
    for (var i = 0; i < r.length; i++) {
        r[i] = input.charCodeAt(i);
    }
    return r;
}
var bytesSerializer = {
    init: function (f) { },
    toJSON: function (x) { return b64encode(x); },
    fromJSON: function (x) { return b64decode(x); }
};
var stringSerializer = {
    init: function (f) { },
    toJSON: function (x) { return x; },
    fromJSON: function (x) { return x; }
};
var ArraySerializer = (function () {
    function ArraySerializer(element) {
        this.element = element;
    }
    ArraySerializer.prototype.init = function (factory) {
        this.inner = factory.getSerializer(this.element);
    };
    ArraySerializer.prototype.toJSON = function (value) {
        var _this = this;
        return value.map(function (x) { return _this.inner.toJSON(x); });
    };
    ArraySerializer.prototype.fromJSON = function (json) {
        var _this = this;
        return json.map(function (x) { return _this.inner.fromJSON(x); });
    };
    return ArraySerializer;
}());
var DictSerializer = (function () {
    function DictSerializer(element) {
        this.element = element;
    }
    DictSerializer.prototype.init = function (factory) {
        this.inner = factory.getSerializer(this.element);
    };
    DictSerializer.prototype.toJSON = function (dict) {
        var _this = this;
        var result = {};
        dict.forEach(function (key, value) {
            result[key] = _this.inner.toJSON(value);
        });
        return result;
    };
    DictSerializer.prototype.fromJSON = function (json) {
        var result = new Dict();
        for (var key in json) {
            result.setAt(key, this.inner.fromJSON(json[key]));
        }
        return result;
    };
    return DictSerializer;
}());
var OptionSerializer = (function () {
    function OptionSerializer(element) {
        this.element = element;
    }
    OptionSerializer.prototype.init = function (factory) {
        this.inner = factory.getSerializer(this.element);
    };
    OptionSerializer.prototype.toJSON = function (opt) {
        return opt === null ? null : [this.inner.toJSON(opt)];
    };
    OptionSerializer.prototype.fromJSON = function (json) {
        return json === null ? null : this.inner.fromJSON(json[0]);
    };
    return OptionSerializer;
}());
var TupleSerializer = (function () {
    function TupleSerializer(elements) {
        this.elements = elements;
    }
    TupleSerializer.prototype.length = function () {
        return this.elements.length;
    };
    TupleSerializer.prototype.init = function (factory) {
        this.inner = this.elements.map(function (x) { return factory.getSerializer(x); });
    };
    TupleSerializer.prototype.toJSON = function (tup) {
        var n = this.length();
        var res = new Array(n);
        for (var i = 0; i < n; i++) {
            res[i] = this.inner[i].toJSON(tup[i]);
        }
        return res;
    };
    TupleSerializer.prototype.fromJSON = function (json) {
        var n = this.length();
        var res = new Array(n);
        for (var i = 0; i < n; i++) {
            res[i] = this.inner[i].fromJSON(json[i]);
        }
        return res;
    };
    return TupleSerializer;
}());
function buildDataTypeSerializer(dt) {
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
var TypeRegistry = (function () {
    function TypeRegistry() {
        this.activators = new Dict();
    }
    TypeRegistry.prototype.registerActivators = function (activators) {
        var _this = this;
        activators.forEach(function (a) {
            _this.activators.setAt(a.typeId, a);
        });
    };
    TypeRegistry.prototype.fullCaseName = function (typeId, caseName) {
        var i = typeId.lastIndexOf('.');
        if (i === -1) {
            return caseName;
        }
        else {
            return typeId.substr(0, i) + '.' + caseName;
        }
    };
    TypeRegistry.prototype.createRecord = function (typeId, args) {
        return this.activators.at(typeId).createInstance(args);
    };
    TypeRegistry.prototype.createUnion = function (typeId, caseName, args) {
        return this.activators.at(this.fullCaseName(typeId, caseName)).createInstance(args);
    };
    return TypeRegistry;
}());
var EnumSerializer = (function () {
    function EnumSerializer() {
    }
    EnumSerializer.prototype.init = function (factory) { };
    EnumSerializer.prototype.toJSON = function (value) { return value; };
    EnumSerializer.prototype.fromJSON = function (json) { return json; };
    return EnumSerializer;
}());
var RecordSerializer = (function () {
    function RecordSerializer(record, typeRegistry) {
        this.record = record;
        this.typeRegistry = typeRegistry;
    }
    RecordSerializer.prototype.init = function (factory) {
        this.fields = this.record.RecordFields.map(function (f) {
            return {
                name: f.FieldName,
                ser: factory.getSerializer(f.FieldType)
            };
        });
    };
    RecordSerializer.prototype.toJSON = function (value) {
        var result = {};
        this.fields.forEach(function (fld) {
            result[fld.name] = fld.ser.toJSON(value[fld.name]);
        });
        return result;
    };
    RecordSerializer.prototype.fromJSON = function (json) {
        var len = this.fields.length;
        var args = new Array(len);
        for (var i = 0; i < len; i++) {
            var fld = this.fields[i];
            args[i] = fld.ser.fromJSON(json[fld.name]);
        }
        return this.typeRegistry.createRecord(this.record.RecordName, args);
    };
    return RecordSerializer;
}());
var FieldInfo = (function () {
    function FieldInfo() {
    }
    return FieldInfo;
}());
var CaseInfo = (function () {
    function CaseInfo() {
    }
    return CaseInfo;
}());
var UnionSerializer = (function () {
    function UnionSerializer(union, typeRegistry) {
        this.union = union;
        this.typeRegistry = typeRegistry;
    }
    UnionSerializer.prototype.init = function (factory) {
        this.cases = this.union.UnionCases.map(function (c) {
            return {
                caseName: c.CaseName,
                fields: c.CaseFields.map(function (f) {
                    return {
                        fieldName: f.FieldName,
                        fieldSerializer: factory.getSerializer(f.FieldType)
                    };
                })
            };
        });
    };
    UnionSerializer.prototype.findCase = function (name) {
        for (var i = 0; i < this.cases.length; i++) {
            var c = this.cases[i];
            if (c.caseName === name) {
                return c;
            }
        }
    };
    UnionSerializer.prototype.toJSON = function (value) {
        var isStringLiteralUnion = typeof value === "string";
        var tag = isStringLiteralUnion ? value : value.tag;
        var uCase = this.findCase(tag);
        if (uCase !== undefined) {
            if (isStringLiteralUnion) {
                return [tag];
            }
            else {
                var res = new Array(uCase.fields.length + 1);
                res[0] = tag;
                for (var i = 0; i < uCase.fields.length; i++) {
                    var f = uCase.fields[i];
                    var v = value[f.fieldName];
                    res[i + 1] = f.fieldSerializer.toJSON(v);
                }
                return res;
            }
        }
        return null;
    };
    UnionSerializer.prototype.fromJSON = function (json) {
        var c = this.findCase(json[0]);
        if (c !== undefined) {
            var args = new Array(json.length - 1);
            for (var i = 0; i < args.length; i++) {
                var fld = c.fields[i];
                args[i] = fld.fieldSerializer.fromJSON(json[i + 1]);
            }
            return this.typeRegistry.createUnion(this.union.UnionName, c.caseName, args);
        }
        else {
            return null;
        }
    };
    return UnionSerializer;
}());
function typeReference(typeId) {
    return { tag: "TypeReference", Item: typeId };
}
function tupleType(dataTypes) {
    return { tag: "TupleType", Item: dataTypes };
}
var SerializerService = (function () {
    function SerializerService() {
        this.dict = new Dict();
        this.registry = new TypeRegistry();
    }
    SerializerService.prototype.add = function (dt, ser) {
        var key = dataTypeKey(dt);
        this.dict.setAt(key, ser);
    };
    SerializerService.prototype.getSerializer = function (dt) {
        var key = dataTypeKey(dt);
        return this.dict.at(key);
    };
    SerializerService.prototype.contains = function (dt) {
        var key = dataTypeKey(dt);
        return this.dict.containsKey(key);
    };
    SerializerService.prototype.init = function () {
        var _this = this;
        this.dict.forEach(function (k, ser) {
            ser.init(_this);
        });
    };
    SerializerService.prototype.registerActivators = function (activators) {
        this.registry.registerActivators(activators);
    };
    SerializerService.prototype.createVisitor = function () {
        var _this = this;
        var vis = defaultVisitor();
        var add = function (dt) {
            if (!_this.contains(dt)) {
                _this.add(dt, buildDataTypeSerializer(dt));
            }
        };
        vis.visitDataType = function (dt) {
            if (dt.tag !== "TypeReference") {
                add(dt);
            }
        };
        vis.visitRecord = function (r) {
            var dt = typeReference(r.RecordName);
            _this.add(dt, new RecordSerializer(r, _this.registry));
        };
        vis.visitUnion = function (u) {
            var dt = typeReference(u.UnionName);
            _this.add(dt, new UnionSerializer(u, _this.registry));
        };
        vis.visitEnum = function (e) {
            var dt = typeReference(e.EnumName);
            _this.add(dt, new EnumSerializer());
        };
        return vis;
    };
    SerializerService.prototype.registerTypes = function (types) {
        visitTypes(types, this.createVisitor());
        this.init();
    };
    SerializerService.prototype.registerServiceMethods = function (methods) {
        visitServiceMethods(methods, this.createVisitor());
        this.init();
    };
    return SerializerService;
}());
var Client = (function () {
    function Client(httpClient, prefix) {
        if (httpClient === void 0) { httpClient = new FetchClient(); }
        if (prefix === void 0) { prefix = "/gluon-api"; }
        this.httpClient = httpClient;
        this.prefix = prefix;
    }
    return Client;
}());
exports.Client = Client;
var FetchClient = (function () {
    function FetchClient(headers) {
        if (headers === void 0) { headers = {}; }
        this.headers = headers;
    }
    FetchClient.serialize = function (obj, prefix) {
        var str = [];
        for (var p in obj) {
            if (obj.hasOwnProperty(p)) {
                var k = prefix ? prefix + "[" + p + "]" : p, v = obj[p];
                str.push((v !== null && typeof v === "object") ?
                    this.serialize(v, k) :
                    encodeURIComponent(k) + "=" + encodeURIComponent(v));
            }
        }
        return str.join("&");
    };
    FetchClient.prototype.httpGet = function (url, queryParams, parseJsonResponse) {
        var queryString = Option.isSome(queryParams) ? FetchClient.serialize(queryParams) : null;
        var urlAndQuery = Option.isNone(queryString) || queryString === "" ? url : url + "?" + queryString;
        return window.fetch(urlAndQuery, {
            method: "GET",
            headers: new Headers(__assign({}, this.headers, { "Accept": "application/json" }))
        }).then(function (r) { return r.json(); }).then(parseJsonResponse);
    };
    FetchClient.prototype.httpCall = function (httpMethod, url, jsonRequest, parseJsonResponse) {
        var params = Option.isSome(jsonRequest) ? {
            method: httpMethod,
            body: jsonRequest,
            headers: new Headers(__assign({}, this.headers, { "Accept": "application/json", "Content-Type": "application/json" }))
        } : { method: httpMethod };
        var promise = window.fetch(url, params);
        if (Option.isSome(parseJsonResponse)) {
            return promise.then(function (response) { return response.json(); }).then(parseJsonResponse);
        }
        else {
            return promise;
        }
    };
    return FetchClient;
}());
exports.FetchClient = FetchClient;
var Remoting;
(function (Remoting) {
    function verbName(m) {
        switch (m) {
            case "Get": return "get";
            case "Delete": return "delete";
            case "Post": return "post";
            case "Put": return "put";
            default: throw new Error("match failed");
        }
    }
    function verb(conv) {
        switch (conv.tag) {
            case "HttpCallingConvention": return conv.Item1;
            default: throw new Error("match failed");
        }
    }
    function localPath(conv) {
        switch (conv.tag) {
            case "HttpCallingConvention": return conv.path;
            default: throw new Error("match failed");
        }
    }
    function buildUrl(cli, m) {
        return cli.prefix + "/" + localPath(m.CallingConvention);
    }
    function buildQueryParams(cli, proxy, args) {
        var query = {};
        proxy.innerMethod.MethodParameters.forEach(function (p, i) {
            query[p.ParameterName] = JSON.stringify(proxy.parameterSerializers[i].toJSON(args[i]));
        });
        return query;
    }
    function buildJsonRequest(cli, proxy, args) {
        var data;
        if (proxy.arity == 0) {
            return null;
        }
        else if (proxy.arity == 1) {
            data = args[0];
        }
        else {
            data = args;
        }
        return JSON.stringify(proxy.jointParametersSerializer.toJSON(data));
    }
    function remoteCall(cli, proxy, args) {
        function parseJsonResponse(resp) {
            if (proxy.doesReturn) {
                var out = proxy.returnTypeSerializer.fromJSON(resp);
                return out;
            }
            else {
                return resp;
            }
        }
        var url = buildUrl(cli, proxy.innerMethod);
        var httpMethod = verb(proxy.innerMethod.CallingConvention);
        switch (httpMethod) {
            case "Get":
                var queryParams = buildQueryParams(cli, proxy, args);
                return cli.httpClient.httpGet(url, queryParams, parseJsonResponse);
            default:
                var jsonRequest = buildJsonRequest(cli, proxy, args);
                return cli.httpClient.httpCall(verbName(httpMethod), url, jsonRequest, parseJsonResponse);
        }
    }
    Remoting.remoteCall = remoteCall;
})(Remoting || (Remoting = {}));
var RemoteMethodProxy = (function () {
    function RemoteMethodProxy(factory, m) {
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
                this.parameterSerializers = m.MethodParameters.map(function (p) {
                    return factory.getSerializer(p.ParameterType);
                });
                this.jointParametersSerializer = factory.getSerializer(tupleType(m.MethodParameters.map(function (p) {
                    return p.ParameterType;
                })));
                break;
        }
        if (!!m.MethodReturnType) {
            this.doesReturn = true;
            this.returnTypeSerializer = factory.getSerializer(m.MethodReturnType);
        }
        else {
            this.doesReturn = false;
        }
    }
    RemoteMethodProxy.prototype.call = function (cli, args) {
        return Remoting.remoteCall(cli, this, args);
    };
    return RemoteMethodProxy;
}());
var MethodBuilder = (function () {
    function MethodBuilder(factory) {
        this.factory = factory;
        this.table = new Dict();
    }
    MethodBuilder.prototype.registerService = function (service) {
        var _this = this;
        service.Methods.forEach(function (m) {
            var proxy = new RemoteMethodProxy(_this.factory, m);
            _this.table.setAt(m.MethodName, proxy);
        });
    };
    MethodBuilder.prototype.getProxy = function (name) {
        return this.table.at(name);
    };
    MethodBuilder.prototype.remoteMethod = function (name) {
        var _this = this;
        return function (client) {
            var proxy = _this.getProxy(name);
            function call() {
                var args = arguments;
                return proxy.call(client, args);
            }
            return call;
        };
    };
    return MethodBuilder;
}());
var RawSchemaJsonParser;
(function (RawSchemaJsonParser) {
    function at(json, pos) {
        return json[pos + 1];
    }
    function rawCaseFields(json) {
        var j = json;
        return j.slice(1);
    }
    function tag(json) {
        return json[0];
    }
    function dataType(json) {
        switch (tag(json)) {
            case "ArrayType": return { tag: "ArrayType", Item: dataType(at(json, 0)) };
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
    function field(json) {
        return { FieldName: json.FieldName, FieldType: dataType(json.FieldType) };
    }
    function record(json) {
        return { RecordName: json.RecordName, RecordFields: json.RecordFields.map(field) };
    }
    function unionCase(json) {
        return { CaseName: json.CaseName, CaseFields: json.CaseFields.map(field) };
    }
    function union(json) {
        return { UnionName: json.UnionName, UnionCases: json.UnionCases.map(unionCase) };
    }
    function enumCase(json) {
        return { EnumCaseName: json.EnumCaseName, EnumCaseValue: json.EnumCaseValue };
    }
    function parseEnum(json) {
        return { EnumName: json.EnumName, EnumCases: json.EnumCases.map(enumCase) };
    }
    function parseTypeDefinition(json) {
        switch (tag(json)) {
            case "DefineRecord": return { tag: "DefineRecord", Item: record(at(json, 0)) };
            case "DefineUnion": return { tag: "DefineUnion", Item: union(at(json, 0)) };
            case "DefineEnum": return { tag: "DefineEnum", Item: parseEnum(at(json, 0)) };
            default: throw new Error("error parsing type definition");
        }
    }
    RawSchemaJsonParser.parseTypeDefinition = parseTypeDefinition;
    function parameter(json) {
        return { ParameterName: json.ParameterName, ParameterType: dataType(json.ParameterType) };
    }
    function httpMethod(json) {
        var httpMethod = tag(json);
        switch (httpMethod) {
            case "Delete": return httpMethod;
            case "Get": return httpMethod;
            case "Post": return httpMethod;
            case "Put": return httpMethod;
            default: throw new Error("error parsing http method");
        }
    }
    function callingConvention(json) {
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
    function opt(json, parse) {
        return json === null ? null : parse(json[0]);
    }
    function method(json) {
        var cc = callingConvention(json.CallingConvention);
        var methodName = json.MethodName;
        var methodParameters = json.MethodParameters.map(parameter);
        var methodReturnType = opt(json.MethodReturnType, dataType);
        return { CallingConvention: cc, MethodName: methodName, MethodParameters: methodParameters, MethodReturnType: methodReturnType };
    }
    function parseServiceSchema(json) {
        return { Methods: json.Methods.map(method), TypeDefinitions: json.TypeDefinitions.map(parseTypeDefinition) };
    }
    RawSchemaJsonParser.parseServiceSchema = parseServiceSchema;
})(RawSchemaJsonParser || (RawSchemaJsonParser = {}));
var Internals;
(function (Internals) {
    var serializerService = new SerializerService();
    var methodBuilder = new MethodBuilder(serializerService);
    function toJSON(typeRef, value) {
        return serializerService.getSerializer({ tag: "TypeReference", Item: typeRef }).toJSON(value);
    }
    Internals.toJSON = toJSON;
    function fromJSON(typeRef, json) {
        return serializerService.getSerializer({ tag: "TypeReference", Item: typeRef }).fromJSON(json);
    }
    Internals.fromJSON = fromJSON;
    function registerActivators(raw) {
        var activators = [];
        function addActivator(typeId, func) {
            activators.push({
                typeId: typeId,
                createInstance: function (args) { return func.apply(null, args); }
            });
        }
        for (var key in raw) {
            addActivator(key, raw[key]);
        }
        serializerService.registerActivators(activators);
    }
    Internals.registerActivators = registerActivators;
    function registerTypeDefinitions(rawTypeDefJson) {
        var typeDefs = rawTypeDefJson.map(RawSchemaJsonParser.parseTypeDefinition);
        serializerService.registerTypes(typeDefs);
    }
    Internals.registerTypeDefinitions = registerTypeDefinitions;
    function registerService(rawServiceJson) {
        var service = RawSchemaJsonParser.parseServiceSchema(rawServiceJson);
        serializerService.registerTypes(service.TypeDefinitions);
        serializerService.registerServiceMethods(service.Methods);
        methodBuilder.registerService(service);
    }
    Internals.registerService = registerService;
    function remoteMethod(name) {
        return methodBuilder.remoteMethod(name);
    }
    Internals.remoteMethod = remoteMethod;
})(Internals = exports.Internals || (exports.Internals = {}));
Internals.registerActivators({
    "Gluon.Schema.Delete": function () { return "Delete"; },
    "Gluon.Schema.Get": function () { return "Get"; },
    "Gluon.Schema.Post": function () { return "Post"; },
    "Gluon.Schema.Put": function () { return "Put"; },
    "Gluon.Schema.HttpCallingConvention": function (httpMethod, path) { return ({ tag: "HttpCallingConvention", Item1: httpMethod, path: path }); },
    "Gluon.Schema.ArrayType": function (a) { return ({ tag: "ArrayType", Item: a }); },
    "Gluon.Schema.BooleanType": function () { return ({ tag: "BooleanType" }); },
    "Gluon.Schema.BytesType": function () { return ({ tag: "BytesType" }); },
    "Gluon.Schema.DateTimeType": function () { return ({ tag: "DateTimeType" }); },
    "Gluon.Schema.DoubleType": function () { return ({ tag: "DoubleType" }); },
    "Gluon.Schema.IntType": function () { return ({ tag: "IntType" }); },
    "Gluon.Schema.JsonType": function () { return ({ tag: "JsonType" }); },
    "Gluon.Schema.ListType": function (a) { return ({ tag: "ListType", Item: a }); },
    "Gluon.Schema.OptionType": function (a) { return ({ tag: "OptionType", Item: a }); },
    "Gluon.Schema.SequenceType": function (a) { return ({ tag: "SequenceType", Item: a }); },
    "Gluon.Schema.StringDictType": function (a) { return ({ tag: "StringDictType", Item: a }); },
    "Gluon.Schema.StringType": function () { return ({ tag: "StringType" }); },
    "Gluon.Schema.TupleType": function (a) { return ({ tag: "TupleType", Item: a }); },
    "Gluon.Schema.TypeReference": function (a) { return ({ tag: "TypeReference", Item: a }); },
    "Gluon.Schema.Parameter": function (a, b) { return ({ ParameterName: a, ParameterType: b }); },
    "Gluon.Schema.Method": function (a, b, c, d) { return ({ CallingConvention: a, MethodName: b, MethodParameters: c, MethodReturnType: d }); },
    "Gluon.Schema.EnumCase": function (a, b) { return ({ EnumCaseName: a, EnumCaseValue: b }); },
    "Gluon.Schema.Enum": function (a, b) { return ({ EnumName: a, EnumCases: b }); },
    "Gluon.Schema.Field": function (a, b) { return ({ FieldName: a, FieldType: b }); },
    "Gluon.Schema.Record": function (a, b) { return ({ RecordName: a, RecordFields: b }); },
    "Gluon.Schema.UnionCase": function (a, b) { return ({ CaseName: a, CaseFields: b }); },
    "Gluon.Schema.Union": function (a, b) { return ({ UnionName: a, UnionCases: b }); },
    "Gluon.Schema.DefineEnum": function (a) { return ({ tag: "DefineEnum", Item: a }); },
    "Gluon.Schema.DefineRecord": function (a) { return ({ tag: "DefineRecord", Item: a }); },
    "Gluon.Schema.DefineUnion": function (a) { return ({ tag: "DefineUnion", Item: a }); },
    "Gluon.Schema.Service": function (a, b) { return ({ Methods: a, TypeDefinitions: b }); }
});
Internals.registerTypeDefinitions([["DefineUnion", { "UnionName": "Gluon.Schema.HttpMethod", "UnionCases": [{ "CaseName": "Delete", "CaseFields": [] }, { "CaseName": "Get", "CaseFields": [] }, { "CaseName": "Post", "CaseFields": [] }, { "CaseName": "Put", "CaseFields": [] }] }], ["DefineUnion", { "UnionName": "Gluon.Schema.CallingConvention", "UnionCases": [{ "CaseName": "HttpCallingConvention", "CaseFields": [{ "FieldName": "Item1", "FieldType": ["TypeReference", "Gluon.Schema.HttpMethod"] }, { "FieldName": "path", "FieldType": ["StringType"] }] }] }], ["DefineUnion", { "UnionName": "Gluon.Schema.DataType", "UnionCases": [{ "CaseName": "ArrayType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.DataType"] }] }, { "CaseName": "BooleanType", "CaseFields": [] }, { "CaseName": "BytesType", "CaseFields": [] }, { "CaseName": "DateTimeType", "CaseFields": [] }, { "CaseName": "DoubleType", "CaseFields": [] }, { "CaseName": "IntType", "CaseFields": [] }, { "CaseName": "JsonType", "CaseFields": [] }, { "CaseName": "ListType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.DataType"] }] }, { "CaseName": "OptionType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.DataType"] }] }, { "CaseName": "SequenceType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.DataType"] }] }, { "CaseName": "StringDictType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.DataType"] }] }, { "CaseName": "StringType", "CaseFields": [] }, { "CaseName": "TupleType", "CaseFields": [{ "FieldName": "Item", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.DataType"]] }] }, { "CaseName": "TypeReference", "CaseFields": [{ "FieldName": "Item", "FieldType": ["StringType"] }] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.Parameter", "RecordFields": [{ "FieldName": "ParameterName", "FieldType": ["StringType"] }, { "FieldName": "ParameterType", "FieldType": ["TypeReference", "Gluon.Schema.DataType"] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.Method", "RecordFields": [{ "FieldName": "CallingConvention", "FieldType": ["TypeReference", "Gluon.Schema.CallingConvention"] }, { "FieldName": "MethodName", "FieldType": ["StringType"] }, { "FieldName": "MethodParameters", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.Parameter"]] }, { "FieldName": "MethodReturnType", "FieldType": ["OptionType", ["TypeReference", "Gluon.Schema.DataType"]] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.EnumCase", "RecordFields": [{ "FieldName": "EnumCaseName", "FieldType": ["StringType"] }, { "FieldName": "EnumCaseValue", "FieldType": ["IntType"] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.Enum", "RecordFields": [{ "FieldName": "EnumName", "FieldType": ["StringType"] }, { "FieldName": "EnumCases", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.EnumCase"]] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.Field", "RecordFields": [{ "FieldName": "FieldName", "FieldType": ["StringType"] }, { "FieldName": "FieldType", "FieldType": ["TypeReference", "Gluon.Schema.DataType"] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.Record", "RecordFields": [{ "FieldName": "RecordName", "FieldType": ["StringType"] }, { "FieldName": "RecordFields", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.Field"]] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.UnionCase", "RecordFields": [{ "FieldName": "CaseName", "FieldType": ["StringType"] }, { "FieldName": "CaseFields", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.Field"]] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.Union", "RecordFields": [{ "FieldName": "UnionName", "FieldType": ["StringType"] }, { "FieldName": "UnionCases", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.UnionCase"]] }] }], ["DefineUnion", { "UnionName": "Gluon.Schema.TypeDefinition", "UnionCases": [{ "CaseName": "DefineEnum", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.Enum"] }] }, { "CaseName": "DefineRecord", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.Record"] }] }, { "CaseName": "DefineUnion", "CaseFields": [{ "FieldName": "Item", "FieldType": ["TypeReference", "Gluon.Schema.Union"] }] }] }], ["DefineRecord", { "RecordName": "Gluon.Schema.Service", "RecordFields": [{ "FieldName": "Methods", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.Method"]] }, { "FieldName": "TypeDefinitions", "FieldType": ["ListType", ["TypeReference", "Gluon.Schema.TypeDefinition"]] }] }]]);
//# sourceMappingURL=Gluon.js.map