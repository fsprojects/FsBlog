namespace FSharp.Http

open System.IO
open System.Net
open System.Threading
open System.Text

// ----------------------------------------------------------------------------
// Simple web server hosting static files
// ----------------------------------------------------------------------------

[<AutoOpen>]
module HttpExtensions = 

  type System.Net.HttpListener with
    member x.AsyncGetContext() = 
      Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)

  type System.Net.HttpListenerRequest with
    member request.InputString =
      use sr = new StreamReader(request.InputStream)
      sr.ReadToEnd()

  type System.Net.HttpListenerResponse with
    member response.Reply(s:string) = 
      let buffer = Encoding.UTF8.GetBytes(s)
      response.ContentLength64 <- int64 buffer.Length
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()
    member response.Reply(typ, s:string) = 
      let buffer = Encoding.UTF8.GetBytes(s)
      response.ContentType <- typ
      response.ContentLength64 <- int64 buffer.Length
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()
    member response.Reply(typ, buffer:byte[]) = 
      response.ContentLength64 <- int64 buffer.Length
      response.ContentType <- typ
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()

/// Simple HTTP server
type HttpServer private (url, root) =

  let mutable replacements : (string * string) list = []
  let contentTypes = 
    dict [ ".css", "text/css"; ".html", "text/html"; ".js", "text/javascript";
           ".png", "image/png"; ".jpg", "image/jpg"; ".gif", "image/gif"; 
           ".woff", "application/font-woff" ; ".eot", "application/vnd.ms-fontobject";
           ".svg", "image/svg+xml"; ".svg", "image/svg+xml"; ".ttf", "application/x-font-ttf"]
  let tokenSource = new CancellationTokenSource()
  let agent = MailboxProcessor<HttpListenerContext>.Start((fun inbox -> async { 
    while true do
      let! context = inbox.Receive()
      try
        let s = context.Request.Url.LocalPath 

        // Handle an ordinary file request
        let file = root + (if s.EndsWith("/") then s + "index.html" else s)
        if File.Exists(file) then 
          let ext = Path.GetExtension(file).ToLower()
          let typ = contentTypes.[ext]
          if typ.StartsWith("text") then 
            let text = File.ReadAllText(file)
            let text = (text, replacements) ||> List.fold (fun s (o, t) -> s.Replace(o, t))
            context.Response.Reply(typ, text)
          else
            context.Response.Reply(typ, File.ReadAllBytes(file))
        else 
          context.Response.Reply(sprintf "File not found: %s" file) 
      with e ->
        context.Response.Reply(sprintf "Error processing request: %A" e) }), tokenSource.Token)

  let server = async { 
    use listener = new HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()
    while true do 
      let! context = listener.AsyncGetContext()
      agent.Post(context) }

  do Async.Start(server, cancellationToken = tokenSource.Token)

  member x.Replacements with get() = replacements and set(v) = replacements <- v

  /// Stops the HTTP server and releases the TCP connection
  member x.Stop() = tokenSource.Cancel()

  /// Starts new HTTP server on the specified URL. The specified
  /// function represents computation running inside the agent.
  static member Start(url, root) = 
    new HttpServer(url, root)
