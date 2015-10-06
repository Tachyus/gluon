/// <reference path="../Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="../../../src/Gluon.Client/Gluon.ts" />
/// <reference path="../Gen/Gluon.Generated.ts" />

module WebApp {

    import P = Gluon;
    import S = SampleApp.Services;

    var p1 = new S.Person(new Date(), new S.Phone(12345), "Anton", 30);
    var p2 = new S.Person(new Date(), new S.Address("San Mateo"), "Lida", 1);

    var j1 = JSON.stringify(p1);
    var j2 = JSON.stringify(p2);

    function parse(kind, json: string) {
        return kind.fromJSON(JSON.parse(json));
    }

    var p1x = parse(S.Person, j1);
    var p2x = parse(S.Person, j2);

    console.log(p1x);
    console.log(p2x);

    var dataSeries = new S.DataSeries([]);

    for (var i = 0; i < 10; i++) {
        dataSeries.DataPoints.push(new S.DataPoint(new Date(), i, i / 2, Math.sqrt(i)));
    }

    var dataJson = JSON.stringify(dataSeries);
    console.log(dataJson);

    var dataSeries1 = parse(S.DataSeries, dataJson);
    console.log(dataSeries1);

    var cli = new P.Client();

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
        console.log("putDataSeriesTurnaround => ", result.DataPoints, result.DataPoints.length);
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

    var dictExample = new Gluon.Dict<number>();
    dictExample.setAt("one", 1);
    dictExample.setAt("two", 2);
    dictExample.setAt("three", 13);
    S.convertDict(cli)(dictExample).then(x => {
        x.forEach((key, value) => {
            console.log("dict:", key, value);
        });
    });

    S.convertRawJson(cli)({ foo: 1 }).then(x => {
        console.log("raw-json:", x);
    });

    S.ping(cli)().then(x => console.log("ping:", x), x => console.log("ping failed:", x));

    S.pingSync(cli)().then(x => console.log("ping (sync):", x), x => console.log("ping (sync) failed:", x));

    S.addWithContext(cli)(1).then(x => console.log("add with context ok"));

    var d0 = new Gluon.Dict<[string, string]>();

    d0.setAt("one", ["zero", "one"]);
    d0.setAt("two", ["one", "two"]);

    console.log("built", d0);
    console.log("JSON", JSON.stringify(d0));

    /// Type-safe matching example for destructuring DUs.
    function showContact(contact: S.Contact): string {
        return S.Contact.match(contact, {
            Address: ((text) => "address: " + text),
            Phone: ((number) => "phone: " + String(number))
        });
    }

    console.log(showContact(p1.contact));
    console.log(showContact(p2.contact));

    S.dictCheck(cli)(d0).then(x => console.log("dict check returned ok", x));

    S.getTwoDates(cli)().then(pair => {
        console.log("getTwoDates => ", pair[0], pair[1])
        S.getTwoDatesBack(cli)(pair[0], pair[1]).then(result => console.log("getTwoDatesBack => ", result));
    });

    S.enumTurnaround(cli)([S.E.E2, S.E.E4, S.E.E8]).then(results => {
        console.log("enumTurnaround =>", results);
    });

    S.tupleTurnaround(cli)([[1, "a"], [2, "b"]]).then(results => {
        console.log("tupleTurnaround =>", results);
    });
}

