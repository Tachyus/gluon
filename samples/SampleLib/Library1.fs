module Services.Library

open System

type Text = { Value : string }

type SomeContent =
    | T of Text
    | B of bool
    | I of int

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
