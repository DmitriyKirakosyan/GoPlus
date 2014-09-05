﻿module Game
// Consolidation of all game logic, so any user interface's interaction with game logic is requesting actions by game and having them either accepted or rejected
// Game will check for validity of actions and carry them out itself, so the ui will have no business logic in it and be completely replaceable and still have the same functioning game
open Util
open Pieces
open Board
open GameOptions
open BoardGen
open Player
/// An error checking type that is returned by all commands to the game
type ActionResponse =
    | Accept
    | Reject of string

type Game (size, genop, powerop) =
    let oldBoards = new System.Collections.Generic.List<Option<Piece>[,]>()
    let mutable board = generate genop size
    let mutable cells = genCells board
    let playerBlack = new Player (Black)
    let playerWhite = new Player (White)
    /// The function to advance the board to the next state, should be the only way the board is changed
    let changeBoard newBoard =
        oldBoards.Add board
        board <- newBoard
        cells <- genCells board

    /// Returns the cells of a potential board after a piece is placed
    let potentialBoard piece color (x, y) =
        let preCheck = addPieces board [ (piece, (x, y)) ] // board with potential piece added before captures
        let postCheck = // board after the potential piece is placed
            preCheck
            |> genCells
            |> checkDead color // checks using the color placed so that if the piece placed is dead, it isn't taken out here before the next check for dead pieces of that color
            |> removePieces preCheck // if the placing of this piece would capture stones, the stones are captured here, so the piece placed isn't counted as dead if it captures a group
            |> genCells
        postCheck

    /// Returns the game's board for displaying
    member this.Board = board

    /// Adds a piece to this location if it's valid, then checks for dead pieces using the given color (for a situation where white is adding a black piece), returning an ActionResponse to signal success or failure
    member this.AddPiece piece color (x, y) =
        // add bounds checking, existing piece checking, optimality checking, and ko rule
        let bounds =
            if List.filter (fun i -> boundCheck i size size = false) (pieceCoords (snd piece) (x, y)) = [] then Accept
            else Reject "Piece would be out of bounds"
        let existing = function
            | Accept ->
                if List.filter (fun (x, y) -> cells.[x,y] <> Free) (pieceCoords (snd piece) (x, y)) = [] then Accept
                else Reject "Piece already exists there"
            | Reject message -> Reject message
        let optimal = function
            | Accept ->
                let potential = potentialBoard piece color (x, y)
                let lastColorDead =
                    potential
                    |> checkDead Neutral // color parameter is neutral because a neutral piece will never be placed
                    |> List.filter (fun (x, y) -> potential.[x,y] = Cell.Taken color) //only cares about the color who just placed a piece having dead groups
                if lastColorDead = [] then Accept
                else Reject "Placing a piece there would cause that piece to be dead"
            | Reject message -> Reject message
        let ko = function
            | Accept ->
                match oldBoards.Count with
                | 0 -> Accept
                | _ ->
                    let newBoard = potentialBoard piece color (x, y)
                    let oldBoard = oldBoards.Item (oldBoards.Count - 1) |> genCells
                    let diff =
                        [
                            for i = 0 to size - 1 do
                                for j = 0 to size - 1 do
                                    if newBoard.[i,j] <> oldBoard.[i,j] then yield oldBoard.[i,j]
                        ]
                    if diff <> [] then Accept
                    else Reject "Placing that piece would violate the ko rule"
            | Reject message -> Reject message
        let success = function
            | Accept -> //only performs modifications if all checks succeeded
                let temp = addPieces board [ (piece, (x, y)) ]
                let numDead =
                    temp
                    |> genCells
                    |> checkDead color
                numDead
                |> List.filter (fun (x, y) -> board.[x,y] <> None)
                |> removePieces temp
                |> changeBoard
                match color with // add the points to the player's score who captured pieces
                | Black -> List.length numDead |> playerBlack.AddScore
                | White -> List.length numDead |> playerWhite.AddScore
                | Neutral -> ()
                Accept
            | Reject message -> Reject message
        bounds |> existing |> optimal |> ko |> success //does a sequence of checks and returns whether or not a problem occured and where

    /// Removes a piece if it exists at the given location
    member this.RemovePiece (x, y) =
        let mutable response = Reject "No piece at given location"
        for i = 0 to size - 1 do
            for j = 0 to size - 1 do
                match board.[i,j] with
                | Some (_, shape) -> 
                    match pieceCoords shape (i, j) |> List.tryFind (fun p -> p = (x, y)) with
                    | Option.Some p ->
                        changeBoard (removePieces board [ (i, j) ])
                        response <- Accept
                    | Option.None -> ()
                | None -> ()
        response

    /// Marks the group at the current location dead, gives all its pieces to the player whose color was not marked, and removes the pices from the board
    member this.MarkDead (x, y) =
        let initial = (cells).[x,y]
        match initial with
        | Taken White -> 
            let dead = genGroup (x,y) cells
            playerBlack.AddScore (List.length dead)
            changeBoard (removePieces board dead)
            Accept
        | Taken Black ->
            let dead = genGroup (x,y) cells
            playerWhite.AddScore (List.length dead)
            changeBoard (removePieces board dead)
            Accept
        | Taken Neutral ->
            let dead = genGroup (x,y) cells
            changeBoard (removePieces board dead)
            Accept
        | Free -> Reject "No group here to remove"
    /// Ends the game and calculates total score, assumes all groups are alive, adds to score by enclosed empty squares
    member this.CalulateScore () =
        let visited = (noVisits size)
        /// Returns the nonempty pieces adjacent
        let findEnclosing (x, y) = 
            let output =
                [(x - 1, y); (x + 1, y); (x, y - 1); (x, y + 1)] 
                |> List.filter (fun (x, y) -> boundCheck (x, y) (size) (size))
                |> List.filter (fun (x, y) -> cells.[x,y] <> Free)
//            List.iter (fun (x, y) -> true |> Array2D.set visited x y) output
            output
        for i = 0 to size - 1 do
            for j = 0 to size - 1 do
                if not visited.[i,j] && cells.[i,j] = Free then 
                    let group = genGroup (i, j) cells
                    List.iter (fun (x, y) -> true |> Array2D.set visited x y) group
                    let enclosingPieces = [ for p in group do yield! findEnclosing p ]
                    let rec enclosingColor pieces comparePiece =
                        match pieces with
                        | [] -> Some comparePiece
                        | (x, y) :: _ ->
                            if comparePiece = cells.[x,y] then enclosingColor (List.tail pieces) comparePiece
                            else None
                    match enclosingColor enclosingPieces cells.[fst enclosingPieces.[0], snd enclosingPieces.[0]] with
                    | Some (Taken Black) ->
                        playerBlack.AddScore (List.length group)
                    | Some (Taken White) ->
                        playerWhite.AddScore (List.length group)
                    | Some (Taken Neutral) -> ()
                    | None -> ()

    member this.BlackScore = playerBlack.Score
    member this.WhiteScore = playerWhite.Score