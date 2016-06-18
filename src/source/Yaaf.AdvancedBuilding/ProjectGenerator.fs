namespace Yaaf.AdvancedBuilding
open System
open System.Xml.Linq

//[<StructuredFormatDisplay("{AsString}")>]
type GlobalProjectInfo = 
  { BuildTemplates : (string * string) list
    ProjectName : string
    Includes : ItemGroupItem list
    GlobalData : (string * obj) list

    }
    static member Empty =
      { BuildTemplates = []
        ProjectName = ""
        Includes = []
        GlobalData = [] }
    member x.DefaultTemplateData buildName =
      let buildTemplates = x.BuildTemplates |> dict
      { Includes = x.Includes
        TemplateName = if buildTemplates.ContainsKey buildName then buildTemplates.[buildName] else "unknown"
        ProjectName = x.ProjectName
        DefineConstants = []
        ProjectGuid = Guid.Empty
        AdditionalData = []
      }

type ProjectGeneratorConfig =
  { BuildFileList : (string * TemplateData) list
     }

open System
open System.IO

module ProjectGeneratorModule =
  let ifNone item opt =
    match opt with
    | Some i -> i
    | None -> item
  let getProjectFileName settingsFile (buildFilename:string) =
    let baseDir = Path.GetDirectoryName settingsFile
    let settingsFile =
        if Path.GetExtension(settingsFile) = ".fsx" then
            Path.Combine(baseDir, Path.GetFileNameWithoutExtension settingsFile)
        else settingsFile
    let addBaseDir baseDir file = Path.Combine(baseDir, file)
    let baseName = Path.GetFileNameWithoutExtension(settingsFile)
    let buildFileSplits = buildFilename.Split ([|'/'|])
    let buildFilename = buildFileSplits |> Seq.last
    let buildFileBaseDir =
        buildFileSplits
        |> Seq.take (buildFileSplits.Length - 1)
        |> Seq.fold (fun state item -> Path.Combine(state, item)) ""
    [ ".fsproj"; ".csproj"]
    |> Seq.tryPick (fun ext ->
      if baseName.EndsWith(ext) then
        if String.IsNullOrEmpty buildFilename then
            Some (sprintf "%s%s" (baseName.Substring(0, baseName.Length - ext.Length)) ext)
        else
            Some (sprintf "%s.%s%s" (baseName.Substring(0, baseName.Length - ext.Length)) buildFilename ext)
      else None)
    |> ifNone (
        if  String.IsNullOrEmpty(baseName) then buildFilename
        elif String.IsNullOrEmpty(Path.GetFileNameWithoutExtension buildFilename) then sprintf "%s%s" baseName buildFilename
        else sprintf "%s.%s" baseName buildFilename)
    |> addBaseDir buildFileBaseDir
    |> addBaseDir baseDir

  let fixInclude (relPath:string) item =
    let fixPath path =
        sprintf "%s\\%s" (relPath.Replace("/","\\")) path
    match item with
    | ItemGroupItem.Compile inc ->
        ItemGroupItem.Compile (fixPath inc)
    | ItemGroupItem.CompileLink (name, inc) ->
        ItemGroupItem.CompileLink (name, fixPath inc)
    | ItemGroupItem.Content inc ->
        ItemGroupItem.Content (fixPath inc)
    | ItemGroupItem.ContentLink (name, inc) ->
        ItemGroupItem.ContentLink (name, fixPath inc)
    | ItemGroupItem.NoneItem inc ->
        ItemGroupItem.NoneItem (fixPath inc)
    | ItemGroupItem.NoneItemLink (name, inc) ->
        ItemGroupItem.NoneItemLink (name, fixPath inc)
    | _ -> item

type MsBuildInfo =
  { Includes : ItemGroupItem list
    ProjectName : string
    ProjectGuid : Guid }
    member x.ContentIncludes = x.Includes |> List.filter (fun (item : ItemGroupItem) -> item.IsAnyItem)
    member x.AnyReferenceIncludes = x.Includes |> List.filter (fun (item : ItemGroupItem) -> item.IsAnyReference)
    member x.ReferenceIncludes = x.Includes |> List.filter (fun (item : ItemGroupItem) -> item.My_IsReference)
    member x.ProjectReferenceIncludes = x.Includes |> List.filter (fun (item : ItemGroupItem) -> item.My_IsProjectReference)

