module State

open Types
open Elmish
open Thoth
open Thoth.Fetch
open System
open Fable.Core

let initialHabitSheet () = Fetch.fetchAs<unit, HabitSheet> "/api/init"

let initialDaysCheckedMap =
    seq { 1 .. 31 }
    |> Seq.fold (fun (mapState : Map<int, bool>) i -> mapState.Add(i, false)) Map.empty

let emptyHabitSheet = {
    HabitSheet = Some []
    HighlightedMonth = "January"
    HabitModalIsActive = false
}

[<ImportAll("C:/Users/Leon/source/repos/HabitTracker/fableInterop.js")>]
let jsNative : IJsNative = jsNative

let init () : HabitSheetModel * Cmd<Msg> =
    let loadHabitSheetCmd =
        Cmd.OfPromise.perform initialHabitSheet () SheetLoaded
    emptyHabitSheet, loadHabitSheetCmd

let addHabit dispatch =
    let habitName = jsNative.triggerPrompt "Enter the habit's name to be added"
    let habit = { Name = habitName; Description = ""; DaysChecked = initialDaysCheckedMap; }
    dispatch (HabitAdded habit)

let deleteHabit dispatch =
    let habitName = jsNative.triggerPrompt "Enter the habit's name to be deleted"
    dispatch (HabitDeleted habitName)

let createHabitDayCheckMsg habit index =
    let invertedDayCheck = not habit.DaysChecked.[index]
    
    let newDaysChecked = habit.DaysChecked.Add (index, invertedDayCheck)
    HabitDayChecked { habit with DaysChecked = newDaysChecked }

let update msg currentHabitSheetModel : HabitSheetModel * Cmd<Msg> =
    match currentHabitSheetModel.HabitSheet, msg with
    | Some currentHabitSheet, HabitAdded newHabit ->
        let containsHabit = currentHabitSheet |> List.contains newHabit
        let isEmpty = newHabit.Name |> String.IsNullOrWhiteSpace
        
        if containsHabit || isEmpty then
            jsNative.triggerAlert("Invalid habit name. Check if the habit already exists or if you entered an empty input.")
            currentHabitSheetModel, Cmd.none
        else
            let newHabitSheet = [newHabit] |> List.append currentHabitSheet
            let newHabitSheetModel = { currentHabitSheetModel with HabitSheet = Some newHabitSheet }
            newHabitSheetModel, Cmd.none

    | Some currentHabitSheet, HabitDeleted habitName ->
        let validHabitName = habitName |> String.IsNullOrWhiteSpace |> not
        if validHabitName then
            let habit = 
                currentHabitSheet
                |> List.find (fun habit -> habit.Name = habitName)
            let newHabitSheet =
                (seq { habit }, currentHabitSheet)
                ||> List.except 
            let newHabitSheetModel = { currentHabitSheetModel with HabitSheet = Some newHabitSheet}
            newHabitSheetModel, Cmd.none
        else
            jsNative.triggerAlert("Invalid habit name. Check if the habit already exists or if you entered an empty input.")
            currentHabitSheetModel, Cmd.none

    | Some currentHabitSheet, HabitDayChecked habitToEdit ->
        let updatedHabitList = 
            currentHabitSheet
            |> List.map (fun currentHabit ->
                   if currentHabit.Name = habitToEdit.Name then
                       { currentHabit with DaysChecked = habitToEdit.DaysChecked }
                   else
                       currentHabit
                )
        let newHabitSheetModel = { currentHabitSheetModel with HabitSheet = Some updatedHabitList }
        newHabitSheetModel, Cmd.none

    | _, SwitchHabitSheet newMonth ->
        let newHabitSheetModel = { currentHabitSheetModel with HighlightedMonth = newMonth }
        newHabitSheetModel, Cmd.none

    | _, ToggleHabitModal ->
        let newHabitSheetModel = { currentHabitSheetModel with HabitModalIsActive =
                                   not currentHabitSheetModel.HabitModalIsActive }
        newHabitSheetModel, Cmd.none

    | _, ResetHabitSheet ->
        emptyHabitSheet, Cmd.none

    | Some _, SheetLoaded _ ->
        let newHabitSheetModel = { currentHabitSheetModel with HighlightedMonth = 
                                   emptyHabitSheet.HighlightedMonth }
        newHabitSheetModel, Cmd.none

    | _ -> currentHabitSheetModel, Cmd.none