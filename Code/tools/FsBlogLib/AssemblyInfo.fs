namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FsBlogLib")>]
[<assembly: AssemblyProductAttribute("FsBlogLib")>]
[<assembly: AssemblyDescriptionAttribute("Blog aware, static site generation using F#.")>]
[<assembly: AssemblyVersionAttribute("0.4.1")>]
[<assembly: AssemblyFileVersionAttribute("0.4.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.1"
