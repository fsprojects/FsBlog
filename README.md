# FsBlog

**Progress:** See the Issues and Milestones pages of this repo.

**FsBlog** aims to be a blog-aware static site generator, mostly built in `F#`. But don't worry, you won't even need to know any F# to get up and running. So long as you are comfortable using a command line or terminal, and have a degree of familiarity with Markdown and Razor syntax - you're good to go!

Inspired by the likes of **Jekyll** and **Octopress** - and greatly helped along the way by the scripts behind [@tomaspetricek](https://twitter.com/tomaspetricek)'s [personal website](https://github.com/tpetricek/TomaspNet.Website); FsBlog gives the average F# and .NET hacker the tools they need to get a statically built website up and running quickly and hosted somewhere like [GitHub Pages](http://pages.github.com/).

This set of tools have been pulled together using some of the following community projects:

* [FAKE](http://fsharp.github.io/FAKE/) for the automation and scripting of the different tasks.
* [F# Formatting](http://tpetricek.github.io/FSharp.Formatting/) which deals with the Markdown and F# processing/colorization.
* [RazorEngine](https://github.com/Antaris/RazorEngine) which is used for the templating and embedded C# code.
* Some of the code that calls *RazorEngine* from F# is based on [Tilde](https://github.com/aktowns/tilde).
* [Bootstrap 3](http://getbootstrap.com/).

## Getting started

You can just grab the **NuGet** package and jump straight on to **Usage** below.

Alternatively you can fork or clone the [FsBlog repo](https://github.com/saxonmatt/FsBlog), then run:

    build

This will build the tools required for usage below.

## Usage

    fake generate

Regenerates the entire static website from source files (Markdown and `.fsx` files).

    fake preview

Generates the site and then starts up a local web server and launches the output content from the last time it was generated.

    fake

But `preview` is also the *default* behaviour when you run the `fake` command without any arguments - so that makes it even easier.

	fake new page="about fsblog"

Will create a new *blank* Markdown file in your blog's source files ready for you to author a non-post page of content.

    fake new post="hello markdown"

Will create a new *blank* Markdown file in your blog's source files ready for you to hack away at your next blog post.

	fake new fsx="hello fsharp"

Will create a new *blank* `.fsx` file in your blog's source files ready for you to hack away at your next code-heavy blog post. To find out more about using `.fsx` files as the source of your posts, check out [F# Formatting: Literate programming](http://tpetricek.github.io/FSharp.Formatting/demo.html).

## Remarks

Note: the documentation above is currently little more than a specification for the command line tooling. It is currently subject to change.

This project is at a very early stage - so bare with me.
