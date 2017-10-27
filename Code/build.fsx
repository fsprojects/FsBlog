(**
# Build Script

This script is intended for use with [FAKE][fake] for the build process of the
**FsBlog** tools themselves.

 [fake]: http://fsharp.github.io/FAKE/
*)

#I "packages/FAKE/tools/"
#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing
open Fake.Testing.NUnit3
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System


// --------------------------------------------------------------------------------------
// Important variables.
// --------------------------------------------------------------------------------------
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")

// Information about the project to be used
//  - by NuGet
//  - in AssemblyInfo files
//  - in FAKE tasks
let solution  = "FsBlog"
let project   = "FsBlogLib"
let authors   = [ "matt ball"; "tomas petricek"; ]
let summary   = "Blog aware, static site generation using F#."
let description = """
  FsBlog aims to be a blog-aware static site generator, mostly built in F#. But don't worry,
  you won't even need to know any F# to get up and running. So long as you are comfortable
  using a command line or terminal, and have a degree of familiarity with Markdown and Razor
  syntax - you're good to go!"""

// Pattern specifying assemblies to be tested using xunit
let testAssemblies = "bin/FsBlogLib/*Tests*.dll"

let tags = "F# fsharp blog website generation"

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->

  let fileName = "tools/" + project + "/AssemblyInfo.fs"
  CreateFSharpAssemblyInfo fileName
      [ Attribute.Title project
        Attribute.Product project
        Attribute.Description summary
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion ]
)


// --------------------------------------------------------------------------------------
// Tasks for running the build of tools.
// --------------------------------------------------------------------------------------

Target "Clean" (fun _ ->
    CleanDirs ["bin"]
)

Target "Build" (fun _ ->
    { BaseDirectory = __SOURCE_DIRECTORY__
      Includes = [ solution +       ".sln" ]
      Excludes = [] }
    |> MSBuildRelease "bin/FsBlogLib" "Rebuild"
    |> ignore
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "RunTests" (fun _ ->
    !! testAssemblies
    |> NUnit3 (fun p ->
        { p with
            ShadowCopy = false
            TimeOut = TimeSpan.FromMinutes 20.
            })
)

// --------------------------------------------------------------------------------------
// Build dependencies.
// --------------------------------------------------------------------------------------
"Clean"
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "RunTests"

RunTargetOrDefault "Build"
