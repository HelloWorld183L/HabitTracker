module View

open Fable.React
open Fable.React.Props
open Fulma
open Fulma.Extensions.Wikiki
open Types
open State
open Fable.FontAwesome
open HabitTracker.Domain.Types

let button txt color onClick =
    Button.button
        [ 
          Button.Color color
          Button.OnClick onClick ]
        [ str txt ]

let habitModal dispatch isActiveHabit habit =
    let closeDisplay = (fun _ -> dispatch (ToggleHabitModal None))
    Modal.modal [ Modal.IsActive isActiveHabit ]
      [ Modal.background [ Props [ OnClick closeDisplay ] ] []
        Modal.Card.card []
          [ Modal.Card.head []
              [ Modal.Card.title []
                  [ str "Habit details" ]
                Delete.delete [ Delete.OnClick closeDisplay ] [] ]
            Modal.Card.body []
              [
                  h6 [] [ str "Name" ]
                  str habit.Name
                  h6 [] [ str "Description" ]
                  str habit.Description
                  h6 [] [ str "Days checked" ]
                  habit.DaysChecked.Count |> string |> str
              ]
            Modal.Card.foot []
              [ Button.button [ Button.Color IsSuccess ]
                  [ str "Save changes" ]
                Button.button [ Button.OnClick closeDisplay ]
                  [ str "Cancel" ] ] ] ]

let checkBox isChecked onChange checkBoxId =
    Checkradio.checkbox [
        Checkradio.Id checkBoxId
        Checkradio.OnChange onChange
        if isChecked then Checkradio.Checked true
        else Checkradio.Checked false ] []

let renderNothingWith funcToRun =
    funcToRun
    nothing

let habitSheetDayComponent dispatch habit currentIndex =
    let habitDayCheckMsg = createHabitDayCheckMsg habit currentIndex
    let isChecked = habit.DaysChecked.ContainsKey currentIndex &&
                    habit.DaysChecked.[currentIndex]
    let checkBoxId = string i |> (+) "checkRadioId"
    td [] [
        checkBox isChecked (fun _ -> dispatch habitDayCheckMsg) checkBoxId
    ]

let habitSheetTableContentComponent dispatch habitSheetState habit =
    let isActiveHabit = 
        match habitSheetState.ActiveHabitName with
        | Some name -> habit.Name = name
        | None -> false
    tr [] [
        td [] [
            habitModal dispatch isActiveHabit habit
            let newActiveHabitName = Some habit.Name
            button habit.Name IsWhite (fun _ -> dispatch (ToggleHabitModal newActiveHabitName))
        ]
        
        for i = 1 to 31 do
            habitSheetDayComponent dispatch habit i
    ]

let habitSheetMonthsComponent dispatch highlightableMonth =
    div [] [
        Button.list [ Button.List.AreLarge ] [
            for month in HabitSheetState.HighlightableMonths do
                let switchHabitSheetDispatch = (fun _ -> dispatch (SwitchHabitSheet month))
                if month = highlightableMonth then
                    button month IsPrimary switchHabitSheetDispatch
                else
                    button month IsDanger switchHabitSheetDispatch
        ]
    ]

let clearSheetDropdownComponent dispatch =
    Dropdown.dropdown [ Dropdown.IsHoverable ] [
        Dropdown.trigger [] [
            Button.button [] [
                span [] [ str "Reset habit sheet(s)" ]
                Icon.icon [ Icon.Size IsSmall ] [
                    Fa.i [ Fa.Solid.AngleDown ]
                        []
                ]
            ]
        ]
        Dropdown.menu [] [
            Dropdown.content [] [
                let confirmThenDispatch confirmMsg stateMsg =
                    (fun _ ->
                        let shouldClear = jsNative'.triggerConfirm confirmMsg
                        if shouldClear then dispatch stateMsg)
                Dropdown.Item.a [] [
                    button "Reset current sheet" IsPrimary (confirmThenDispatch "Are you sure you wish to reset the current habit sheet?" ResetHabitSheet)
                ]
                Dropdown.Item.a [] [
                    button "Reset all habit sheets" IsPrimary (confirmThenDispatch "Are you sure you wish to reset all the habit sheets?" ResetHabitSheets)
                ]
            ]
        ]
    ]

let headerComponent dispatch =
    Field.div [ Field.IsGrouped ] [
        Level.level [] [
            Level.left [] [
                Level.item [] [
                    Heading.h1 [] [
                        str "Monthly Habit Tracker"
                    ]
                ]
                Level.item [] [
                    clearSheetDropdownComponent dispatch
                ]
            ]
        ]
    ]

let view (habitSheetState : HabitSheetState) (dispatch : StateChangeMsg -> unit) =
    let habitSheet = habitSheetState.CurrentHabitSheet.Value
    
    body [] [
        Container.container [ Container.IsFluid ] [
                Content.content [] [
                    headerComponent dispatch
                    
                    habitSheetMonthsComponent dispatch habitSheetState.HighlightedMonth
                    Table.table [ Table.IsFullWidth ]
                        [
                            tbody [] [
                                tr [] [
                                    th [] [ str "Habit" ]
                                    for i = 1 to 31 do
                                        th [] [ i |> string |> str ]
                                ]
                                
                                for habit in habitSheet do
                                    habitSheetTableContentComponent dispatch habitSheetState habit
                            ]
                        ]
                    button "Add habit" IsSuccess (fun _ -> createAddHabit dispatch)
                    button "Delete habit" IsWarning (fun _ -> createDeleteHabit dispatch)
                    button "Save changes" IsInfo (fun _ -> dispatch SaveChanges)
                ]
            ]
    ]