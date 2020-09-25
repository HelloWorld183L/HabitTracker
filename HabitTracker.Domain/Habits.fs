namespace HabitTracker.Domain

module Habits =
    type Habit = { Name: string; Description: string; DaysChecked: Map<int, bool> }

    type HabitSheet = { Habits: Habit list }
