(**
# FsBlog Script

This script is the main workhorse of FsBlog that just coordinates the commands
and tasks that operate with the static site generation.
*)

#r "packages/FAKE/tools/FakeLib.dll"
#r "bin/FsBlogLib/RazorEngine.dll"
#r "bin/FsBlogLib/FsBlogLib.dll"
open Fake
open System
open System.IO
open System.Text.RegularExpressions
open RazorEngine
open FsBlogLib.FileHelpers
open FsBlogLib.BlogPosts
open FsBlogLib.Blog
open FSharp.Http


// --------------------------------------------------------------------------------------
// Configuration.
// --------------------------------------------------------------------------------------
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let root = "http://saxonmatt.co.uk/fsblog"
let title = "FsBlog - F# static site generation"
let description = """
    FsBlog aims to be a blog-aware static site generator, mostly built in F#. But don't worry, 
    you won't even need to know any F# to get up and running. So long as you are comfortable 
    using a command line or terminal, and have a degree of familiarity with Markdown and Razor 
    syntax - you're good to go!"""

let source = __SOURCE_DIRECTORY__ ++ "source/"
let blog = __SOURCE_DIRECTORY__ ++ "source/blog/"
let blogIndex = __SOURCE_DIRECTORY__ ++ "source/blog/index.cshtml"
let layouts = __SOURCE_DIRECTORY__ ++ "layouts"
let content = __SOURCE_DIRECTORY__ ++ "content"
let template = __SOURCE_DIRECTORY__ ++ "empty-template.html"

let output = __SOURCE_DIRECTORY__ ++ "output"

let tagRenames = List.empty<string*string> |> dict
let exclude = []
let references = []
let dependencies = [ yield! Directory.GetFiles(layouts) ] 
let special =
    [ source ++ "index.cshtml"
      source ++ "blog" ++ "index.cshtml" ]


// --------------------------------------------------------------------------------------
// Static site tooling as a set of targets.
// --------------------------------------------------------------------------------------

/// Regenerates the entire static website from source files (markdown and fsx).
Target "Generate" (fun _ ->

    let buildSite (updateTagArchive) =
        let noModel = { Model.Root = root; MonthlyPosts = [||]; Posts = [||]; TaglyPosts = [||]; GenerateAll = true }
        let razor = FsBlogLib.Razor(layouts, Model = noModel)
        let model = LoadModel(tagRenames, TransformAsTemp (template, source) razor, root, blog)

        // Generate RSS feed
        GenerateRss root title description model (output ++ "rss.xml")

        let uk = System.Globalization.CultureInfo.GetCultureInfo("en-GB")
        GeneratePostListing 
            layouts template blogIndex model model.MonthlyPosts 
            (fun (y, m, _) -> output ++ "blog" ++ "archive" ++ (m.ToLower() + "-" + (string y)) ++ "index.html")
            (fun (y, m, _) -> y = DateTime.Now.Year && m = uk.DateTimeFormat.GetMonthName(DateTime.Now.Month))
            (fun (y, m, _) -> sprintf "%d %s" y m)
            (fun (_, _, p) -> p)

        if updateTagArchive then
            GeneratePostListing 
                layouts template blogIndex model model.TaglyPosts
                (fun (_, u, _) -> output ++ "blog" ++ "tag" ++ u ++ "index.html")
                (fun (_, _, _) -> true)
                (fun (t, _, _) -> t)
                (fun (_, _, p) -> p)

        let filesToProcess = 
            GetSourceFiles source output
            |> SkipExcludedFiles exclude
            |> TransformOutputFiles output
            |> FilterChangedFiles dependencies special
    
        let razor = FsBlogLib.Razor(layouts, Model = model)
        for current, target in filesToProcess do
            EnsureDirectory(Path.GetDirectoryName(target))
            printfn "Processing file: %s" (current.Substring(source.Length + 1))
            TransformFile template true razor None current target

        CopyFiles content output 

    buildSite (true)
)

Target "Preview" (fun _ ->
    let server : ref<option<HttpServer>> = ref None
    
    let stop () = server.Value |> Option.iter (fun v -> v.Stop())
    
    let run() =
        let url = "http://localhost:8080/" 
        stop ()
        server := Some(HttpServer.Start(url, output, Replacements = [root, url]))
        printfn "Starting web server at %s" url
        System.Diagnostics.Process.Start(url) |> ignore
        
    run ()
)

Target "New" (fun _ ->       
    let post, fsx, page = 
        getBuildParam "post", 
        getBuildParam "fsx",
        getBuildParam "page"    
    
    match page, post, fsx with
    | "", "", "" -> traceError "Please specify either a new 'page', 'post' or 'fsx'."
    | _, "", ""  -> CreateMarkdownPage source page
    | "", _, ""  -> CreateMarkdownPost blog post
    | "", "", _  -> CreateFsxPost blog fsx
    | _, _, _    -> traceError "Please specify only one argument, 'post' or 'fsx'."
)

Target "Deploy" DoNothing

Target "Commit" DoNothing

"Generate" ==> "Preview"

Target "Help" (fun _ ->
    traceImportant "Welcome to FsBlog!"
    trace          ""
    traceImportant "Usage:"
    traceImportant "fake <command> <args optional>"
    trace          ""
    traceImportant "Commands available:"
    traceImportant " 1. generate"
    traceImportant " 2. preview"
    traceImportant " 3. new [args]"
    traceImportant "       post=\"hello markdown post\""
    traceImportant "        fsx=\"hello fsx post\""
    traceImportant " 4. help"
    trace          ""
)

// --------------------------------------------------------------------------------------
// Run a specified target.
// --------------------------------------------------------------------------------------
RunTargetOrDefault "Help"