﻿module HabitTracker.Domain.Types

type DayNum = int

type Habit = 
    { Name: string
      Description: string 
      DaysChecked: Map<DayNum, bool> }

    static member InitialDaysChecked =
        seq { 1 .. 31 }
        |> Seq.fold (fun (daysCheckedState : Map<DayNum, bool>) day -> 
                        daysCheckedState.Add(day, false)) Map.empty

type HabitSheet = Habit list
type Month = string
type HabitSheetState = 
    { 
      HabitSheets: Map<Month, HabitSheet option>
      CurrentHabitSheet: HabitSheet option
      HighlightedMonth: Month
      ActiveHabitName: string option }

    static member HighlightableMonths =
        ["JAN"; "FEB"; "MAR"; "APR";
         "MAY"; "JUN"; "JUL"; "AUG";
         "SEP"; "OCT"; "NOV"; "DEC" ]

    static member InitialState =
        {
            HabitSheets = HabitSheetState.InitialHabitSheets 
            CurrentHabitSheet = Some []
            HighlightedMonth = "JAN"
            ActiveHabitName = None
        }
    
    static member private InitialHabitSheets =
        HabitSheetState.HighlightableMonths
        |> List.fold (fun (currentState : Map<Month, HabitSheet option>) month -> currentState.Add(month, Some [])) Map.empty
