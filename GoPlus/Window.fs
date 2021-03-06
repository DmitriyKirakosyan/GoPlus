﻿module Window
// TODO

// 0. make the end game in scoring over network act like passing in the play stage, where both players have to confirm it
// 0.5 Make a new button for scoring stage only that when pressed sets the game back to the last move and returns to the play stage

// 1. make tooltip showing what a powerup is when you hover mouse over it
// 1.5 make a general display label that shows the tooltip and the turn feedback, instead of writing over the powerup label

// 2. make default option be guaranteed powerup every x turns, and which turn in that range is random,
// as opposed to a random chance every turn

// 3. make code for loading played games, going back in time, branching, encoding and decoding the game as a file
// you can visually inspect old states and branched states during play

// 4. make graphics be images

open Pieces
open Board
open Gameplay
open Game
open Util
open GameOptions
open Encoding
open Network
open System
open System.Threading
open System.Collections.Generic
open System.Drawing
open System.Windows.Forms

let brushFromColor color =
    match color with
    |  Pieces.Color.Neutral -> Brushes.Gray
    |  Pieces.Color.Black -> Brushes.Black
    |  Pieces.Color.White -> Brushes.White
    |  Pieces.Color.Pickup (_) -> Brushes.Cyan

let transparentBlack = new SolidBrush (Color.FromArgb (128, Color.Black))
let transparentWhite = new SolidBrush (Color.FromArgb (128, Color.White))

let transparentBrushFromColor color =
    match color with
    |  Pieces.Color.Neutral -> Brushes.Gray
    |  Pieces.Color.Black -> transparentBlack :> Brush
    |  Pieces.Color.White -> transparentWhite :> Brush
    | _ -> failwith "there are no transparent brushes of this color"

