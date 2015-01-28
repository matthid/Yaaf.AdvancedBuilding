namespace Yaaf.AdvancedBuilding

open System

module ProjectTypes =
    let SolutionFolder = Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8")
    let FSharp = Guid.Parse("F2A71F9B-5D33-465A-A702-920D77279786")
    let CSharp = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")
    let VB = Guid.Parse("F184B08F-C81C-45F6-A57F-5ABD9991F28F")

type SectionItem = string

type SolutionSection =
  { Name : string
    PrePostProcess : string
    Items : SectionItem list }
type SolutionProject =
  { ProjectType : Guid
    Name : string
    Path : string
    ProjectGuid : Guid
    Sections : SolutionSection list }
    static member CreateSolutionFolder name =
      { ProjectType = ProjectTypes.SolutionFolder
        Name = name
        Path = name
        ProjectGuid = Guid.NewGuid()
        Sections = [] }
type SolutionFile =
  { FileVersion : string
    Projects : SolutionProject list
    GlobalSections : SolutionSection list }
    static member NestedProjectGlobalSectionName = "NestedProjects"
    static member Empty = { FileVersion = "12.00"; Projects = []; GlobalSections = []}
    member x.AddNestingItem (parent:Guid) (child:Guid) =
      let newEntry = sprintf "{%s} = {%s}" (child.ToString().ToUpperInvariant()) (parent.ToString().ToUpperInvariant())
      match x.GlobalSections |> Seq.tryFind(fun sec -> sec.Name = SolutionFile.NestedProjectGlobalSectionName) with
      | Some nestingSection ->
        let newNestingSection = { nestingSection with Items = newEntry :: nestingSection.Items }
        { x with GlobalSections = x.GlobalSections |> List.map (fun sec -> if sec.Name = SolutionFile.NestedProjectGlobalSectionName then newNestingSection else sec) }
      | None ->
        let newNestingSection =
          { Name = SolutionFile.NestedProjectGlobalSectionName
            PrePostProcess = "preSolution"
            Items = [newEntry] }
        { x with GlobalSections = newNestingSection :: x.GlobalSections }
    member x.AddSolutionProject (parent:Guid option) (newProject:SolutionProject)  =
      let nestingAdded =
        match parent with
        | Some p ->
          if x.Projects |> Seq.exists (fun pa -> p = pa.ProjectGuid) |> not then
              failwith "the given parent was not found!"
          x.AddNestingItem p newProject.ProjectGuid
        | None -> x
      { nestingAdded with Projects = newProject :: nestingAdded.Projects }
    member x.AddItemToProject (projectGuid:Guid) secName prePost (item:SectionItem) =
      let project =
        match x.Projects |> Seq.tryFind (fun pro -> pro.ProjectGuid = projectGuid) with
        | Some pro -> pro
        | None -> failwith "given project was not found!"
      let newProject =
        match project.Sections |> Seq.tryFind(fun sec -> sec.Name = secName && sec.PrePostProcess = prePost) with
        | Some section ->
          let newSection = { section with Items = item :: section.Items}
          { project with Sections = project.Sections |> List.map (fun sec -> if sec.Name = secName && sec.PrePostProcess = prePost then newSection else sec) }
        | None ->
          let newNestingSection =
            { Name = secName
              PrePostProcess = prePost
              Items = [item] }
          { project with Sections = newNestingSection :: project.Sections }
      { x with Projects = x.Projects |> List.map (fun pro -> if pro.ProjectGuid = projectGuid then newProject else pro) }
    member x.AddSolutionItem (projectGuid) itemName =
        x.AddItemToProject projectGuid "SolutionItems" "preProject" (sprintf "%s = %s" itemName itemName)

