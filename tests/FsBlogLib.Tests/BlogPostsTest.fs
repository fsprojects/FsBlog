module FsBlogLib.Tests

open Xunit
open FsBlogLib.FileHelpers
open FsBlogLib.BlogPosts
open FsUnit.Xunit
open System
open System.IO

[<Fact>]
let ``Create blog post`` () =
    let path = __SOURCE_DIRECTORY__ ++ "..\\source\\blog"
    let title = "FsharpTest"
    let dateFormat = DateTime.Now
    let expectedPath = path ++ dateFormat.Year.ToString() ++ sprintf "%02i-%02i-%s" dateFormat.Month dateFormat.Day "fsharptest.md"
    CreateMarkdownPost path title
    Assert.True(File.Exists(expectedPath))
