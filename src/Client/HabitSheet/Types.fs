module Types
open Fable.React

type Habit = 
    { Name: string
      Description: string 
      DaysChecked: Map<int, bool> }

type HabitSheet = Habit list
type HabitModal = ReactElement
type HabitSheetState = { HabitSheet: HabitSheet option; HighlightedMonth: string; ActiveHabitName: string option }

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