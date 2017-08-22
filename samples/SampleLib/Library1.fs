module Services.Library

open System

type Text = { Value : string }

type DataPoint =
    {
        time : DateTime
        x: float
        y: float
        z: float
    }

type DataSeries =
    {
        DataPoints : seq<DataPoint>
    }
