#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open System

// --------------------------------------------------------------------------------------
// Static site tooling as a set of targets.
// --------------------------------------------------------------------------------------
Target "Generate" DoNothing

Target "Preview" DoNothing

Target "GenerateAndPreview" DoNothing

Target "Deploy" DoNothing

"Generate" ==> "GenerateAndPreview"

// --------------------------------------------------------------------------------------
// Build the tools as a separate set of targets.
// --------------------------------------------------------------------------------------
Target "Clean" DoNothing

Target "Build" DoNothing

// --------------------------------------------------------------------------------------
// Run a specified target
// --------------------------------------------------------------------------------------
RunTargetOrDefault "GenerateAndPreview"