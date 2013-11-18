@{
    Layout = "post";
    Title = "Getting Started";
    Date = "2013-11-09T12:38:14";
    Tags = "";
    Description = "";
}

If you've much experience with other static site generators like *Octopress* or *Jekyll* most of this should be fairly familiar, if not, hopefully it won't be too difficult to pick up.

<!--more-->

## Command line

FsBlog tends to rely on the fact that you're using the command line or terminal - almost all interaction with FsBlog and git are documented as such.

## Dependencies

Although X-Platform is a goal for the [not-too-distant-future](https://github.com/saxonmatt/FsBlog/issues?milestone=2&state=open) - at the moment you'll need:

* .NET 4.5
* F# 3.0
* VS2012 or VS2013
* A git client

If you follow the instructions for [using F# on Windows](http://fsharp.org/use/windows/) you'll probably be good to go.

## 1. Setup FsBlog

First you'll need to clone the repo. On your command line, using git:

    git clone git://github.com/saxonmatt/FsBlog.git FsBlog
    cd FsBlog

Next, run the build:

    build

This should install a few [NuGet](http://www.nuget.org/) dependencies that FsBlog makes use of.

## 2. Configuration

Using your favourite F# code editor, edit the following section of the `./fsblog.fsx` file in your repo:

    // --------------------------------------------------------------------------------------
    // Configuration.
    // --------------------------------------------------------------------------------------
    Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
    let root = "http://saxonmatt.co.uk/FsBlog"
    let title = "FsBlog - F# static site generation"
    let description = """
        FsBlog aims to be a blog-aware static site generator, mostly built in F#. But don't worry, 
        you won't even need to know any F# to get up and running. So long as you are comfortable 
        using a command line or terminal, and have a degree of familiarity with Markdown and Razor 
        syntax - you're good to go!"""

There isn't a whole lot to change right now, but at some point you'll be able to configure a Twitter handle, GitHub account etc.

## 3. Create a post

Again on your command line:

    fake new post="post title"

This will create a new file in the following location, ready for you to edit: `./source/blog/yyyy/MM-dd-title.md`.

## 4. Generate and preview

You can generate your website at any time from the command line using the following command:

    fake generate

If you also want to preview your website, you can run the following command:

    fake preview

The `preview` command will also `generate` the website too.