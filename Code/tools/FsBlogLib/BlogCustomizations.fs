namespace FsBlogLib

open System
open System.Web
open FSharp.Markdown
open FSharp.Literate

module BlogCustomizations =

    let private youtubeFrame id =
        sprintf "<iframe id=\"ytplayer\" type=\"text/html\" width=\"640\" height=\"390\" src=\"http://www.youtube.com/embed/%s?autoplay=0\" frameborder=\"0\"/>"
            id

    let youtubeTransform (link: Uri) =
        let urlParams = HttpUtility.ParseQueryString(link.Query)
        if urlParams.AllKeys |> Seq.exists (fun x -> x = "v") then
            let newLink = Literal(youtubeFrame urlParams.["v"])
            Some newLink
        else
            None

    let singleLinkAnalysis (link: Uri) =
        match link with
        | _ when link.DnsSafeHost.EndsWith("youtube.com") ->
            youtubeTransform link
        | _ -> None

    let rec private matchSpan span =
        match span with
        | Strong (spans) ->
            let spans' = List.map matchSpan spans
            Strong(spans')
        | Emphasis (spans) ->
            let spans' = List.map matchSpan spans
            Emphasis (spans')
        | DirectLink ([Literal(linkText)], (link, label)) as orgLink
            when linkText = link ->
            let linkUri = new Uri(link)
            match singleLinkAnalysis linkUri with
            | Some x -> x
            | None -> orgLink
        | DirectLink (spans, (link, label)) ->
            let spans' = List.map matchSpan spans
            DirectLink (spans', (link, label))
        | IndirectLink (spans, link, label) ->
            let spans' = List.map matchSpan spans
            IndirectLink (spans', link, label)
        | other -> other

    let rec private matchParagraph paragraph =
        match paragraph with
        | Heading (i, spans) ->
            let spans = List.map matchSpan spans
            Heading(i, spans)
        | Paragraph spans ->
            let spans' = List.map matchSpan spans
            Paragraph(spans')
        | ListBlock (kind, paragraphsList) ->
            let paragraphsList' =
                paragraphsList |> List.map (fun paragraphs -> List.map matchParagraph paragraphs)
            ListBlock(kind, paragraphsList')
        | QuotedBlock paragraphs ->
            let paragraphs' = List.map matchParagraph paragraphs
            QuotedBlock(paragraphs')
        | Span spans ->
            let spans = List.map matchSpan spans
            Span (spans)
        | other -> other


    let CustomizeDocument (context: ProcessingContext) (doc: LiterateDocument) =
        let paragraphs' = List.map matchParagraph doc.Paragraphs
        doc.With(?paragraphs = Some paragraphs')



