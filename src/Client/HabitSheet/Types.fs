module Types
open HabitTracker.Domain.Types

type StateChangeMsg =
    | AddHabit of Habit
    | DeleteHabit of string
    | HabitDayChecked of Habit
    | SwitchHabitSheet of Month
    | ResetHabitSheet
    | ResetHabitSheets
    | ToggleHabitModal of string option
    | SheetLoaded of Map<Month, HabitSheet option>
    | SaveChanges

type IJsNative =
    abstract triggerPrompt: ?message: string -> string
    abstract triggerAlert:  ?message: string -> unit
    abstract triggerConfirm: ?message: string -> bool