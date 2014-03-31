(**
# FsBlog Script

This script is the main workhorse of FsBlog that just coordinates the commands
and tasks that operate with the static site generation.
*)

#I "packages/FAKE/tools/"
#r "packages/FAKE/tools/FakeLib.dll"
#r "bin/FsBlogLib/RazorEngine.dll"
#r "bin/FsBlogLib/FsBlogLib.dll"
#r "bin/FsBlogLib/FSharp.Configuration.dll"

open Fake
open Fake.Git
open System
open System.IO
open System.Text.RegularExpressions
open RazorEngine
open FsBlogLib.FileHelpers
open FsBlogLib.BlogPosts
open FsBlogLib.Blog
open FSharp.Http
open FSharp.Configuration

// --------------------------------------------------------------------------------------
// Configuration.
// --------------------------------------------------------------------------------------
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
type Config = YamlConfig<"config/config.yml">

let config = Config()
let root = config.url.AbsoluteUri
let title = config.title
let description = config.description
let gitLocation = config.gitlocation
let gitbranch = config.gitbranch

let source = __SOURCE_DIRECTORY__ ++ config.source
let blog = __SOURCE_DIRECTORY__ ++ config.blog
let blogIndex = __SOURCE_DIRECTORY__ ++ config.blogIndex
let themes = __SOURCE_DIRECTORY__ ++ config.themes
let content = __SOURCE_DIRECTORY__ ++ config.content
let layouts = content ++ config.layouts
let template = __SOURCE_DIRECTORY__ ++ config.template

let output = __SOURCE_DIRECTORY__ ++ config.output
let deploy = __SOURCE_DIRECTORY__ ++ config.deploy

let tagRenames = List.empty<string*string> |> dict
let exclude = []
let references = []

let special =
    [ source ++ "index.cshtml"
      source ++ "blog" ++ "index.cshtml" ]
let rsscount = 20

// --------------------------------------------------------------------------------------
// Static site tooling as a set of targets.
// --------------------------------------------------------------------------------------

/// Regenerates the entire static website from source files (markdown and fsx).
Target "Generate" (fun _ ->

    let buildSite (updateTagArchive) =
        let dependencies = [ yield! Directory.GetFiles(layouts) ] 
        let noModel = { Model.Root = root; MonthlyPosts = [||]; Posts = [||]; TaglyPosts = [||]; GenerateAll = true }
        let razor = FsBlogLib.Razor(layouts, Model = noModel)
        let model = LoadModel(tagRenames, TransformAsTemp (template, source) razor, root, blog)

        // Generate RSS feed
        GenerateRss root title description model rsscount (output ++ "rss.xml")

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
            printfn "Processing file: %s" (current.Substring(source.Length))
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

    traceImportant "Press Ctrl+C to stop!"
    // wat!?    
    while true do ()
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

Target "Clean" (fun _ ->
    CleanDirs [output]
)

Target "Deploy" DoNothing

Target "Commit" DoNothing

Target "DoNothing" DoNothing

Target "GitClone" (fun _ ->
    if(FileSystemHelper.directoryExists(deploy ++ ".git")) then
        ()
    else
        Repository.cloneSingleBranch __SOURCE_DIRECTORY__ gitLocation.AbsoluteUri gitbranch deploy
)

Target "GitPublish" (fun _ -> 
    CopyRecursive output deploy true |> ignore
    CommandHelper.runSimpleGitCommand deploy "add ." |> printfn "%s"
    let cmd = sprintf """commit -a -m "Update generated web site (%s)""" (DateTime.Now.ToString("dd MMMM yyyy"))
    CommandHelper.runSimpleGitCommand deploy cmd |> printfn "%s"
    Branches.push deploy
)

Target "Install" (fun _ -> 
    let theme = getBuildParam "theme"

    match theme with
    | "" -> traceError "Please specify theme"
    | _ -> 
           CleanDir content
           CopyDir content (themes ++ theme) (fun file -> not(file.StartsWith(themes ++ theme ++ "source"))) |> ignore
           CopyRecursive (themes ++ theme ++ "source") source true |> ignore
)

"DoNothing" =?> 
("Install", hasBuildParam "theme")  ==>
"Generate" ==> "Preview"

"Clean" ==>
"Generate" ==>
"GitClone" ==> "GitPublish"

// --------------------------------------------------------------------------------------
// Run a specified target.
// --------------------------------------------------------------------------------------
RunTargetOrDefault "Preview"