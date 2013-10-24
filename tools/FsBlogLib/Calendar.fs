namespace FsBlogLib

open Blog
open BlogPosts
open FileHelpers
open System
open System.IO
open System.Drawing
open System.Drawing.Imaging

// --------------------------------------------------------------------------------------
// Generating calendar
// --------------------------------------------------------------------------------------

module Calendar = 

  type CalendarIndexModel =
    { Root : string 
      Posts : BlogHeader[] 
      MonthlyPosts : (int * string * seq<BlogHeader>)[]
      // More things needed by calendar index
      Year : int
      Months : seq<seq<string>> }

  type CalendarMonthModel =
    { Root : string 
      Posts : BlogHeader[] 
      MonthlyPosts : (int * string * seq<BlogHeader>)[]
      // More things needed by calendar page
      Title : string
      Link : string
      Days : seq<bool * int> }

  // Get objects needed for JPEG encoding
  let JpegCodec = 
    ImageCodecInfo.GetImageEncoders() 
    |> Seq.find (fun c -> c.FormatID = ImageFormat.Jpeg.Guid)
  let JpegEncoder = Encoder.Quality
  let QualityParam = new EncoderParameters(Param = [| new EncoderParameter(JpegEncoder, 95L) |])

  /// Resize file so that both width & height are smaller than 'maxSize'
  let ResizeFile maxSize source (target:string) = 
    use bmp = Bitmap.FromFile(source)
    let scale = max ((float bmp.Width) / (float maxSize)) ((float bmp.Height) / (float maxSize))
    use nbmp = new Bitmap(int (float bmp.Width / scale), int (float bmp.Height / scale))
    ( use gr = Graphics.FromImage(nbmp)
      gr.DrawImage(bmp, 0, 0, nbmp.Width, nbmp.Height) )
    nbmp.Save(target, JpegCodec, QualityParam)

  let GenerateCalendar root layouts output dependencies calendar calendarMonth calendarIndex (model:Model) = 
    let newestDependency = dependencies |> List.map Directory.GetLastWriteTime |> List.max
    let uk = System.Globalization.CultureInfo.GetCultureInfo("en-GB")
    let table f = seq { for row in 0 .. 3 -> seq { for col in 0 .. 2 -> f (row * 3 + col + 1) }}

    // Generate index HTML files with links to individual month files
    for dir in Directory.GetDirectories(calendar) do
      let year = int (Path.GetFileNameWithoutExtension(dir))
      let target = output ++ "calendar" ++ (string year) ++ "index.html"
      if not (File.Exists(target)) || ( Directory.GetLastWriteTime(target) < newestDependency) then
        // Generate index for the current year, because it is missing
        printfn "Generating calendar index: %d" year
        let index = 
          { CalendarIndexModel.Root = root; Year = year;
            Posts = model.Posts; MonthlyPosts = model.MonthlyPosts;
            Months = table uk.DateTimeFormat.GetMonthName }
        let razor = FsBlogLib.Razor(layouts, Model = index)
        EnsureDirectory (Path.GetDirectoryName(target))
        TransformFile "" false razor None calendarIndex target
        
        // Generate individual calendar files
        for month in 1 .. 12 do
          let name = uk.DateTimeFormat.GetMonthName(month)
          let target = output ++ "calendar" ++ (string year) ++ (name.ToLower() + ".html")
          let days = seq { 
            for i in 1 .. uk.Calendar.GetDaysInMonth(year, month) ->
              uk.Calendar.GetDayOfWeek(DateTime(year, month, i)) = DayOfWeek.Sunday, i }                    
          let month = 
            { CalendarMonthModel.Root = root; Title = name + " " + (string year);
              Posts = model.Posts; MonthlyPosts = model.MonthlyPosts;
              Link = name.ToLower() + ".jpg"; Days = days }
          let razor = FsBlogLib.Razor(layouts, Model = month)
          EnsureDirectory (Path.GetDirectoryName(target))
          TransformFile "" false razor None calendarMonth target
        

    // Generate all calendar files (resize appropriately)
    for dir in Directory.GetDirectories(calendar) do
      let year = int (Path.GetFileNameWithoutExtension(dir))
      let yearDir = output ++ "calendar" ++ (string year)
      printfn "Checking calendar files for: %d" year
      for month in 1 .. 12 do 
        let monthName = uk.DateTimeFormat.GetMonthName(month).ToLower()
        let file = calendar ++ (string year) ++ (monthName + ".jpg")
        let source = if File.Exists(file) then file else calendar ++ "na.jpg"
        let writeFile size suffix = 
          let target = yearDir ++ (Path.GetFileNameWithoutExtension(file) + suffix + ".jpg")
          if not (File.Exists(target)) || 
            (File.GetLastWriteTime(target) < File.GetLastWriteTime(source)) ||
            (File.GetLastWriteTime(target) < File.GetLastAccessTime(source)) then 
            printfn "Resizing file for: %s" monthName
            ResizeFile size source target
        writeFile 2400 "-original"
        writeFile 700 ""
        writeFile 240 "-preview"


