namespace GoPlus.iOS.Renderers

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS
open UIKit
open GameBoardLayout

type GameBoardLayoutRenderer () =
    inherit VisualElementRenderer<Layout> ()

    // System.DateTime.Now
    let mutable lastClick = DateTime.Now;

    override this.TouchesBegan (touches, e) =
        
        base.TouchesBegan (touches, e)

        let gbl = this.Element :?> GameBoardLayout

        let touch = touches.AnyObject :?> UITouch

        let pos = touch.LocationInView(this)

        gbl.GetTouchedColumn(float pos.X, float pos.Y)

        ()

//         Touch += (object sender, TouchEventArgs e) => {
//             if (DateTime.Now.Subtract (lastClick).TotalMilliseconds > 200 && e.Event.Action == MotionEventActions.Up){
//                 GameBoardLayout gbl = ((GameBoardLayout)this.Element);

//                 float realX = (float)((e.Event.GetX () * gbl.Width) / this.Width);
//                 float realY = (float)((e.Event.GetY () * gbl.Height) / this.Height);
// // Logger.Instance.LogDebug (this.ToString (), String.Format ("{0} - {1}", this.Width, gbl.Width));
// // Logger.Instance.LogDebug (this.ToString (), String.Format ("{0} - {1}", e.Event.GetX (), e.Event.GetY ()));
// // Logger.Instance.LogDebug (this.ToString (), String.Format ("{0} - {1}", realX, realY));

//                 gbl.GetTouchedColumn (realX, realY);

//                 lastClick = DateTime.Now;
//             }
//         };
//         ChildViewRemoved += (object sender, ChildViewRemovedEventArgs e) => {
//             e.Child.Dispose ();
//         };
