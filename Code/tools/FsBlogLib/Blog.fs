namespace FsBlogLib

open System
open System.IO
open FsBlogLib.BlogPosts
open FsBlogLib.FileHelpers
open System.Xml.Linq
open FSharp.Literate

// --------------------------------------------------------------------------------------
// Blog - the main blog functionality
// --------------------------------------------------------------------------------------

/// Represents the model that is passed to all pages
type Model =
  { Posts : BlogHeader[]
    MonthlyPosts : (int * string * seq<BlogHeader>)[]
    TaglyPosts : (string * string * seq<BlogHeader>)[]
    GenerateAll : bool
    Root : string
    SiteTitle : string
    SiteSubtitle : string }

module Blog =

  /// Walks over all blog post files and loads model (caches abstracts along the way)
  let LoadModel(tagRenames, transformer, (root:string), blog, title, subtitle) =
    let urlFriendly (s:string) = s.Replace("#", "sharp").Replace(" ", "-").Replace(".", "dot")
    let posts = LoadBlogPosts tagRenames transformer blog
    let uk = System.Globalization.CultureInfo.GetCultureInfo("en-GB")
    { Posts = posts
      GenerateAll = false
      TaglyPosts =
        query { for p in posts do
                for t in p.Tags do
                select t into t
                distinct
                let posts = posts |> Seq.filter (fun p -> p.Tags |> Seq.exists ((=) t))
                let recent = posts |> Seq.filter (fun p -> p.Date > (DateTime.Now.AddYears(-1))) |> Seq.length
                where (recent > 0)
                sortByDescending (recent * (Seq.length posts))
                select (t, urlFriendly t, posts) }
        |> Array.ofSeq
      MonthlyPosts =
        query { for p in posts do
                groupBy (p.Date.Year, p.Date.Month) into g
                let year, month = g.Key
                sortByDescending (year, month)
                select (year, uk.DateTimeFormat.GetMonthName(month), g :> seq<_>) }
        |> Array.ofSeq
      Root = root.Replace('\\', '/')
      SiteTitle = title
      SiteSubtitle = subtitle }

  let TransformFile template hasHeader (razor:FsBlogLib.Razor) prefix current target =
    let html =
      match Path.GetExtension(current).ToLower() with
      | (".fsx" | ".md") as ext ->
          let header, content =
            if not hasHeader then "", File.ReadAllText(current)
            else RemoveScriptHeader ext current
          use fsx = DisposableFile.Create(current.Replace(ext, "_" + ext))
          use html = DisposableFile.CreateTemp(".html")
          File.WriteAllText(fsx.FileName, content |> RemoveScriptAbstractMarker)
          if ext = ".fsx" then
              Literate.ProcessScriptFile(fsx.FileName, template, html.FileName, ?prefix=prefix)
          else
              Literate.ProcessMarkdown(fsx.FileName, template, html.FileName, ?prefix=prefix,
                                       customizeDocument=BlogCustomizations.CustomizeDocument)
          let processed = File.ReadAllText(html.FileName)
          File.WriteAllText(html.FileName, header + processed)
          EnsureDirectory(Path.GetDirectoryName(target))
          razor.ProcessFile(html.FileName)
      | ".html" | ".cshtml" ->
          razor.ProcessFile(current)
      | _ -> failwith "Not supported file!"
    File.WriteAllText(target, html)

  let TransformAsTemp (template, source:string) razor prefix current =
    let cached = (Path.GetDirectoryName(current) ++ "cached" ++ Path.GetFileName(current))
    if File.Exists(cached) &&
      (File.GetLastWriteTime(cached) > File.GetLastWriteTime(current)) then
      File.ReadAllText(cached)
    else
      printfn "Processing abstract: %s" (current.Substring(source.Length + 1))
      EnsureDirectory(Path.GetDirectoryName(current) ++ "cached")
      TransformFile template false razor (Some prefix) current cached
      File.ReadAllText(cached)

  let GenerateRss root title description model take target =
    let count = Seq.length model.Posts
    let (!) name = XName.Get(name)
    let items =
      [| for item in model.Posts |> Seq.take (if count < take then count else take) -> 
           XElement
            ( !"item",
              XElement(!"title", item.Title),
              XElement(!"guid", root + "/blog/" + item.Url),
              XElement(!"link", root + "/blog/" + item.Url + "/index.html"),
              XElement(!"pubDate", item.Date.ToUniversalTime().ToString("r")),
              XElement(!"description", item.Abstract) ) |]
    let channel =
      XElement
        ( !"channel",
          XElement(!"title", (title:string)),
          XElement(!"link", (root:string)),
          XElement(!"description", (description:string)),
          items )
    let doc = XDocument(XElement(!"rss", XAttribute(!"version", "2.0"), channel))
    EnsureDirectory(Path.GetDirectoryName(target))
    File.WriteAllText(target, doc.ToString())

  let GeneratePostListing layouts template blogIndex model posts urlFunc needsUpdate infoFunc getPosts =
    for item in posts do
      let model = { model with GenerateAll = true; Posts = Array.ofSeq (getPosts item) }
      let razor = FsBlogLib.Razor(layouts, Model = model)
      let target = urlFunc item
      EnsureDirectory(Path.GetDirectoryName(target))
      if not (File.Exists(target)) || needsUpdate item then
        printfn "Generating archive: %s" (infoFunc item)
        TransformFile template true razor None blogIndex target
