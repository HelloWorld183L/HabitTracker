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

let initialHabitSheetState = {
    HabitSheet = Some []
    HighlightedMonth = "January"
    ActiveHabitName = None
}

[<ImportAll("E:/Programming shit/HabitTracker/fableInterop.js")>]
let jsNative' : IJsNative = jsNative

let init () : HabitSheetState * Cmd<StateChangeMsg> =
    initialHabitSheetState, Cmd.none

let createAddHabit dispatch =
    let habitName = jsNative'.triggerPrompt "Enter the habit's name to be added"
    let habit = { Name = habitName; Description = ""; DaysChecked = initialDaysCheckedMap; }
    dispatch (AddHabit habit)

let createDeleteHabit dispatch =
    let habitName = jsNative'.triggerPrompt "Enter the habit's name to be deleted"
    dispatch (DeleteHabit habitName)

let createHabitDayCheckMsg habit index =
    let invertedDayCheck = not habit.DaysChecked.[index]
    
    let newDaysChecked = habit.DaysChecked.Add (index, invertedDayCheck)
    HabitDayChecked { habit with DaysChecked = newDaysChecked }

let update msg currentHabitSheetState : HabitSheetState * Cmd<StateChangeMsg> =
    let triggerAlertCmd = Cmd.ofSub (fun _ -> jsNative'.triggerAlert "Invalid habit name. Check if the habit already exists or if you entered an empty input.")
    match msg, currentHabitSheetState.HabitSheet with
    | AddHabit newHabit, Some currentHabitSheet ->
        let containsHabit = currentHabitSheet |> List.contains newHabit
        let isEmpty = newHabit.Name |> String.IsNullOrWhiteSpace
        
        if containsHabit || isEmpty then
            currentHabitSheetState, triggerAlertCmd
        else
            let newHabitSheet = [newHabit] |> List.append currentHabitSheet
            { currentHabitSheetState with HabitSheet = Some newHabitSheet }, Cmd.none

    | DeleteHabit habitName, Some currentHabitSheet ->
        let validHabitName = habitName |> String.IsNullOrWhiteSpace |> not
        if validHabitName then
            let newHabitSheet =
                let habit = 
                    currentHabitSheet
                    |> List.find (fun habit -> habit.Name = habitName)
                    |> Seq.singleton
                List.except habit currentHabitSheet
            { currentHabitSheetState with HabitSheet = Some newHabitSheet }, Cmd.none
        else
            currentHabitSheetState, triggerAlertCmd

    | HabitDayChecked habitToEdit, Some currentHabitSheet ->
        let updatedHabitSheet = 
            currentHabitSheet
            |> List.map (fun currentHabit ->
                   if currentHabit.Name = habitToEdit.Name then
                       { currentHabit with DaysChecked = habitToEdit.DaysChecked }
                   else currentHabit
                )
        { currentHabitSheetState with HabitSheet = Some updatedHabitSheet }, Cmd.none

    | SwitchHabitSheet newMonth, _ ->
        { currentHabitSheetState with HighlightedMonth = newMonth }, Cmd.none

    | ToggleHabitModal newActiveHabitName, _  ->
        { currentHabitSheetState with ActiveHabitName = newActiveHabitName }, Cmd.none

    | ResetHabitSheet, _ ->
        initialHabitSheetState, Cmd.none

    | SheetLoaded _, _ ->
        let newHabitSheetState = { currentHabitSheetState with HighlightedMonth = 
                                   initialHabitSheetState.HighlightedMonth }
        newHabitSheetState, Cmd.none