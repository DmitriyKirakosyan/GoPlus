module Menu

open Window
open Encoding
open Network
open System
open System.Net
//open System.Net.Sockets
open System.Collections.Generic
//open System.Drawing
//open System.Windows.Forms
open Xamarin.Forms
open GameOptions
open Util


let port = 33764

type Menu () as self =
    inherit ContentPage ()

    let mutable gameStarted = false

    let stack = StackLayout(VerticalOptions = LayoutOptions.Center)

    let gameSizeLabel = new Label()
    let gameSizeBox = new Entry()
    let networkLabel = new Label ()
    let networkBox = new Entry ()
    let powerUpLabel = new Label ()
    let neutralSwitch = new Switch ()
    let powerUpBox = new Entry()
    let startGameButton = new Button ()
    let hostGameButton = new Button ()
    let goswips = new Label ()


        

    // let startButtonClicked (sender : Object, args : EventArgs) =
    //     startGame false

    do
        // this.Text <- "GoPlus"
        // this.ClientSize <- new Size(width, height)
        // this.SetStyle (ControlStyles.AllPaintingInWmPaint, true)
        // this.SetStyle (ControlStyles.UserPaint, true)
        // this.SetStyle (ControlStyles.OptimizedDoubleBuffer, true)
        // this.FormBorderStyle <- FormBorderStyle.Fixed3D
        // this.MaximizeBox <- false
        // this.BackColor <- Color.Tan
        

        powerUpLabel.Text <- "powerup frequency"
        powerUpLabel.HorizontalTextAlignment <- TextAlignment.Center

        // neutralSwitch.Text <- "enable neutral stones"
        // neutralSwitch.Dock <- DockStyle.Top
        // neutralCheck.AutoSize <- true
        gameSizeLabel.Text <- "enter board size"
        gameSizeLabel.HorizontalTextAlignment <- TextAlignment.Center
        // gameSizeLabel.AutoSize <- true

        gameSizeBox.WidthRequest <- 50.0
        // gameSizeBox.Dock <- DockStyle.Top
        // gameSizeBox.AutoSize <- true
        networkLabel.Text <- "enter IP address"
        // networkLabel.Dock <- DockStyle.Top
        // networkLabel.AutoSize <- true
        networkBox.WidthRequest <- 50.0
        // networkBox.Dock <- DockStyle.Top
        // networkBox.AutoSize <- true
        goswips.FontFamily <- "Arial"
        goswips.FontSize <- 47.0
        //goswips.Font <- new Drawing.Font ("Arial", 47.0f)
        goswips.HorizontalTextAlignment <- TextAlignment.Center
        goswips.Text <- "WELCOME TO GoPlus"
        //goswips.Dock <- DockStyle.Fill
        startGameButton.Text <- "Join/Start Game"
        //startGameButton.Dock <- DockStyle.Bottom
        //startGameButton.AutoSize <- true


        startGameButton.Clicked.Add( fun _ -> self.StartGame(false) )
        stack.Children.AddRange [
            goswips
            gameSizeLabel; gameSizeBox
            networkLabel; networkBox
            powerUpLabel
            neutralSwitch
            powerUpBox
            startGameButton
        ]
        base.Content <- stack


    member private this.StartGame host =
        let client = None
            // if host then
            //     let listener = new TcpListener(IPAddress.Any, port)
            //     listener.Start ()
            //     printfn "I am hosting and waiting"
            //     let client = listener.AcceptTcpClient ()
            //     printfn "%s" (client.ToString ())
            //     Some (client)
            // elif networkBox.Text <> "" then
            //     let ipAddress = IPAddress.Parse (networkBox.Text)
            //     let endpoint = new IPEndPoint (ipAddress, port)
            //     let requester = new TcpClient ()
            //     printfn "I am connecting"
            //     requester.Connect (endpoint)
            //     printfn "%s" (requester.ToString ())
            //     Some (requester)
            // else
            //     None
        
        let gameSize = if gameSizeBox.Text = "" || gameSizeBox.Text = null then 19 else Convert.ToInt32 gameSizeBox.Text
        let powerOp =

            // let radio = Array.find (fun (x : RadioButton) -> x.Checked) powerUpFreq
            match powerUpBox.Text with
            | "Vanilla" -> Vanilla
            | "Low" -> Low
            | "Medium" -> Medium
            | "High" -> High
            | "Guaranteed" -> Guaranteed
            | _ -> Vanilla
            //| text -> failwith(String.Format("[Menu.StartGame] Radio button had text that isn't supported {0}", text))
        let steve = new Window (gameSize, { NeutralGen = neutralSwitch.IsToggled }, powerOp, None, (new Random ()).Next ())

        // let steve =     
        //     match client with
        //     | None ->
        //         let gameSize = if gameSizeBox.Text = "" then 19 else Convert.ToInt32 gameSizeBox.Text
        //         let powerOp =
        //             let radio = Array.find (fun (x : RadioButton) -> x.Checked) powerUpFreq
        //             match radio.Text with
        //             | "Vanilla" -> Vanilla
        //             | "Low" -> Low
        //             | "Medium" -> Medium
        //             | "High" -> High
        //             | "Guaranteed" -> Guaranteed
        //             | _ -> failwith "radio button had text that isn't supported"
        //         new Window (gameSize, { NeutralGen = neutralCheck.Checked }, powerOp, 640, 480, None, (new Random ()).Next ())
        //     | Some (client) when host ->
        //         let gameSize = if gameSizeBox.Text = "" then 19 else Convert.ToInt32 gameSizeBox.Text
        //         let genOp = { NeutralGen = neutralCheck.Checked }
        //         let powerOp =
        //             let radio = Array.find (fun (x : RadioButton) -> x.Checked) powerUpFreq
        //             match radio.Text with
        //             | "Vanilla" -> Vanilla
        //             | "Low" -> Low
        //             | "Medium" -> Medium
        //             | "High" -> High
        //             | "Guaranteed" -> Guaranteed
        //             | _ -> failwith "radio button had text that isn't supported"
        //         let seed = (new Random ()).Next ()

        //         // send the game generation info to the other player
        //         let message = gameInfoToBytes gameSize genOp powerOp seed

        //         client.GetStream().Write (message, 0, message.Length)

        //         let network = { Client = client; PlayerColor = Pieces.Color.Black }
        //         new Window (gameSize, genOp, powerOp, 640, 480, Some network, seed)
        //     | Some (client) when not host ->
        //         // read the gameInfo from the host
        //         let gameInfo = Array.zeroCreate 10
        //         if client.GetStream().Read (gameInfo, 0, gameInfo.Length) <> gameInfo.Length then
        //             failwith "could not read game info from the host"
                    
        //         let (gameSize, genOp, powerOp, seed) = decodeGameInfo gameInfo
        //         let network = { Client = client; PlayerColor = Pieces.Color.White }
        //         new Window (gameSize, genOp, powerOp, 640, 480, Some network, seed)

        
        base.Navigation.PushAsync(steve) |> ignore
        // this.Hide ()
        // steve.Show ()
        gameStarted <- true
        // this.Close ()
        ()


        // hostGameButton.Text <- "Host Game"
        // hostGameButton.Dock <- DockStyle.Bottom
        // hostGameButton.AutoSize <- true
        // hostGameButton.Click.Add (startGame true)
        // this.Controls.AddRange [| networkLabel; networkBox; gameSizeLabel; gameSizeBox; startGameButton; hostGameButton; goswips; neutralCheck |]
        // this.Controls.AddRange [| for i in powerUpFreq do yield i :> Control |]
        // this.Controls.Add powerUpLabel

    // override this.OnClosed args =
    //     if gameStarted then
    //         ()
    //     else
    //         Environment.Exit (0)