type Window (gameSize, gen, powerop, width, height, maybeNetwork, seed) as this =
    inherit Form ()

    /// Instance of the signal event from Network
    let signalReceived = new Event<_> ()

    let listenerThread = 
        match maybeNetwork with
        | Some (networkOptions) -> Some (new Thread(listen networkOptions.Client signalReceived))
        | None -> None

    ///scales a given coordinate by a certain amount and returns it as an int
    let scale coord = (int) ((float coord) * 2.0 / 3.0)

    let mutable (mouseX, mouseY) = (0, 0)

    /// the collection of coordinates of tentative moves
    let mutable curMoves : (int * int) list = [ ]

    let game = new Game (gameSize, gen, powerop, seed)

    let canPlay () =
        match maybeNetwork with
        | Some networkOptions ->
            match game.Stage with
            | Scoring -> true
            | Play ->
                if game.NextToMove = networkOptions.PlayerColor then
                    true
                else
                    false
        | None -> true

    /// Board for displaying, should be updated every click
    let mutable intermediateBoard = game.Board

    let mutable squareSize = (scale width) / (gameSize)

    let mutable errorMessage = ""

    let scoreDisplay = new Label ()
    let turnDisplay = new Label ()
    let powerupDisplay = new Label ()
    let endGameButton = new Button ()
    let revertButton = new Button ()
    let undoButton = new Button ()

    let makePass () =
        game.Pass () |> ignore
        errorMessage <- ""
        this.Invalidate ()

    let undo args =
        errorMessage <- ""
        match List.length curMoves with
        | 0 -> ()
        | 1 ->
            let lastMove = curMoves.[List.length curMoves - 1]
            curMoves <- allBut curMoves
            intermediateBoard <- game.Board
            if game.Stage = Scoring && Option.isSome maybeNetwork then
                let networkOptions = Option.get maybeNetwork
                sendMoves networkOptions.Client GameMessage.Undo
            this.Invalidate ()
        | _ ->
            let lastMove = curMoves.[List.length curMoves - 1]
            curMoves <- allBut curMoves
            match game.CalculateState curMoves with
            | Accept (_, intermediateState) ->
                intermediateBoard <- intermediateState.board
                if game.Stage = Scoring && Option.isSome maybeNetwork then
                    let networkOptions = Option.get maybeNetwork
                    sendMoves networkOptions.Client GameMessage.Undo
                this.Invalidate ()
            | _ -> failwith "shouldn't be able to fail by removing a move"

    let endGame args =
        match game.Stage with
        | Play ->
            if canPlay () then
                makePass ()
                if Option.isSome maybeNetwork then
                    let networkOptions = Option.get maybeNetwork
                    sendMoves networkOptions.Client GameMessage.Pass
                this.Invalidate ()
        | Scoring ->
            game.MakeMoves curMoves |> ignore
            if Option.isSome maybeNetwork then
                let networkOptions = Option.get maybeNetwork
                sendMoves networkOptions.Client GameMessage.Pass
            let (blackScore, whiteScore) = game.CalulateScore ()
            MessageBox.Show(String.Format("black score: {0}, white score: {1}", blackScore, whiteScore)) |> ignore
            this.Close ()
    
    let revertMode args =
        let result = game.RevertToPlay ()
        match result with
        | Reject _ -> ()
        | Accept () ->
            intermediateBoard <- game.Board
            curMoves <- []
            if Option.isSome maybeNetwork then
                let networkOptions = Option.get maybeNetwork
                sendMoves networkOptions.Client GameMessage.Revert
            this.Invalidate ()

    do
        this.Text <- "GoPlus"
        this.ClientSize <- new Size(width, height)
        this.SetStyle (ControlStyles.AllPaintingInWmPaint, true)
        this.SetStyle (ControlStyles.UserPaint, true)
        this.SetStyle (ControlStyles.OptimizedDoubleBuffer, true)
        this.FormBorderStyle <- FormBorderStyle.Sizable
        this.MaximizeBox <- true
        this.BackColor <- Color.Tan
        scoreDisplay.Text <- ""
        scoreDisplay.Dock <- DockStyle.Right
        scoreDisplay.AutoSize <- true
        turnDisplay.Text <- ""
        turnDisplay.Size <- new Size(120, 50) //bad hardcoding make it better eventually
        turnDisplay.Location <- new Point(this.ClientSize.Width - turnDisplay.Size.Width, scoreDisplay.Size.Height)
        powerupDisplay.Text <- ""
        powerupDisplay.Size <- new Size(120, 50) //bad hardcoding make it better eventually
        powerupDisplay.Location <- new Point(this.ClientSize.Width - turnDisplay.Size.Width, scoreDisplay.Size.Height + turnDisplay.Height)
        undoButton.Text <- "Undo"
        undoButton.Dock <- DockStyle.Bottom
        undoButton.AutoSize <- true
        undoButton.Click.Add undo
        endGameButton.Text <- "Pass"
        endGameButton.Dock <- DockStyle.Bottom
        endGameButton.AutoSize <- true
        endGameButton.Click.Add endGame
        revertButton.Text <- "Revert"
        revertButton.Dock <- DockStyle.Bottom
        revertButton.AutoSize <- true
        revertButton.Click.Add revertMode
        this.Controls.AddRange [| scoreDisplay; turnDisplay; powerupDisplay; endGameButton; undoButton; revertButton |]
        signalReceived.Publish.Add (this.OnSignalReceived)
        

        //if there is a listener thread, start it now
        match listenerThread with
        | Some thread -> thread.Start ()
        | None -> ()

    override this.OnClosed args =
        Environment.Exit (0)

    // Window will handle timer and whose turn it is, it will translate ui actions into function calls on Game.
    // It decides when things happen, Game implements them
    override this.OnMouseMove args =
        mouseX <- args.X
        mouseY <- args.Y
        this.Invalidate ()

    override this.OnResize args =
        base.OnResize args
        squareSize <- (scale this.ClientSize.Width ) / gameSize
        turnDisplay.Location <- new Point(this.ClientSize.Width - turnDisplay.Size.Width, scoreDisplay.Size.Height)
        this.Invalidate ()
    
    override this.OnMouseDown args =
        if args.X < scale this.ClientSize.Width && args.Y < scale this.ClientSize.Width 
           && canPlay () then
            let x = args.X / squareSize
            let y = args.Y / squareSize
            let moves = curMoves @ [(x, y)]
            if game.GetMovesNeeded () = List.length moves && game.Stage = Play then
                match game.MakeMoves moves with
                | Accept () ->
                    intermediateBoard <- game.Board
                    curMoves <- []
                    errorMessage <- ""
                    //send the moves to the other player
                    if Option.isSome maybeNetwork then
                        let networkOptions = Option.get maybeNetwork
                        sendMoves networkOptions.Client (GameMessage.Moves moves)
                | Reject message ->
                    errorMessage <- message
            else
                match game.CalculateState moves with
                | Accept (_, intermediateState) ->
                    intermediateBoard <- intermediateState.board
                    curMoves <- moves
                    errorMessage <- ""
                    if game.Stage = Scoring && Option.isSome maybeNetwork then
                    //send the intermediate move if it's scoring mode
                        let networkOptions = Option.get maybeNetwork
                        sendMoves networkOptions.Client (GameMessage.Moves moves)
                | Reject message ->
                    errorMessage <- message
            this.Invalidate ()

    [<CLIEvent>]
    member this.SignalReceived = signalReceived.Publish

    /// This is where the Window handles a move by the other player
    member this.OnSignalReceived (args : SignalArgs) =
        let message = args.Message
        let networkOptions = Option.get maybeNetwork
        match message with
        | Undo ->
            curMoves <- allBut curMoves
            match curMoves with
            | [] ->
                intermediateBoard <- game.Board
                this.Invalidate ()
            | _ ->
                match game.CalculateState curMoves with
                | Accept (_, intermediateState) ->
                    intermediateBoard <- intermediateState.board
                    this.Invalidate ()
                | _ -> failwith "the other player is broken or cheating"
        | Pass ->
            match game.Stage with
            | Play -> makePass ()  
            | Scoring ->
                //end the game
                game.MakeMoves curMoves |> ignore
                let (blackScore, whiteScore) = game.CalulateScore ()
                MessageBox.Show(String.Format("black score: {0}, white score: {1}", blackScore, whiteScore)) |> ignore
                this.Invoke(new MethodInvoker (this.Close)) |> ignore
        | Revert ->
            match game.RevertToPlay () with
            | Accept () ->
                intermediateBoard <- game.Board
                curMoves <- []
                this.Invalidate ()
            | Reject message ->
                failwith "the other player is broken or cheating"
        | Moves moves ->
            if game.GetMovesNeeded () = List.length moves 
                && game.Stage = Play 
                && not (canPlay ()) then
                match game.MakeMoves moves with
                | Accept () ->
                    intermediateBoard <- game.Board
                | Reject message ->
                    failwith "the other player is broken or cheating"
            elif game.Stage = Scoring then
                //don't wait for the moves needed, just update the current moves stack across the network
                curMoves <- moves
                match game.CalculateState curMoves with
                | Accept (_, intermediateState) ->
                    intermediateBoard <- intermediateState.board
                    errorMessage <- ""
                | _ -> failwith "the other player is broken or cheating"
        this.Invalidate ()
    
    override this.OnPaint args =
        endGameButton.Text <- 
            match game.Stage with
            | Play -> "Pass"
            | Scoring -> "End Game"
            
        scoreDisplay.Text <-
            let scoreString = 
                match maybeNetwork with
                | Some networkOptions -> String.Format("score: {0}", game.GetScore networkOptions.PlayerColor)
                | None -> String.Format("current player's score: {0}", game.GetScore game.NextToMove)
            match game.Stage with
            | Play -> scoreString
            | Scoring -> ""

        turnDisplay.Text <-
            match game.Stage with
            | Play ->
                match game.NextToMove with
                | Black ->
                    "Black's move"
                | White ->
                    "White's move"
                | _ -> failwith "it can only be White or Black's turn during play"
            | Scoring ->
                "Scoring Mode"

        powerupDisplay.Text <-
            match errorMessage with
            | "" ->
                match game.Stage with
                | Play ->
                    let color =
                        match maybeNetwork with
                        | None -> game.NextToMove
                        | Some networkOptions -> networkOptions.PlayerColor
                    match game.GetPlayerPowerup color with
                    | None ->
                        "No Powerup"
                    | Some x ->
                        match canPlay () with
                        | true -> 
                            String.Format("{0}, {1} moves remaining", Powerup.powerupString x color, game.GetMovesNeeded () - List.length curMoves)
                        | false -> Powerup.powerupString x color
                        
                | Scoring ->
                    ""
            | x -> x

        let size1 = Array2D.length1 game.Board
        let size2 = Array2D.length2 game.Board
        for i = 0 to size1 - 1 do
            args.Graphics.DrawLine(Pens.Black, 0, i * squareSize + (squareSize / 2), scale this.ClientSize.Width, i * squareSize + (squareSize / 2))
        for j = 0 to size2 - 1 do
            args.Graphics.DrawLine(Pens.Black, j * squareSize + (squareSize / 2), 0, j * squareSize + (squareSize / 2), scale this.ClientSize.Width)

        //draw a ghost piece over where the player would place a piece
        if mouseX < scale this.ClientSize.Width && mouseY < scale this.ClientSize.Width then
            match game.Stage with
            | Play ->
                let x = mouseX / squareSize
                let y = mouseY / squareSize
                if boundCheck (x, y) size1 size2 then
                    let color =
                        match maybeNetwork with
                        | Some networkOptions -> networkOptions.PlayerColor
                        | None -> game.NextToMove
                    args.Graphics.FillEllipse(transparentBrushFromColor(color), x * squareSize, y * squareSize, squareSize, squareSize) 
            | Scoring -> ()

        for i = 0 to size1 - 1 do
            for j = 0 to size2 - 1 do
                match intermediateBoard.[i,j] with
                | Some (color, Normal) ->
                    args.Graphics.FillEllipse(brushFromColor(color), i * squareSize, j * squareSize, squareSize, squareSize)
                | Some (color, Big (xext, yext)) ->
                    args.Graphics.FillEllipse(brushFromColor(color), (i - xext) * squareSize, (j - yext) * squareSize, squareSize * (1 + 2 * xext), squareSize * (1 + 2 * yext))
                | Some (color, L) ->
                    args.Graphics.FillEllipse(brushFromColor(color), (i) * squareSize, (j) * squareSize, squareSize, squareSize * 2)
                    args.Graphics.FillEllipse(brushFromColor(color), (i - 2) * squareSize, (j) * squareSize, squareSize * 2, squareSize)
                | None -> ()