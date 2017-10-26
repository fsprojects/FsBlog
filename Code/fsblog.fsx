(**
# FsBlog Script

This script is the main workhorse of FsBlog that just coordinates the commands
and tasks that operate with the static site generation.
*)

#I @"packages/FAKE/tools/"
#I @"packages/FSharp.Configuration/lib/net45"
#I @"packages/RazorEngine/lib/net45"
#I @"packages/Suave/lib/net40"
#I @"bin/FsBlogLib"

#r "FakeLib.dll"
#r "RazorEngine.dll"
#r "FsBlogLib.dll"
#r "FSharp.Configuration.dll"
#r "Suave.dll"

open Fake
open Fake.Git
open System
open System.IO
open System.Net
open System.Diagnostics
open System.Threading
open FsBlogLib
open FSharp.Configuration
open Suave
open Suave.Web
open Suave.Files
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open Suave.Operators


// --------------------------------------------------------------------------------------
// Configuration.
// --------------------------------------------------------------------------------------
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
type Config = YamlConfig<"config/config.yml">

let config = new Config()
let title = config.title
let subtitle = config.subtitle
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

let special =
    [ source ++ "index.cshtml"
      source ++ "blog" ++ "index.cshtml" ]
let rsscount = 20

// --------------------------------------------------------------------------------------
// Regenerates the site
// --------------------------------------------------------------------------------------

type RoutingMode =
    | Production
    | Preview

let buildSite routing updateTagArchive =

    let root =
        match routing with
        | Production -> config.url.AbsoluteUri
        | Preview -> "http://localhost:8080"

    let dependencies = [ yield! Directory.GetFiles(layouts) ]
    let noModel = { Root = root; SiteTitle = title; SiteSubtitle = subtitle; MonthlyPosts = [||]; Posts = [||]; TaglyPosts = [||]; GenerateAll = true }
    let razor = new Razor(layouts, Model = noModel)
    let model =  Blog.LoadModel(tagRenames, Blog.TransformAsTemp (template, source) razor, root, blog, title, subtitle)

    // Generate RSS feed
    Blog.GenerateRss root title description model rsscount (output ++ "rss.xml")

    let uk = System.Globalization.CultureInfo.GetCultureInfo("en-GB")
    Blog.GeneratePostListing
        layouts template blogIndex model model.MonthlyPosts
        (fun (y, m, _) -> output ++ "blog" ++ "archive" ++ (m.ToLower() + "-" + (string y)) ++ "index.html")
        (fun (y, m, _) -> y = DateTime.Now.Year && m = uk.DateTimeFormat.GetMonthName(DateTime.Now.Month))
        (fun (y, m, _) -> sprintf "%d %s" y m)
        (fun (_, _, p) -> p)

    if updateTagArchive then
        Blog.GeneratePostListing
            layouts template blogIndex model model.TaglyPosts
            (fun (_, u, _) -> output ++ "blog" ++ "tag" ++ u ++ "index.html")
            (fun (_, _, _) -> true)
            (fun (t, _, _) -> t)
            (fun (_, _, p) -> p)

    let filesToProcess =
        FileHelpers.GetSourceFiles source output
        |> FileHelpers.SkipExcludedFiles exclude
        |> FileHelpers.TransformOutputFiles output source
        |> FileHelpers.FilterChangedFiles dependencies special

    let razor = new Razor(layouts, Model = model)
    for current, target in filesToProcess do
        FileHelpers.EnsureDirectory(Path.GetDirectoryName(target))
        printfn "Processing file: %s" (current.Substring(source.Length))
        Blog.TransformFile template true razor None current target

    FileHelpers.CopyFiles content output

// --------------------------------------------------------------------------------------
// Webserver stuff
// --------------------------------------------------------------------------------------

let refreshEvent = new Event<_>()

let handleWatcherEvents (events:FileChange seq) =
    for e in events do
        let fi = fileInfo e.FullPath
        traceImportant <| sprintf "%s was changed." fi.Name
        match fi.Attributes.HasFlag FileAttributes.Hidden || fi.Attributes.HasFlag FileAttributes.Directory with
        | true -> ()
        | _ ->  buildSite Preview false // TODO optimize based on which file has changed
    refreshEvent.Trigger()

let socketHandler (webSocket : WebSocket) =
  fun cx -> socket {
    while true do
      let! refreshed =
        Control.Async.AwaitEvent(refreshEvent.Publish)
        |> Suave.Sockets.SocketOp.ofAsync
      do! webSocket.send Text (new ByteSegment(System.Text.Encoding.UTF8.GetBytes "refreshed")) true
  }

let startWebServer () =
    printfn "starting webserver: %s" (FullName output)
    let serverConfig =
        { defaultConfig with
           homeFolder = Some (FullName output)
           bindings = [HttpBinding.create HTTP IPAddress.Loopback 8080us]
        }
    let app =
      choose [
        Filters.path "/websocket" >=> handShake socketHandler
        Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
        >=> Writers.setHeader "Pragma" "no-cache"
        >=> Writers.setHeader "Expires" "0"
        >=> browseHome ]
    startWebServerAsync serverConfig app |> snd |> Async.Start
    Process.Start "http://localhost:8080/index.html" |> ignore

// --------------------------------------------------------------------------------------
// Static site tooling as a set of targets.
// --------------------------------------------------------------------------------------

/// Regenerates the entire static website from source files (markdown and fsx).
Target "Generate" (fun _ ->
    buildSite Production true
)

Target "Preview" (fun _ ->

    buildSite Preview true

    use watcherDynamic = !! (source + "**/*.*") |> WatchChanges (fun changes ->
        printfn "Dynamic: %A" changes
        handleWatcherEvents changes
    )

    use watcherStatic = !! (content + "/**/*.*") |> WatchChanges (fun changes ->
        printfn "Static: %A" changes
        handleWatcherEvents changes
    )

    startWebServer ()

    traceImportant "Press Ctrl+C to stop!"
    // wat!?
    Thread.Sleep(-1)
)

Target "New" (fun _ ->
    let post, fsx, page =
        getBuildParam "post",
        getBuildParam "fsx",
        getBuildParam "page"

    match page, post, fsx with
    | "", "", "" -> traceError "Please specify either a new 'page', 'post' or 'fsx'."
    | _, "", ""  -> BlogPosts.CreateMarkdownPage source page
    | "", _, ""  -> BlogPosts.CreateMarkdownPost blog post
    | "", "", _  -> BlogPosts.CreateFsxPost blog fsx
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
    let cmd = sprintf """commit -a -m "Update generated web site (%s)" """ (DateTime.Now.ToString("dd MMMM yyyy"))
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

"Clean" =?>
("Install", hasBuildParam "theme") ==>
"Generate"

"Clean" =?>
("Install", hasBuildParam "theme") ==>
"Preview"

"Generate" ==> "GitClone" ==> "GitPublish"

// --------------------------------------------------------------------------------------
// Run a specified target.
// --------------------------------------------------------------------------------------
RunTargetOrDefault "Preview"
