
namespace GoPlus.Droid.Renderers

open System
open Xamarin.Forms
open Xamarin.Forms.Platform.Android
open GameBoardLayout

// open Android.OS;

type GameBoardLayoutRenderer () as this =
    inherit VisualElementRenderer<Layout> ()

    // System.DateTime.Now
    let mutable lastClick = DateTime.Now;

    [<assembly: Xamarin.Forms.ExportRendererAttribute (typeof<GameBoardLayout>, typeof<GameBoardLayoutRenderer>)>]
    do        
        this.Touch.Add (fun args -> 
            do
                let gbl = this.Element
                let realX = (float(args.Event.GetX()) * gbl.Width) / float this.Width 
                let realY = (float(args.Event.GetY()) * gbl.Height) / float this.Height


                (gbl :?> GameBoardLayout).GetTouchedColumn (realX, realY)

                 
                // F.. this F#
                ()
        )

        this.ChildViewRemoved.Add ( fun args -> args.Child.Dispose() ) |> ignore
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
