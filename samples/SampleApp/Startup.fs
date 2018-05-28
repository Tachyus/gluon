namespace SampleApp

open System
open System.Collections.Generic
open global.Owin
open Microsoft.Owin
open Gluon

module private Services =

    module L = Services.Library

    type M = Gluon.Method

    type Marker = class end

    type Content = { Value : L.SomeContent }

    type DataGroup = {
        Name: string
        Effective: DateTimeOffset
        Id: Guid
        }

    [<Remote>]
    let testGeneric () =
        { Content.Value = L.T({L.Text.Value = "test"}) }

    [<Remote>]
    let dictCheck (x: IDictionary<string,string*string>) : IDictionary<string,string*string> =
        dict [for kv in x -> (kv.Key, kv.Value)]

    [<Remote(Verb="GET")>]
    let getSimple () =
        "SIMPLE"

    [<Remote(Verb="GET")>]
    let addWithContext (ctx: Gluon.Context) (input: int) =
        ctx.OwinContext.Response.StatusCode <- 403
        ctx.OwinContext.Response.Headers.Add("X-STUFF", [| "STUFF" |])
        input + 1

    [<Remote(Verb="GET")>]
    let getAdded (x: int) =
        x + 1

    [<Remote(Verb="GET")>]
    let getAdded2 (x: int) (y: int) =
        x + y + 1

    [<Remote(Verb="GET")>]
    let getSpliced (x: int) (y: string) =
        sprintf "OK: %i %s" x y

    [<Remote>]
    let ping () =
        async {
            return ()
        }

    [<Remote>]
    let pingSync () = ()

    [<Remote>]
    let incr x =
        x + 1

    [<Remote>]
    let add x y =
        x + y

    type Contact =
        | Address of text: string
        | Phone of number: int

    type Person =
        {
            birthday : DateTime
            contact : Contact
            name : string
            age : int
        }

    [<Remote>]
    let showContact (p: Person) =
        match p.contact with
        | Address text -> sprintf "address %s" text
        | Phone number -> sprintf "phone number %i" number

    [<Remote>]
    let putDataSeriesTurnaround (x: L.DataSeries) =
        printfn "ok.."
        x

    [<Remote>]
    let older p =
        async {
            return {
                p with age = p.age + 1
            }
        }

    [<Remote>]
    let younger p =
        async {
            return {
                p with age = p.age - 1
            }
        }

    [<Remote>]
    let younger2 p =
        async {
            return {
                p with age = p.age - 2
            }
        }

    [<Remote>]
    let convertDict (input: IDictionary<string,int>) =
        seq {
            for KeyValue (k, v) in input ->
                (sprintf "<%s>" k, v)
        }
        |> dict

    [<Remote>]
    let reverseBytes (bytes: byte[]) =
        Array.rev bytes

    [<Remote>]
    let convertRawJson (raw: Json) =
        Json.FromJsonString("1")

    [<Remote>]
    let getTwoDates () =
        let d1 = DateTime(2001, 02, 03, 04, 05, 06)
        (DateTime.Now, d1)

    [<Remote>]
    let getTwoDatesBack (a: DateTime, b: DateTime) =
        sprintf "(%O %O, %O %O)" a a.Kind b b.Kind

    type E =
        | E2 = 2
        | E4 = 4
        | E8 = 8

    [<Remote>]
    let enumTurnaround (enums: list<E>) =
        [E.E2 ||| E.E4] @ enums

    let service =
        Service.FromAssembly(typeof<Marker>.Assembly)

    [<Remote>]
    let tupleTurnaround (tuples: list<int*string>) =
        [for (a, b) in tuples -> (b, a)]

    type Union =
        | C1 of string
        | C2 of string []
        | C3 of string

    [<Remote>]
    let unionTurnaround x =
        match x with
        | C1 x -> C3 x
        | C3 x -> C1 x
        | C2 xs -> C2 xs

    type Color =
        | Blue
        | Red
        | ``Green Orange``

    [<Remote>]
    let getColor () = ``Green Orange``

    [<Remote>]
    let setColor (color: Color) =
        match color with
        | Blue -> "Set color to blue"
        | Red -> "Set color to red"
        | ``Green Orange`` -> "Set color to green orange"

    [<Remote>]
    let optionTurnaround (value: int option) =
        value

    [<Remote(Verb="GET")>]
    let getDataGroup () = 
        let date = DateTimeOffset(DateTime.Now)
        {
            Name = "Bob"
            Effective = date
            Id = Guid.NewGuid()
            }

    [<Remote(Verb="POST")>]
    let setDataGroup (context:Context) (dataGroup: DataGroup) = 
        let t = dataGroup.Effective
        context.OwinContext.Response.StatusCode <- int Net.HttpStatusCode.Created
        t

type Startup() =
    member __.Configuration(app: IAppBuilder) =
        let service = Service.FromAssembly(Reflection.Assembly.GetExecutingAssembly())
        let options = Gluon.Options.Create(service)
        //let options = Options.Create(service, ?prefix = prefix) // Default prefix is /gluon-api
        app.Map(options.UrlPrefix, fun ctx -> ctx.Use(Owin.middleware options) |> ignore) |> ignore

[<assembly:OwinStartupAttribute(typeof<Startup>)>]
do ()
