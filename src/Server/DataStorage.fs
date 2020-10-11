module DataStorage

open LiteDB
open LiteDB.FSharp
open HabitTracker.Domain.Types
open FSharp.Data

type Configuration = JsonProvider<"App.json">
let configurationSample = Configuration.GetSample()

[<CLIMutable>]
type HabitSheetsDAO = { Id: int; HabitSheets: Map<Month, HabitSheet option> }

let inline isNull (x:^T when ^T: not struct) = obj.ReferenceEquals (x, null)

type Repository () =
    let mapper = FSharpBsonMapper()
    let database =
        new LiteDatabase (configurationSample.ConnectionString, mapper)
    let habitSheetDAOs = database.GetCollection<HabitSheetsDAO>("habitSheetDAOs")

    member _.Create (habitSheetsDAO : HabitSheetsDAO) =
        habitSheetDAOs.Upsert habitSheetsDAO |> ignore

    // Purely for demonstration purposes. Real world would have a different data model to support multiple habit sheets with months
    member _.GetFirstHabitSheetsMap() =
        1
        |> BsonValue
        |> habitSheetDAOs.FindById