# Datatypes

Gluon allows correct and consistent automatic serialization and
deserialization for a specific universe of datatypes. For every type
in this universe, Gluon defines an F# representation, a TypeScript
representation, a serialized JSON representation, and automatic
converters between in-memory and serialized representations. The
grammar for the type set is given below:

```fsharp
type Type =
    | Boolean
    | String
    | Bytes
    | Json
    | Int
    | Double
    | DateTime
    | Record of list<Field * Type>
    | Union of list<Case * list<Field * Type>>
    | Tuple of list<Type>
    | Option of Type
    | Array of Type
    | Sequence of Type
    | List of Type   
    | StringDict of Type
```

JSON (or other serialized format) representations should be considered
Gluon internal implementation detail and may change without notice.
Gluon will never need to discriminate on values to infer the type of
the represented objects, since type information is always available
statically. This allows distinct types use similar JSON
representations (such as a string) without confusion.

Boolean, String, Int, Double have intuitive representations. Gluon
should preserve the complete range of values (including NaN, Infinity,
null strings).

## DateTime

Since version 0.4.2, Gluon supports two semantics for DateTime values:
the "physical" interpretation of DateTime is a time-point in universal
time coordinates, and the "calendar" interpretation:

    x = y <=> ymdhms(x) = ymdhmx(y)

For convenience, both interpretations use the same types (the
distinction is not tracked in the type system). On the F# side the
type is `System.DateTime`, and on the TypeScript side it is `Date`.

## Physical datetimes

On CLR, physical datetimes are represented as `DateTime` object with
`Kind` being either `DateTimeKind.Local` or `DateTimeKind.Utc`. In
TypeScript, these are represented as vanilla `Date` objects. During
interop, zone conversions happen to preserve the universal coordinate
meaning of the datetime. Note that the browser always displays `Date`
objects in user-local zone, which might be different from the server
timezone; server timezone information is lost.

## Calendar datetimes

Calendar semantics are useful when displaying dates as-is, bypassing
timezone conversions. Two dates are considered calendar-equivalent if
they have the same year, month, day, hour, minute and second. Time
zone information is not tracked and must be implied.

On CLR, calendar datetimes are `DateTime` objects with
`DateTimeKind.Unspecified`. In TypeScript, these are represented as a
`Date` object that is extended to have an extra tag `x.unspecified ==
true`. Browsers ignores the tag and treat the `Date` objects as if
they were in browser-local timezone, however the tag is used by Gluon
to make sure these values roundtrip to the server using calendar
semantics.

## Records and unions

Records and unions use the WebSharper serialization convention, where
the internal representation of the type is considered, regardless of
public/internal/private declarations. This is unlike some other
conventions such as JSON.NET or other CLR serializers, that use public
properties or opt-in and opt-out attributes. This limits flexibility
but greatly simplifies constructing equivalent types in TypeScript,
and ensuring turnaround. Records will map to TypeScript classes in an
intuitive manner.

Unions currently generate a TypeScript class for each case and an
untagged TypeScript union type alias for the union itself. This scheme
has runtime efficiency and allows to discriminate values with the
`instanceof` operator. This representation may change in the future,
an alternative here is to provide a match construct that enforces
completeness check in the type system.

Since 0.4.x, Gluon also generates a typed pattern-matching helper for
unions.

## Tuples

Tuples will map to JavaScript records and anonymous object types in
TypeScript.

## Collections

Array, sequence and list types will all map to TypeScript arrays.

## Options

Options will map to T | null | undefined. This provides type-safety
when `strictNullChecks` is enabled in `tsconfig.json`. This also
provides nicer interop with existing JavaScript code. An `Option`
namespace provides helper functions to test for `isSome`, `isNone`,
or provide default values.

## Dictionaries
StringDict will use a newly introduced Dict<T> TypeScript type
logically equivalent to F# Dictionary<string,T>, implemented
efficiently using object keys in JavaScript.

## Unstructured data

Finally, there are some rare situations where absolute type safety is
not feasible or counter-productive. In these cases it is nice to have
an escape hatch that allows to communicate data without specifying its
structure entirely. String is one such escape hatch; Gluon provides
two more: Bytes type to transfer byte sequences, represented as packed
typed arrays in TypeScript, and a black-box JSON type that allows
embedding any value in the JSON grammar http://www.json.org/ -
represented as a corresponding JavaScript object, and typed "any" in
TypeScript.
