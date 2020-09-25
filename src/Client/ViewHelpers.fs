module ViewHelpers

open Fulma
open Fable.React
open Fable.React.Props
open HabitTracker.Domain.Habits
open Fulma.Extensions.Wikiki

let button txt color onClick =
    Button.button
        [ 
          Button.Color color
          Button.OnClick onClick ]
        [ str txt ]

let habitModal isActive closeDisplay habit =
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