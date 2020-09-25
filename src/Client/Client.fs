module Client

open System
open Elmish
open Elmish.React
open Fable.React
open Fable.Core.JsInterop
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Thoth.Json
open HabitTracker.Domain.Habits
open ViewHelpers
open Fable.Core

type HabitSheetModel = { HabitSheet: HabitSheet option; HighlightedMonth: string; HabitModalIsActive: bool }

type Msg =
    | HabitAdded of Habit
    | HabitDeleted of string
    | HabitDayChecked of Habit
    | SwitchHabitSheet of string
    | ResetHabitSheet
    | SheetLoaded of HabitSheet
    | ToggleHabitModal

type IJsNative =
    abstract triggerPrompt: ?message: string -> string
    abstract triggerAlert:  ?message: string -> unit
    abstract triggerConfirm: ?message: string -> bool

[<ImportAll("C:/Users/Leon/source/repos/HabitTracker/fableInterop.js")>]
let jsNative : IJsNative = jsNative

let initialHabitSheet () = Fetch.fetchAs<unit, HabitSheet> "/api/init"

let initialDaysCheckedMap =
    seq { 1 .. 31 }
    |> Seq.fold (fun (mapState : Map<int, bool>) i -> mapState.Add(i, false)) Map.empty

let emptyHabitSheet = {
    HabitSheet = Some {
        Habits = []
    }
    HighlightedMonth = "January"
    HabitModalIsActive = false
}

let init () : HabitSheetModel * Cmd<Msg> =
    let loadHabitSheetCmd =
        Cmd.OfPromise.perform initialHabitSheet () SheetLoaded
    emptyHabitSheet, loadHabitSheetCmd

let update msg currentHabitSheetModel : HabitSheetModel * Cmd<Msg> =
    match currentHabitSheetModel.HabitSheet, msg with
    | Some currentHabitSheet, HabitAdded newHabit ->
        let containsHabit = currentHabitSheet.Habits |> List.contains newHabit
        let isEmpty = newHabit.Name |> String.IsNullOrWhiteSpace
        
        if containsHabit || isEmpty then
            jsNative.triggerAlert("Invalid habit name. Check if the habit already exists or if you entered an empty input.")
            currentHabitSheetModel, Cmd.none
        else
            let newHabitSheet = { currentHabitSheet with Habits = [newHabit] |> List.append currentHabitSheet.Habits }
            let newHabitSheetModel = { currentHabitSheetModel with HabitSheet = Some newHabitSheet }
            newHabitSheetModel, Cmd.none

    | Some currentHabitSheet, HabitDeleted habitName ->
        let validHabitName = habitName |> String.IsNullOrWhiteSpace |> not
        if validHabitName then
            let habit = 
                currentHabitSheet.Habits 
                |> List.find (fun habit -> habit.Name = habitName)
            let newHabitList =
                (seq { habit }, currentHabitSheet.Habits)
                ||> List.except 
            let newHabitSheet = { currentHabitSheet with Habits = newHabitList }
            let newHabitSheetModel = { currentHabitSheetModel with HabitSheet = Some newHabitSheet}
            newHabitSheetModel, Cmd.none
        else
            jsNative.triggerAlert("Invalid habit name. Check if the habit already exists or if you entered an empty input.")
            currentHabitSheetModel, Cmd.none

    | Some currentHabitSheet, HabitDayChecked habitToEdit ->
        let updatedHabitList = 
            currentHabitSheet.Habits
            |> List.map (fun currentHabit ->
                   if currentHabit.Name = habitToEdit.Name then
                       { currentHabit with DaysChecked = habitToEdit.DaysChecked }
                   else
                       currentHabit
                )
        let newHabitSheet = { currentHabitSheet with Habits = updatedHabitList }
        let newHabitSheetModel = { currentHabitSheetModel with HabitSheet = Some newHabitSheet }
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

let view (habitSheetModel : HabitSheetModel) (dispatch : Msg -> unit) =
    let habitSheet = habitSheetModel.HabitSheet.Value

    body [] [
        Container.container [ Container.IsFluid ]
            [
                Content.content [] [
                    Field.div [ Field.IsGrouped ] [
                        Level.level [] [
                            Level.left [] [
                                Level.item [] [
                                    Heading.h1 [] [
                                        str "Monthly Habit Tracker"
                                    ]
                                ]
                                Level.item [] [
                                    button "Clear habit sheet" IsPrimary (fun _ ->
                                                                    let shouldClear = jsNative.triggerConfirm("Are you sure you wish to clear the habit sheet?")
                                                                    if shouldClear then dispatch ResetHabitSheet)
                                ]
                            ]
                        ]
                    ]
                    
                    Column.column [ Column.Width (Screen.All, Column.Is12)  ] [
                        div [] [
                            let highlightableMonths = ["JAN"; "FEB"; "MAR"; "APR";
                                                       "MAY"; "JUN"; "JUL"; "AUG";
                                                       "SEP"; "OCT"; "NOV"; "DEC" ]
                            Button.list [ Button.List.AreLarge ] [
                                for month in highlightableMonths do
                                    button month IsPrimary (fun _ -> dispatch (SwitchHabitSheet month))
                            ]
                        ]

                        Table.table [ Table.IsHoverable
                                      Table.IsFullWidth 
                                    ]
                            [
                                tbody [] [
                                    tr [] [
                                        th [] [ str "Habit" ]
                                        for i = 1 to 31 do
                                            th [] [ i |> string |> str ]
                                    ]
                                    let toggleHabitModalMsg = (fun _ -> dispatch ToggleHabitModal)
                                    for habit in habitSheet.Habits do
                                        tr [] [
                                            td [] [
                                                habitModal habitSheetModel.HabitModalIsActive toggleHabitModalMsg habit
                                                button habit.Name IsWhite toggleHabitModalMsg
                                            ]
                                            for i = 1 to 31 do
                                                let habitDayCheckMsg = createHabitDayCheckMsg habit (i)
                                                let isChecked = habit.DaysChecked.ContainsKey (i) &&
                                                                habit.DaysChecked.[i]
                                                let checkBoxId = string i |> (+) "checkRadioId"
                                                td [] [
                                                    checkBox isChecked (fun _ -> dispatch habitDayCheckMsg) checkBoxId
                                                ]
                                        ]
                                ]
                            ]
                        button "Add habit" IsSuccess (fun _ -> addHabit dispatch)
                        button "Delete habit" IsWarning (fun _ -> deleteHabit dispatch)
                    ]
                ]
            ]
    ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

importAll "C:/Users/Leon/source/repos/HabitTracker/src/Client/style.scss"

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
