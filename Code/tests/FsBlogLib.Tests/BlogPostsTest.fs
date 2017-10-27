module FsBlogLib.Tests.BlogPostsTests

open FsBlogLib
open System
open System.IO
open NUnit.Framework

[<Test>]
let ``Create blog post`` () =
    let path = __SOURCE_DIRECTORY__ ++ "..\\source\\blog"
    let title = "FsharpTest"
    let dateFormat = DateTime.Now
    let expectedPath = path ++ dateFormat.Year.ToString() ++ sprintf "%02i-%02i-%s" dateFormat.Month dateFormat.Day "fsharptest.md"
    BlogPosts.CreateMarkdownPost path title
    Assert.True(File.Exists(expectedPath))
