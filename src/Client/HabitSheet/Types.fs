module Types

type Habit = { Name: string; Description: string; DaysChecked: Map<int, bool> }

type HabitSheet = Habit list
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