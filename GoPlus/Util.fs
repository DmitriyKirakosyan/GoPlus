﻿module Util

open System.Runtime.CompilerServices
open System.Collections.Generic

/// Returns true when the given coordinates are inside the bounds given
let boundCheck (x, y) width height = x >= 0 && x < width && y >= 0 && y < height
/// A board that is initially marked all not visited
let noVisits size = Array2D.create (size) (size) false
/// Returns the value of x on a normal distribution, centered around 0
let normal x = (2.718 ** (- 0.5 * x * x)) / (sqrt (2.0 * 3.142))
///gets all but the last element of the given list
let rec allBut input =
    match input with
    | head :: (h::t) -> head :: allBut (h::t)
    | _ -> []
    // | head :: [] -> []
    // | head :: tail -> head :: allBut tail
    // | [] -> []


[<Extension>]
type IListExtensions () =
    [<Extension>]
    static member inline AddRange(xs:'a IList, range) = range |> Seq.iter xs.Add
