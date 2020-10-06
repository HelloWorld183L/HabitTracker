module State

open Types
open Elmish
open Thoth
open Thoth.Fetch
open System
open Fable.Core

let initialHabitSheet () = Fetch.fetchAs<unit, HabitSheet> "/api/init"

[<ImportAll("E:/Programming shit/HabitTracker/fableInterop.js")>]
let jsNative' : IJsNative = jsNative

let init () : HabitSheetState * Cmd<StateChangeMsg> =
    HabitSheetState.InitialState, Cmd.none

let createAddHabit dispatch =
    let habitName = jsNative'.triggerPrompt "Enter the habit's name to be added"
    let habit = { Name = habitName; Description = ""; DaysChecked = Habit.InitialDaysChecked; }
    dispatch (AddHabit habit)

let createDeleteHabit dispatch =
    let habitName = jsNative'.triggerPrompt "Enter the habit's name to be deleted"
    dispatch (DeleteHabit habitName)

let createHabitDayCheckMsg habit index =
    let invertedDayCheck = not habit.DaysChecked.[index]
    
    let newDaysChecked = habit.DaysChecked.Add (index, invertedDayCheck)
    HabitDayChecked { habit with DaysChecked = newDaysChecked }

let update msg habitSheetState : HabitSheetState * Cmd<StateChangeMsg> =
    let triggerAlertCmd = Cmd.ofSub (fun _ -> jsNative'.triggerAlert "Invalid habit name. Check if the habit already exists or if you entered an empty input.")
    match msg, habitSheetState.CurrentHabitSheet with
    | AddHabit newHabit, Some currentHabitSheet ->
        let containsHabit = currentHabitSheet |> List.contains newHabit
        let isEmpty = newHabit.Name |> String.IsNullOrWhiteSpace
        
        if containsHabit || isEmpty then
            habitSheetState, triggerAlertCmd
        else
            let newHabitSheet : HabitSheet = [newHabit] |> List.append currentHabitSheet
            let newHabitSheets =
                (Some newHabitSheet, habitSheetState.HabitSheets)
                ||> Map.add habitSheetState.HighlightedMonth
            { habitSheetState with CurrentHabitSheet = Some newHabitSheet; HabitSheets = newHabitSheets }, Cmd.none

    | DeleteHabit habitName, Some currentHabitSheet ->
        let validHabitName = habitName |> String.IsNullOrWhiteSpace |> not
        if validHabitName then
            let newHabitSheet =
                let habit = 
                    currentHabitSheet
                    |> List.find (fun habit -> habit.Name = habitName)
                    |> Seq.singleton
                List.except habit currentHabitSheet
            let newHabitSheets =
                (Some newHabitSheet, habitSheetState.HabitSheets)
                ||> Map.add habitSheetState.HighlightedMonth
            { habitSheetState with CurrentHabitSheet = Some newHabitSheet; HabitSheets = newHabitSheets }, Cmd.none
        else
            habitSheetState, triggerAlertCmd

    | HabitDayChecked habitToEdit, Some currentHabitSheet ->
        let updatedHabitSheet = 
            currentHabitSheet
            |> List.map (fun currentHabit ->
                   if currentHabit.Name = habitToEdit.Name then
                       { currentHabit with DaysChecked = habitToEdit.DaysChecked }
                   else currentHabit
                )
        let newHabitSheets =
            (Some updatedHabitSheet, habitSheetState.HabitSheets)
            ||> Map.add habitSheetState.HighlightedMonth
        { habitSheetState with CurrentHabitSheet = Some updatedHabitSheet; HabitSheets = newHabitSheets }, Cmd.none

    | SwitchHabitSheet newMonth, _ ->
        let newHabitSheet = 
            habitSheetState.HabitSheets
            |> Map.find newMonth
        { habitSheetState with HighlightedMonth = newMonth; CurrentHabitSheet = newHabitSheet; ActiveHabitName = None }, Cmd.none

    | ToggleHabitModal newActiveHabitName, _  ->
        { habitSheetState with ActiveHabitName = newActiveHabitName }, Cmd.none

    | ResetHabitSheet, _ ->
        let newHabitSheets = 
            habitSheetState.HabitSheets
            |> Map.add habitSheetState.HighlightedMonth (Some [])
        { habitSheetState with CurrentHabitSheet = Some []; ActiveHabitName = None; HabitSheets = newHabitSheets }, Cmd.none
    
    | ResetHabitSheets, _ ->
        HabitSheetState.InitialState, Cmd.none