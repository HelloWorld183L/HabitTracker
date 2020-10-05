module Types
open Fable.React

type Habit = 
    { Name: string
      Description: string 
      DaysChecked: Map<int, bool> }

    static member InitialDaysChecked =
        seq { 1 .. 31 }
        |> Seq.fold (fun (mapState : Map<int, bool>) i -> mapState.Add(i, false)) Map.empty

type HabitSheet = Habit list
type HabitModal = ReactElement
type HabitSheetState = 
    { HabitSheet: HabitSheet option
      HighlightedMonth: string
      ActiveHabitName: string option }
    
    static member InitialState =
        { 
            HabitSheet = Some []
            HighlightedMonth = "January"
            ActiveHabitName = None
        }

type StateChangeMsg =
    | AddHabit of Habit
    | DeleteHabit of string
    | HabitDayChecked of Habit
    | SwitchHabitSheet of string
    | ResetHabitSheet
    | SheetLoaded of HabitSheet
    | ToggleHabitModal of string option

type IJsNative =
    abstract triggerPrompt: ?message: string -> string
    abstract triggerAlert:  ?message: string -> unit
    abstract triggerConfirm: ?message: string -> bool