open System.IO
module SolutionModule =
    type IReader =
        abstract CurrentLine : string
        abstract MoveNext : unit -> string
        abstract IsAtEnd : bool
    let readPart (reader:IReader) isLineOk handleLine =
        Seq.initInfinite (fun _ ->
            if reader.IsAtEnd || not (isLineOk reader.CurrentLine) then
                None
            else
                let line = handleLine reader
                reader.MoveNext() |> ignore
                Some (line))
            |> Seq.takeWhile (fun s -> s.IsSome)
            |> Seq.choose id
            |> Seq.toList

    let parseHeaderFileVersion (reader:IReader) =
        readPart reader (fun line -> line.Contains("Format Version")) (fun reader ->
            let formatLine = reader.CurrentLine
            formatLine.Split(' ') |> Seq.last)
            |> Seq.exactlyOne
    let parseHeaderIgnore (reader:IReader) =
        readPart reader (fun line -> not (line.Contains("Project("))) (fun reader ->
            reader.CurrentLine)

    let parseSectionItems (reader:IReader) =
        readPart reader (fun line -> line.StartsWith("\t\t")) (fun reader ->
            reader.CurrentLine.Substring(2) : SectionItem)
    let parseSections sectionType (reader:IReader) =
        readPart reader (fun line -> line.StartsWith("\t" + sectionType)) (fun reader ->
            let line = reader.CurrentLine

            let index = line.IndexOf('(') + 1
            let name = line.Substring(index, line.IndexOf(')', index) - index)
            let index = line.IndexOf("= ", index) + 2
            let prePostProject = line.Substring(index)
            reader.MoveNext() |> ignore
            let sections = parseSectionItems reader
            assert (reader.CurrentLine.StartsWith("\tEnd" + sectionType))
            { Name = name
              PrePostProcess = prePostProject
              Items = sections })

    let parseProjects (reader:IReader) =
        readPart reader (fun line -> line.StartsWith("Project(")) (fun reader ->
            let line = reader.CurrentLine

            let index = line.IndexOf('{') + 1
            let projectTypeId = new Guid(line.Substring(index, line.IndexOf('}', index) - index))
            let index = line.LastIndexOf('{') + 1
            let projectId = new Guid(line.Substring(index, line.IndexOf('}', index) - index))
            let index = line.IndexOf("= \"") + 3
            let name = line.Substring(index, line.IndexOf('\"', index) - index)
            let index = line.IndexOf(", \"", index) + 3
            let path = line.Substring(index, line.IndexOf('\"', index) - index)

            reader.MoveNext() |> ignore
            let sections = parseSections "ProjectSection" reader

            { ProjectType = projectTypeId
              Name = name
              Path = path
              ProjectGuid = projectId
              Sections = sections })

    let parseGlobals (reader:IReader) =
        readPart reader (fun line -> line.StartsWith("Global")) (fun reader ->
            reader.MoveNext() |> ignore
            let globalSections = parseSections "GlobalSection" reader
            assert (reader.CurrentLine.StartsWith("EndGlobal"))
            globalSections)
            |> Seq.exactlyOne

    let parse (solutionReader:TextReader) =
        let reader =
            let currentLine = ref (solutionReader.ReadLine())
#if DEBUG
            let lines = new System.Collections.Generic.List<_>()
            lines.Add(!currentLine)
#endif
            { new IReader with
                member x.CurrentLine = !currentLine
                member x.MoveNext () =
                    currentLine := solutionReader.ReadLine()
#if DEBUG
                    lines.Add(!currentLine)
#endif
                    !currentLine
                member x.IsAtEnd = !currentLine = null}

        let fileVersion = parseHeaderFileVersion reader
        parseHeaderIgnore reader |> ignore
        let projects = parseProjects reader
        let globals = parseGlobals reader

        { FileVersion = fileVersion
          Projects = projects
          GlobalSections = globals }

    let write (solution:SolutionFile) (writer:TextWriter) =
        let writeSection sectionType (section:SolutionSection) =
            writer.WriteLine(sprintf "\t%s(%s) = %s" sectionType section.Name section.PrePostProcess)
            for item in section.Items do
                writer.WriteLine(sprintf "\t\t%s" item)
            writer.WriteLine(sprintf "\tEnd%s" sectionType)

        // Write header
        writer.Write(System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetPreamble()))
        let vsVersion =
            match solution.FileVersion with
            | "10.00" -> "2008"
            | "11.00" -> "2010"
            | "12.00" -> "2012"
            | _ -> failwith "unknown fileversion"
        writer.WriteLine(sprintf "Microsoft Visual Studio Solution File, Format Version %s" solution.FileVersion)
        writer.WriteLine(sprintf "# Visual Studio %s" vsVersion)
        // Write Projects
        for project in solution.Projects do
            writer.Write(sprintf "Project(\"{%s}\") = " (project.ProjectType.ToString().ToUpper()))
            writer.WriteLine(
                sprintf "\"%s\", \"%s\", \"{%s}\""
                    project.Name project.Path (project.ProjectGuid.ToString().ToUpper()))

            project.Sections |> Seq.iter (writeSection "ProjectSection")
            writer.WriteLine("EndProject")
        // Write Global
        writer.WriteLine "Global"
        solution.GlobalSections |> Seq.iter (writeSection "GlobalSection")
        writer.WriteLine "EndGlobal"
        writer.Flush()

    let addSolutionProject parent project (solution:SolutionFile) = solution.AddSolutionProject parent project
    let addSolutionItem project item (solution:SolutionFile) = solution.AddSolutionItem project item