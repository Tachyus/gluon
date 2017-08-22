module Services.Library

open System

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
