module View

open Fable.React
open Fable.React.Props
open Fulma
open Fulma.Extensions.Wikiki
open Types
open State

let button txt color onClick =
    Button.button
        [ 
          Button.Color color
          Button.OnClick onClick ]
        [ str txt ]

let habitModal closeDisplay isActive habit =
    Modal.modal [ Modal.IsActive isActive ]
      [ Modal.background [ Props [ OnClick closeDisplay ] ] [ ]
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
            Modal.Card.foot [ ]
              [ Button.button [ Button.Color IsSuccess ]
                  [ str "Save changes" ]
                Button.button [ ]
                  [ str "Cancel" ] ] ] ]

let checkBox isChecked onChange checkBoxId =
    Checkradio.checkbox [
        Checkradio.Id checkBoxId
        Checkradio.OnChange onChange
        if isChecked then Checkradio.Checked true
        else Checkradio.Checked false ] []

let view (habitSheetModel : HabitSheetState) (dispatch : StateChangeMsg -> unit) =
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
                                                                    let shouldClear = jsNative'.triggerConfirm "Are you sure you wish to clear the habit sheet?"
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
                                    for habit in habitSheet do
                                        tr [] [
                                            td [] [
                                                habitModal toggleHabitModalMsg habitSheetModel.HabitModalIsActive habit
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