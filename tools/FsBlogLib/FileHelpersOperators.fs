namespace FsBlogLib
open System.IO

[<AutoOpen>]
module FileHelpersOperators =
  /// Concantenate path using the right separator
  let (++) p1 p2 = Path.Combine(p1, p2)
