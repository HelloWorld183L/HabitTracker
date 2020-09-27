module StartApp

#if DEBUG
open Elmish.Debug
open Elmish.HMR
open Fable.Core
open Elmish
open Fable.Core
open View
open State
#endif

Fable.Core.JsInterop.importAll "C:/Users/Leon/source/repos/HabitTracker/src/Client/style.scss"

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run