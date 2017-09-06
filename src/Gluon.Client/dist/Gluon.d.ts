export declare namespace Schema {
    type HttpMethod = "Delete" | "Get" | "Post" | "Put";
    interface HttpCallingConvention {
        tag: "HttpCallingConvention";
        Item1: HttpMethod;
        path: string;
    }
    type CallingConvention = HttpCallingConvention;
    interface ArrayType {
        tag: "ArrayType";
        Item: DataType;
    }
    interface BooleanType {
        tag: "BooleanType";
    }
    interface BytesType {
        tag: "BytesType";
    }
    interface DateTimeType {
        tag: "DateTimeType";
    }
    interface DoubleType {
        tag: "DoubleType";
    }
    interface IntType {
        tag: "IntType";
    }
    interface JsonType {
        tag: "JsonType";
    }
    interface ListType {
        tag: "ListType";
        Item: DataType;
    }
    interface OptionType {
        tag: "OptionType";
        Item: DataType;
    }
    interface SequenceType {
        tag: "SequenceType";
        Item: DataType;
    }
    interface StringDictType {
        tag: "StringDictType";
        Item: DataType;
    }
    interface StringType {
        tag: "StringType";
    }
    interface TupleType {
        tag: "TupleType";
        Item: DataType[];
    }
    interface TypeReference {
        tag: "TypeReference";
        Item: string;
    }
    type DataType = ArrayType | BooleanType | BytesType | DateTimeType | DoubleType | IntType | JsonType | ListType | OptionType | SequenceType | StringDictType | StringType | TupleType | TypeReference;
    class Parameter {
        ParameterName: string;
        ParameterType: DataType;
    }
    class Method {
        CallingConvention: CallingConvention;
        MethodName: string;
        MethodParameters: Parameter[];
        MethodReturnType: Option<DataType>;
    }
    class EnumCase {
        EnumCaseName: string;
        EnumCaseValue: number;
    }
    class Enum {
        EnumName: string;
        EnumCases: EnumCase[];
    }
    class Field {
        FieldName: string;
        FieldType: DataType;
    }
    class Record {
        RecordName: string;
        RecordFields: Field[];
    }
    class UnionCase {
        CaseName: string;
        CaseFields: Field[];
    }
    class Union {
        UnionName: string;
        UnionCases: UnionCase[];
    }
    interface DefineEnum {
        tag: "DefineEnum";
        Item: Enum;
    }
    interface DefineRecord {
        tag: "DefineRecord";
        Item: Record;
    }
    interface DefineUnion {
        tag: "DefineUnion";
        Item: Union;
    }
    type TypeDefinition = DefineEnum | DefineRecord | DefineUnion;
    interface Service {
        Methods: Method[];
        TypeDefinitions: TypeDefinition[];
    }
}
export declare type Option<T> = T | null | undefined;
export declare namespace Option {
    function some<T>(value: T): Option<T>;
    function isSome<T>(value: Option<T>): value is T;
    function none<T>(): Option<T>;
    function isNone<T>(value: Option<T>): value is null | undefined;
    function fromJSON<T>(json: any): Option<T>;
    function toJSON<T>(value: Option<T>): any;
    function withDefault<T>(value: Option<T>, defaultValue: T): T;
}
export declare class Dict<T> {
    private data;
    private check(key);
    containsKey(key: string): boolean;
    forEach(visit: (key: string, element: T) => void): void;
    copy(): Dict<T>;
    at(key: string): T;
    tryFind(key: string): Option<T>;
    setAt(key: string, value: T): void;
    toJSON(): {
        [key: string]: T;
    };
}
export interface IActivator {
    createInstance(args: any[]): any;
    typeId: string;
}
export declare class Client {
    httpClient: IHttpClient;
    prefix: string;
    constructor(httpClient?: IHttpClient, prefix?: string);
}
export interface RemoteMethod<T> {
    (client: Client): T;
}
export interface IHttpClient {
    httpGet<T>(url: string, queryParams: {
        [key: string]: string;
    }, parseJsonResponse: (json: any) => T): Promise<Option<T>>;
    httpCall<T>(httpMethod: string, url: string, jsonRequest?: any, parseJsonResponse?: (json: any) => T): Promise<Option<T> | Response>;
}
export declare class FetchClient implements IHttpClient {
    private headers;
    constructor(headers?: {
        [key: string]: string;
    });
    static serialize(obj: any, prefix?: string): string;
    httpGet<T>(url: string, queryParams: {
        [key: string]: string;
    }, parseJsonResponse: (json: any) => T): Promise<Option<T>>;
    httpCall<T>(httpMethod: string, url: string, jsonRequest: any, parseJsonResponse: (json: any) => T): Promise<Option<T> | Response>;
}
export declare namespace Internals {
    function toJSON(typeRef: string, value: any): any;
    function fromJSON(typeRef: string, json: any): any;
    function registerActivators(raw: {
        [key: string]: Function;
    }): void;
    function registerTypeDefinitions(rawTypeDefJson: any[]): void;
    function registerService(rawServiceJson: any): void;
    function remoteMethod<T>(name: string): RemoteMethod<T>;
}
