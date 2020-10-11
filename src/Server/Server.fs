module Server

open System.IO
open System
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open DataStorage
open HabitTracker.Domain.Types

let tryGetEnv key = 
    match Environment.GetEnvironmentVariable key with
    | x when String.IsNullOrWhiteSpace x -> None 
    | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let dataStorage = Repository()

let webApp = router {
    get "/api/init" (fun next ctx ->
        task {
            let isNull' = DataStorage.isNull
            let habitSheetsDAO = dataStorage.GetFirstHabitSheetsMap()
            if isNull' habitSheetsDAO then 
                return! json HabitSheetState.InitialState.HabitSheets next ctx
            else
                return! json habitSheetsDAO.HabitSheets next ctx
        }
    )

    post "/api/save" (fun next ctx ->
        task {
            let! habitSheets = ctx.BindModelAsync<Map<Month, HabitSheet option>>()
            let habitSheetsDAO = { Id = 1; HabitSheets = habitSheets }
            dataStorage.Create habitSheetsDAO
            return! json habitSheetsDAO next ctx
        }
    )
}

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    use_gzip
}

run app