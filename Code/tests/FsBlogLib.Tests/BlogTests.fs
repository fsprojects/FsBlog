module FsBlogLib.Tests.BlogTests

open FsBlogLib.FileHelpers
open FsBlogLib
open System.IO
open NUnit.Framework

let layouts = "content/layouts"
let root = "http://localhost:8080"
let template = "tools/empty-template.html"

[<Literal>]
let basicPost = "
This is a heading
================="

[<Literal>]
let basicExpectedHtml = "<h1>This is a heading</h1>\r\n\r\n"

[<Literal>]
let youtubePost = "https://www.youtube.com/watch?v=6q_IIWZwEhs"

[<Literal>]
let youtubeExpectedHtml = "<p><iframe id=\"ytplayer\" type=\"text/html\" width=\"640\" height=\"390\" src=\"http://www.youtube.com/embed/6q_IIWZwEhs?autoplay=0\" frameborder=\"0\"/></p>\r\n\r\n"

[<Literal>]
let youtubeNamedPost = "[My video](https://www.youtube.com/watch?v=6q_IIWZwEhs)"

[<Literal>]
let youtubeNamedExpectedHtml = "<p><a href=\"https://www.youtube.com/watch?v=6q_IIWZwEhs\">My video</a></p>\r\n\r\n"

[<Literal>]
let moreComplexPost = "
This is a heading
=================

https://www.youtube.com/watch?v=6q_IIWZwEhs

Some text."

[<Literal>]
let moreComplexExpectedHtml = "<h1>This is a heading</h1>\r\n<p><iframe id=\"ytplayer\" type=\"text/html\" width=\"640\" height=\"390\" src=\"http://www.youtube.com/embed/6q_IIWZwEhs?autoplay=0\" frameborder=\"0\"/></p>\r\n<p>Some text.</p>\r\n\r\n"


[<TestCase(basicPost, basicExpectedHtml)>]
[<TestCase(youtubePost, youtubeExpectedHtml)>]
[<TestCase(youtubeNamedPost, youtubeNamedExpectedHtml)>]
[<TestCase(moreComplexPost, moreComplexExpectedHtml)>]
let ``Transform post`` (post, expected: string) =
    use tempSource = DisposableFile.CreateTemp(".md")
    File.WriteAllText(tempSource.FileName, post)
    let noModel = { Root = root; MonthlyPosts = [||]; Posts = [||]; TaglyPosts = [||]; GenerateAll = true ; SiteTitle = "Test blog"; SiteSubtitle = "Subtitle." }
    let razor = new Razor(layouts, Model = noModel)
    use tempTarget = DisposableFile.CreateTemp()
    Blog.TransformFile template false razor None tempSource.FileName tempTarget.FileName
    let targetText = File.ReadAllText(tempTarget.FileName)
    Assert.AreEqual(expected, targetText)
