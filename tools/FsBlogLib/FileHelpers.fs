namespace FsBlogLib
open System.IO

// --------------------------------------------------------------------------------------
// Various file & directory helpers
// --------------------------------------------------------------------------------------
module FileHelpers =

  /// Concantenate path using the right separator
  let (++) p1 p2 = Path.Combine(p1, p2)

  /// Delete directory if it exists
  let SafeDeleteDir (directory:string) recurse =     
    if Directory.Exists(directory) then 
      Directory.Delete(directory, recurse)
    

  /// Ensure that a given directory exists
  let rec EnsureDirectory directory = 
    if Directory.Exists(directory) |> not then 
      EnsureDirectory (Path.GetDirectoryName(directory))
      Directory.CreateDirectory(directory) |> ignore

  /// Copy files recursively and ensure all directories are created
  /// (overwrites older files)
  let rec CopyFiles source target = 
    EnsureDirectory target
    for dir in Directory.GetDirectories(source) do
      CopyFiles dir (target ++ Path.GetFileName(dir))
    for file in Directory.GetFiles(source) do
      let fullTarget = target ++ Path.GetFileName(file)
      if not (File.Exists(fullTarget)) || 
         File.GetLastWriteTime(file) > File.GetLastWriteTime(fullTarget) then
        printfn "Copying: %s" file
        File.Copy(file, fullTarget, true)

  /// Returns a file name in the TEMP folder and deletes it when disposed
  type DisposableFile(file, deletes) =
    static member Create(file) =
      new DisposableFile(file, [file])
    static member CreateTemp(?extension) = 
      let temp = Path.GetTempFileName()
      let file = match extension with Some ext -> temp + ext | _ -> temp
      new DisposableFile(file, [temp; file])
    member x.FileName = file
    interface System.IDisposable with
      member x.Dispose() = 
        for delete in deletes do
          if File.Exists(delete) then File.Delete(delete)


  /// Get all *.cshtml, *.html, *.md and *.fsx files in the source directory
  /// and return tuple containing the source file and required target file
  let GetSourceFiles source output = seq {
    let exts = set [ ".md"; ".fsx"; ".cshtml"; ".html" ]
    let rec getFiles source = seq {
      yield! Directory.GetFiles(source)
      for dir in Directory.GetDirectories(source) do
        yield! getFiles dir }
    for file in getFiles source do
      if exts |> Set.contains (Path.GetExtension(file).ToLower()) then
        let relativeFile = file.Remove(0, source.Length + 1)
        let relativeFolder = 
          let idx = relativeFile.LastIndexOf('.') 
          relativeFile.Substring(0, idx)
        let output = output ++ relativeFolder
        yield file, output }


  /// Given a sequence of source - output files, return only those where the
  /// source has changed since the output was generated. If any of the dependencies
  /// is newer than the output, then a file is also returned.
  let FilterChangedFiles dependencies special files = seq {
    let newestDependency = dependencies |> List.map Directory.GetLastWriteTime |> List.max
    let special = set special
    for source, output in files do 
      let outputWrite = Directory.GetLastWriteTime(output)
      if Set.contains source special || 
         outputWrite < Directory.GetLastWriteTime(source) || 
         outputWrite < newestDependency then
        yield source, output }


  /// Skip all files whose name starts with any string in the exclusion list
  let SkipExcludedFiles exclusions (files:seq<string * string>) = seq {
    for file, output in files do
      let fileNorm = System.Uri(file).LocalPath.ToLower()
      let excl = exclusions |> Seq.exists (fun (excl:string) -> 
        let excl = System.Uri(excl).LocalPath.ToLower()
        fileNorm.StartsWith(excl))
      if not excl then yield file, output }

  /// If the output file is in some subdirectory, then generate /blog/foo.index.html
  /// For files in the root, generate just /index.html (etc.)
  let TransformOutputFiles (output:string) files = seq {
    for file, (target:string) in files ->
      let relativeOut = target.Substring(output.Length + 1)
      // If it is not index & it is not in the root directory, then make it a sub-dir
      if not (relativeOut.EndsWith("index")) && (relativeOut.Contains("\\") || relativeOut.Contains("/"))
      then file, output ++ relativeOut ++ "index.html"
      else file, output ++ (relativeOut + ".html") }