module MsBuildHelper =
  let private msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003"
  let private xname name = XName.Get(name, msbuildNamespace)

  let readItemGroupItemsFromDocument (doc : XDocument) =
    doc.Descendants(xname "Project").Descendants(xname "ItemGroup").Elements()
    |> Seq.choose (fun e ->
        let includeAttribute = e.Attribute(XName.Get "Include")
        if includeAttribute = null then failwithf "expected Include attribute on: %A" e
        let includeValue = includeAttribute.Value
        let getElemValue elemName =
            e.Elements(xname elemName) |> Seq.tryPick (fun elem -> Some elem.Value)
        let linkValue = getElemValue "Link"
        // see https://msdn.microsoft.com/en-us/library/dd393574.aspx (property, item and metadata is not case-sensitive)
        match e.Name.LocalName.ToLowerInvariant(), linkValue with
        | "compile", Some h -> CompileLink(h, includeValue) |> Some
        | "compile", _ -> Compile(includeValue) |> Some
        | "content", Some h -> ContentLink(h, includeValue) |> Some
        | "content", _ -> Content(includeValue) |> Some
        | "none", Some h -> NoneItemLink(h, includeValue) |> Some
        | "none", _ -> NoneItem(includeValue) |> Some
        | "reference", _ ->
            Reference
              { ReferenceItem.Include = includeValue
                ReferenceItem.HintPath =
                    match getElemValue "HintPath" with
                    | Some h -> h
                    | None -> ""
                ReferenceItem.IsPrivate =
                    match getElemValue "Private" |> Option.map (fun v -> v.ToLowerInvariant()) with
                    | Some "true"
                    | Some "always"
                    | Some "preservenewest" -> true
                    | _ -> false } |> Some
        | "projectreference", _ ->
            ProjectReference
              { ProjectReferenceItem.Include = includeValue
                ProjectReferenceItem.ProjectGuid =
                    match getElemValue "Project" with
                    | Some h -> Guid.Parse(h)
                    | None -> failwith "expected 'Project' element in 'ProjectReference'!"
                ProjectReferenceItem.Name =
                    match getElemValue "Name" with
                    | Some h -> h
                    | None -> failwith "expected 'Name' element in 'ProjectReference'!"
                ProjectReferenceItem.IsPrivate =
                    match getElemValue "Private" |> Option.map (fun v -> v.ToLowerInvariant()) with
                    | Some "true"
                    | Some "always"
                    | Some "preservenewest" -> true
                    | _ -> false } |> Some
        | _ -> None)
    |> Seq.toList
    
  let filterItems (items ) = items |> List.filter (fun (item : ItemGroupItem) -> item.IsAnyItem)
  let readContentItemsFromDocument (doc : XDocument) =
    readItemGroupItemsFromDocument doc |> filterItems

  let readContentItems (file:string) = readContentItemsFromDocument (XDocument.Load(file))

  let tryFindProperty name (doc : XDocument) =
    doc.Descendants(xname "Project").Descendants(xname "PropertyGroup").Elements(xname name)
    |> Seq.choose (fun e ->
        Some e.Value)
    |> Seq.toList
    |> List.rev
    |> Seq.tryFind (fun _ -> true)

  let readMsBuildInfoFromDocument (doc:XDocument) =
    { MsBuildInfo.Includes = readItemGroupItemsFromDocument doc
      MsBuildInfo.ProjectGuid =
        match tryFindProperty "ProjectGuid" doc with
        | Some v ->  Guid.Parse v
        |  _ -> failwith "ProjectGuid not found!"
      MsBuildInfo.ProjectName =
        match tryFindProperty "AssemblyName" doc with
        | Some v -> v
        | _ -> failwith "AssemblyName not found!" }
  let readMsBuildInfo (file:string) = readMsBuildInfoFromDocument (XDocument.Load file)

  let fixIncludes relPath msBuildInfo =
    { msBuildInfo with
        MsBuildInfo.Includes = msBuildInfo.Includes |> List.map (ProjectGeneratorModule.fixInclude relPath) }