namespace GoPlus.iOS

open System
open UIKit
open Foundation
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS
open GoPlus.iOS.Renderers
open GameBoardLayout

[<assembly: Xamarin.Forms.ExportRendererAttribute (typeof<GameBoardLayout>, typeof<GameBoardLayoutRenderer>)>] do ()

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit FormsApplicationDelegate ()


    override this.FinishedLaunching (app, options) =
        Forms.Init()
        this.LoadApplication (new GoPlus.App())
        base.FinishedLaunching(app, options)

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main(args, null, "AppDelegate")
        0

