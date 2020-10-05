module StartApp

#if DEBUG
open Elmish.Debug
open Elmish.HMR
open Elmish
open Fable.Core
open View
open State
#endif

JsInterop.importAll "E:/Programming shit/HabitTracker/src/Client/style.scss"
Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run