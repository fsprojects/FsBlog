namespace FsBlogLib

open System
open System.IO
open System.Dynamic
open System.Collections
open System.Collections.Generic

open RazorEngine
open RazorEngine.Templating

type MetadataDict() =
    let dictionary = new Dictionary<string, obj>()

    interface IDictionary<string, obj> with
        member x.Add(key: string, value : obj) =
            dictionary.Add(key.ToUpper(), value)
        member x.Add(kvp) =
            dictionary.Add(kvp.Key, kvp.Value)
        member x.ContainsKey(key) =
            dictionary.ContainsKey(key)
        member x.Contains(kvp) =
            dictionary.TryGetValue(kvp.Key, ref kvp.Value)
        member x.Item
            with get key =
                if dictionary.ContainsKey(key) then
                    dictionary.[key]
                else null
            and set key value = dictionary.Item(key) <- value
        member x.Count
            with get() = dictionary.Count
        member x.IsReadOnly
            with get() = false
        member x.Keys =
            dictionary.Keys :> ICollection<string>
        member x.Remove key =
            dictionary.Remove(key)
        member x.Remove(kvp : KeyValuePair<string,obj>) =
            dictionary.Remove(kvp.Key)
        member x.TryGetValue(key, value) =
            dictionary.TryGetValue(key, ref value)
        member x.Values =
            dictionary.Values :> ICollection<obj>
        member x.Clear() =
            dictionary.Clear()
        member x.CopyTo(array, arrayIndex) =
            (dictionary :> IDictionary<string, obj>).CopyTo(array, arrayIndex)
        member x.GetEnumerator() =
            dictionary.GetEnumerator() :> IEnumerator
        member x.GetEnumerator() =
            dictionary.GetEnumerator() :> IEnumerator<KeyValuePair<string,obj>>

[<AbstractClass>]
type TemplateBaseExtensions<'T>() =
    inherit TemplateBase<'T>()

    member private x.tryGetViewBagValue<'C> key =
        let vb = x.ViewBag :?> DynamicViewBag
        let memBinder =
            {
                new GetMemberBinder(key, false) with
                    member x.FallbackGetMember(y,z) = failwith "not implemented"
            }
        let mutable output = ref (new Object ())
        let result = vb.TryGetMember(memBinder, output)
        if result && !output <> null then Some(!output :?> 'C) else None

    member private x.trySetViewBagValue<'C> key (value:'C) =
        let vb = x.ViewBag :?> DynamicViewBag
        let memBinder =
            {
                new DeleteMemberBinder(key, false) with
                    member x.FallbackDeleteMember(y,z) = failwith "not implemented"
            }
        let names =
            vb.GetDynamicMemberNames()
            |> Seq.tryFind(fun x -> x = key)
        match names with
        | Some(v) ->
            vb.TryDeleteMember(memBinder) |> ignore
            vb.AddValue(key, value)
        | _ -> vb.AddValue(key, value)

    member x.Title
        with get() = defaultArg (x.tryGetViewBagValue<string> "Title") ""
        and set value = x.trySetViewBagValue<string> "Title" value
    member x.Description
        with get() = defaultArg (x.tryGetViewBagValue<string> "Description") ""
        and set value = x.trySetViewBagValue<string> "Description" value
    member x.Tags
        with get() = defaultArg (x.tryGetViewBagValue<string> "Tags") ""
        and set value = x.trySetViewBagValue<string> "Tags" value
    member x.Date
        with get() = defaultArg (x.tryGetViewBagValue<string> "Date") ""
        and set value = x.trySetViewBagValue<string> "Date" value
    member x.Url
        with get() = defaultArg (x.tryGetViewBagValue<string> "Url") ""
        and set value = x.trySetViewBagValue<string> "Url" value
    member x.Meta
        with get() : IDictionary<string, obj> =
            let res = x.tryGetViewBagValue<IDictionary<string, obj>> "Meta"
            match res with
            | Some(value) -> value
            | _ ->
                let newcol = (new MetadataDict()) :> IDictionary<string, obj>
                newcol.["generator"] <- "tilde"
                x.trySetViewBagValue<IDictionary<string, obj>> "Meta" newcol
                newcol
        and set value = x.trySetViewBagValue<IDictionary<string, obj>> "Meta" value


    member x.RenderPart(templateName) = x.RenderPart(templateName, None)
    member x.RenderPart(templateName, ?model) =
        printfn "    Resolving(partial): %s" templateName
        let filecontents =
            if File.Exists ("_includes"+System.IO.Path.DirectorySeparatorChar.ToString() + templateName) then "_includes"+System.IO.Path.DirectorySeparatorChar.ToString() + templateName
            elif File.Exists ("_includes"+System.IO.Path.DirectorySeparatorChar.ToString() + templateName + ".cshtml") then "_includes"+System.IO.Path.DirectorySeparatorChar.ToString() + templateName + ".cshtml"
            else failwithf "Failed to find partial template: %s" templateName

        match model with
        | Some(model) -> Engine.Razor.RunCompile(File.ReadAllText(filecontents), templateName, model.GetType(), model)
        | _ -> Engine.Razor.RunCompile(File.ReadAllText(filecontents), templateName)