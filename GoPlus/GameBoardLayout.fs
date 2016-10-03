module GameBoardLayout

open Pieces
open Board
open Xamarin.Forms


let colorFromPieceColor color =
    match color with
    |  Pieces.Color.Neutral -> Color.Gray
    |  Pieces.Color.Black -> Color.Black
    |  Pieces.Color.White -> Color.White
    |  Pieces.Color.Pickup (_) -> Color.Aqua


type GameBoardLayout(boardSize, boardViewSize) as this =
    inherit AbsoluteLayout()

    // let mutable playViewSize = 0 

    let fieldSize = boardViewSize / float boardSize

    let mutable wStonePool = []
    let mutable bStonePool = []

    let mutable playedStones = [] : ((int * int) * Image) list

    do
        let boardImage = Image(Source = ImageSource.FromFile("board"))

        AbsoluteLayout.SetLayoutBounds(boardImage, Rectangle(0.0, 0.0, boardViewSize, boardViewSize))

        this.Children.Add(boardImage)

        ()

    // Is there a better way to do this?
    member val OnFieldTap: int * int -> unit = ignore with get, set

    member this.GetTouchedColumn(x: float, y: float) =
        let boardX = int (x / fieldSize)
        let boardY = int (y / fieldSize)

        this.OnFieldTap(boardX, boardY)
        ()

    member this.UpdateFromModel board =
        let size1 = Array2D.length1 board
        let size2 = Array2D.length2 board

        for i = 0 to size1 - 1 do
            for j = 0 to size2 - 1 do
                let stoneIsPlayed = playedStones |> List.exists (fun ((x, y), _) -> x = i && y = j )
                match board.[i,j] with
                | Some (_)  when stoneIsPlayed ->
                    // there is stone already
                    ()
                | Some (color: Pieces.Color, _) ->
                    this.AddStoneWithColor (i, j, color)
                | None when stoneIsPlayed ->
                    this.RemoveStone (i, j)
                // | Some (color, Normal) ->
                //     args.Graphics.FillEllipse(brushFromColor(color), i * squareSize, j * squareSize, squareSize, squareSize)
                // | Some (color, Big (xext, yext)) ->
                //     args.Graphics.FillEllipse(brushFromColor(color), (i - xext) * squareSize, (j - yext) * squareSize, squareSize * (1 + 2 * xext), squareSize * (1 + 2 * yext))
                // | Some (color, L) ->
                //     args.Graphics.FillEllipse(brushFromColor(color), (i) * squareSize, (j) * squareSize, squareSize, squareSize * 2)
                //     args.Graphics.FillEllipse(brushFromColor(color), (i - 2) * squareSize, (j) * squareSize, squareSize * 2, squareSize)
                | None -> ()
        ()

    member private this.AddStoneWithColor(x, y, color) =
        match color with
        | Black -> this.AddStone(x, y, "black_stone")
        | _ -> this.AddStone(x, y, "white_stone")

    member private this.AddStone(x, y, fileName) =
        let stone = Image(Source = ImageSource.FromFile(fileName))
        
        let realX = float x * fieldSize
        let realY = float y * fieldSize

        playedStones <- ((x, y), stone) :: playedStones 

        AbsoluteLayout.SetLayoutBounds(stone, Rectangle(realX, realY, stone.Width, stone.Height))
        this.Children.Add(stone)
        

    member private this.RemoveStone(x, y) =
        let (_, stone) as playedStone = playedStones |> List.find (fun ((sX, sY), _) -> sX = x && sY = y)
        this.Children.Remove(stone) |> ignore
        playedStones <- playedStones |> List.except [playedStone]
