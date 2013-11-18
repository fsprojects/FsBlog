@{
    Layout = "post";
    Title = "Hello, FsBlog!";
    Date = "2013-11-08T11:27:31";
    Tags = "";
    Description = "Introducing FsBlog - a blog-aware static site generator using F#.";
}

**FsBlog** aims to be a blog-aware static site generator, mostly built in `F#` for .NET and Mono developers. 

<!--more-->

But don't worry, you won't even need to know any F# to get up and running. So long as you are comfortable using a command line or terminal, and have a degree of familiarity with Markdown and Razor syntax - you're already most of the way there!

Inpired by the likes of **Jekyll** and **Octopress** - and greatly helped along the way by the scripts behind [@@tomaspetricek](https://twitter.com/tomaspetricek)'s personal website; FsBlog gives the average .NET or Mono dev the tools that they need to get a statically built website up and running quickly and hosted somewhere like [Github Pages](http://pages.github.com/).

This set of tools have been pulled together using some of the following community projects:

* [FAKE](http://fsharp.github.io/FAKE/) for automation and scripting of the different tasks.
* [F# Formatting](http://tpetricek.github.io/FSharp.Formatting/) for Markdown and F# literate programming, processing and colorization.
* [RazorEngine](https://github.com/Antaris/RazorEngine) which is used for the templating and embedded `C#` code.
* Some of the code in **FsBlogLib** that calls the **RazorEngine** has been based on F# code in [Tilde](https://github.com/aktowns/tilde).
* [Github Pages](http://pages.github.com/) for our default theme.

## Development

It is very early days and we haven't yet reached anything vaguely resembling a version 1.0 of the software.

You can track the progress of the development, outstanding issues and check what we're working towards by checking out the [Issues and Milestones](https://github.com/saxonmatt/FsBlog/issues/milestones) section of our repository.

If you find a bug, or have a request or whatever - please raise a new issue there! or even better make the change and open a pull request.