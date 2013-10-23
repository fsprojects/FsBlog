#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open System

// --------------------------------------------------------------------------------------
// Important variables.
// --------------------------------------------------------------------------------------
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

// --------------------------------------------------------------------------------------
// Restore all missing NuGet packages.
// --------------------------------------------------------------------------------------
Target "RestorePackages" (fun _ ->
    !! "./**/packages.config" 
    |> Seq.iter (RestorePackage (fun p -> { p with ToolPath = "./.nuget/nuget.exe" }))
)

// --------------------------------------------------------------------------------------
// Static site tooling as a set of targets.
// --------------------------------------------------------------------------------------
Target "Generate" DoNothing

Target "Preview" DoNothing

Target "Deploy" DoNothing

Target "Commit" DoNothing

"RestorePackages"
    ==> "Generate" 
    ==> "Preview"

// --------------------------------------------------------------------------------------
// Build the tools as a separate set of targets.
// --------------------------------------------------------------------------------------
Target "Clean" DoNothing

Target "Build" DoNothing

"RestorePackages"
    ==> "Clean"
    ==> "Build"    

// --------------------------------------------------------------------------------------
// Run a specified target.
// --------------------------------------------------------------------------------------
RunTargetOrDefault "GenerateAndPreview"