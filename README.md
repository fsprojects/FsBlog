[![Issue Stats](http://issuestats.com/github/fsprojects/FsBlog/badge/issue)](http://issuestats.com/github/fsprojects/FsBlog)
[![Issue Stats](http://issuestats.com/github/fsprojects/FsBlog/badge/pr)](http://issuestats.com/github/fsprojects/FsBlog)

# FsBlog [![NuGet Status](http://img.shields.io/nuget/v/FsBlog.svg?style=flat)](https://www.nuget.org/packages/FsBlog/)

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

Alternatively you can fork or clone the [FsBlog repo](https://github.com/fsprojects/FsBlog), then run:

    build

1. On Windows, use the *32-bit* Visual Studio Command Prompt, the 64-bit
   solution is not defined.

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
    
    fake clean

Will remove all files from the output folder

## Theming

FsBlog supports theming. New theme can be installed by running:

    fake install theme="theme-name"

Once the new theme is installed, FsBlog will use the theme to generate the entire site. Themes are located in `themes` folder. Please see the `default` theme for the sample.

## Config.yml

The `config.yml` is a [YAML](http://yaml.org/) file that contains configuration settings used by FsBlog. You can specify the following settings:
* `source` : a root folder that contains main razor (index.cshtml) and Markdown (*.md) files to be processed when `fake generate` is run.
* `blog` : a folder that contains razor (*.cshtml) and Markdown (*.md) files to be processed as blog posts.
* `content` : a folder contains files for an active theme.
* `output` : a folder contains generated static website.
* `themes` : a folder contains FsBlog themes.

## Remarks

Note: the documentation above is currently little more than a specification for the command line tooling. It is currently subject to change.

This project is at a very early stage - so bare with me.


## Maintainer(s)

- [@MattDrivenDev](https://github.com/MattDrivenDev)
- [@nelak](https://github.com/nelak)
- [@cdrnet](https://github.com/cdrnet)
- [@tpetricek](https://github.com/tpetricek)

The default maintainer account for projects under "fsprojects" is [@fsprojectsgit](https://github.com/fsprojectsgit) - F# Community Project Incubation Space (repo management)
