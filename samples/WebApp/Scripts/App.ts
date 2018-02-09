import * as Gluon from "gluon-client"
import { SampleApp, Services } from "./Generated"

export namespace WebApp {

    import L = Services.Library;
    import S = SampleApp.Services;

    const p1 = new S.Person(new Date(), { tag: "Phone", number: 12345 }, "Anton", 30);
    const p2 = new S.Person(new Date(), { tag: "Address", text: "San Mateo" }, "Lida", 1);

    const j1 = JSON.stringify(p1);
    const j2 = JSON.stringify(p2);

    function parse<T>(kind: { fromJSON(json: any): T }, json: string): T {
        return kind.fromJSON(JSON.parse(json));
    }

    const p1x = parse(S.Person, j1);
    const p2x = parse(S.Person, j2);

    console.log("parse(S.Person, j1) ==> ", p1x);
    console.log("parse(S.Person, j2) ==> ", p2x);

    const dataSeries = new L.DataSeries([]);

    for (let i = 0; i < 10; i++) {
        dataSeries.DataPoints.push(new L.DataPoint(new Date(), i, i / 2, Math.sqrt(i)));
    }

    const dataJson = JSON.stringify(dataSeries);
    console.log("JSON.stringify(dataSeries) ==> ", dataJson);

    const dataSeries1 = parse(L.DataSeries, dataJson);
    console.log("parse(L.DataSeries, dataJson) ==> ", dataSeries1);

    const cli = new Gluon.Client();

    (async function testPersonPhone() {
        const result = await S.showContact(cli)(p1);
        console.log("testPersonPhone ==> ", result);
    })();

    (async function testPersonAddress() {
        const result = await S.showContact(cli)(p2);
        console.log("testPersonAddress ==> ", result);
    })();


    (async function getDataGroup() {
        const result = await S.getDataGroup(cli)();
        console.log("getDataGroup ==> ", result)
        if (Gluon.Option.isSome(result)) {
            const setResult = await S.setDataGroup(cli)(result);
            console.log("setDataGroup ==> ", setResult)
        }
    })();

    S.incr(cli)(1).then(x => {
        console.log("incr(1) ==> ", x);
    });

    S.older(cli)(p2).then(x => {
        console.log("older(lida) ==> ", x);
    });

    S.younger(cli)(p2).then(x => {
        console.log("younger(lida) ==> ", x);
    });

    S.younger2(cli)(p2).then(x => {
        console.log("younger2(lida) ==> ", x);
    });

    S.putDataSeriesTurnaround(cli)(dataSeries).then(result => {
        if (Gluon.Option.isSome(result)) {
            console.log("putDataSeriesTurnaround => ", result.DataPoints, result.DataPoints.length);
        }
    });

    S.getAdded(cli)(10).then(x => {
        console.log("Added(10) = ", x);
    });

    S.getAdded2(cli)(10, 5).then(x => {
        console.log("Added(10, 5) = ", x);
    });

    S.getSimple(cli)().then(x => {
        console.log("Simple => ", x);
    });

    S.getSpliced(cli)(15, " f/o/o ").then(x => {
        console.log("Spliced(15, ' f/o/o ') = ", x);
    });

    S.reverseBytes(cli)(new Uint8Array([1, 2, 3])).then(x => {
        console.log("reverseBytes => ", x);
    });

    const dictExample = new Gluon.Dict<number>();
    dictExample.setAt("one", 1);
    dictExample.setAt("two", 2);
    dictExample.setAt("three", 13);
    S.convertDict(cli)(dictExample).then(x => {
        if (Gluon.Option.isSome(x)) {
            x.forEach((key: string, value: number) => {
                console.log("dict:", key, value);
            });
        }
    });

    S.convertRawJson(cli)({ foo: 1 }).then(x => {
        console.log("raw-json:", x);
    });

    S.ping(cli)().then(x => console.log("ping:", x), x => console.log("ping failed:", x));

    S.pingSync(cli)().then(x => console.log("ping (sync):", x), x => console.log("ping (sync) failed:", x));

    S.addWithContext(cli)(1).then(x => console.log("add with context ok"));

    const d0 = new Gluon.Dict<[string, string]>();

    d0.setAt("one", ["zero", "one"]);
    d0.setAt("two", ["one", "two"]);

    console.log("built", d0);
    console.log("JSON", JSON.stringify(d0));

    /// Type-safe matching example for destructuring DUs.
    function showContact(contact: S.Contact): string {
        switch (contact.tag) {
            case "Address": return `address: ${contact.text}`;
            case "Phone": return `phone: ${contact.number}`;
        }
    }

    console.log(showContact(p1.contact));
    console.log(showContact(p2.contact));

    S.dictCheck(cli)(d0).then(x => console.log("dict check returned ok", x));

    S.getTwoDates(cli)().then(pair => {
        if (Gluon.Option.isSome(pair)) {
            const [x, y] = pair;
            console.log("getTwoDates => ", x, y)
            S.getTwoDatesBack(cli)(x, y).then(result => console.log("getTwoDatesBack => ", result));
        }
    });

    S.enumTurnaround(cli)([S.E.E2, S.E.E4, S.E.E8]).then(results => {
        console.log("enumTurnaround =>", results);
    });

    S.tupleTurnaround(cli)([[1, "a"], [2, "b"]]).then(results => {
        console.log("tupleTurnaround =>", results);
    });
    
    S.unionTurnaround(cli)({ tag: "C1", Item: "A" }).then(results => {
        console.log("unionTurnaround => ", results);
    });

    (async function getColor() {
        const result = await S.getColor(cli)();
        console.log("retrieved the color " + result);
    })();
    
    async function chooseColor(color: S.Color) {
        const result = await S.setColor(cli)(color);
        console.log(result);
    }

    chooseColor("Blue");
    chooseColor("Red");
    chooseColor("Green Orange");

    // Option tests
    const some = Gluon.Option.some(1);
    const none = Gluon.Option.none<number>();
    const someLiteral = 1;
    const noneLiteral = null;

    if (some === someLiteral) {
        console.log("some constructor equals literal some value");
    } else {
        console.error("some constructor does not equal literal some value");
    }

    if (none === noneLiteral) {
        console.log("none constructor equals literal none value");
    } else {
        console.error("none constructor does not equal literal none value");
    }

    console.log("withDefault when using some", Gluon.Option.withDefault(some, 2));
    console.log("withDefault when using none", Gluon.Option.withDefault(none, 2));
    console.log("withDefault with default of null", Gluon.Option.withDefault(none, null));

    (async function someOptionTurnaround() {
        const result = await S.optionTurnaround(cli)(some);
        console.log("optionTurnaround with some", result);
    })();

    (async function noneOptionTurnaround() {
        const result = await S.optionTurnaround(cli)(none);
        console.log("optionTurnaround with none", result);
    })();

}

