namespace Yaaf.AdvancedBuilding
open System
open System.Xml
open System.Xml.Linq
open System.IO
open System.Collections.Generic

type SolutionItemHelper =
  { PathInSolution : string
    PathRelativeToSolutionFile : string }
type SolutionProjectHelper =
  { PathInSolution : string
    Project : SolutionProject }

module SolutionGenerator =
    let makeRelative (solutionDirectory:string) projectFile =
        let skipLast = 
            (if solutionDirectory.EndsWith("/") then solutionDirectory.Substring(0, solutionDirectory.Length - 1) else solutionDirectory)
        let solutionUri = new Uri(Path.GetFullPath skipLast + "/")
        let projectFileUri = new Uri(Path.GetFullPath projectFile)
        solutionUri.MakeRelativeUri(projectFileUri).ToString().Replace("/", "\\")

    let getSolutionProject solutionDirectory projectFile =
        let doc = XDocument.Load(projectFile:string)
        let projectGuid =
            match MsBuildHelper.tryFindProperty "ProjectGuid" doc with
            | Some guid -> Guid.Parse guid
            | None -> failwith "ProjectGuid not found!"
        { SolutionProject.Path = makeRelative solutionDirectory projectFile
          SolutionProject.Name = Path.GetFileNameWithoutExtension projectFile
          SolutionProject.ProjectGuid = projectGuid
          SolutionProject.ProjectType =
            match Path.GetExtension projectFile with
            | ".fsproj" -> ProjectTypes.FSharp
            | ".csproj" -> ProjectTypes.CSharp
            | ".vbproj" -> ProjectTypes.VB
            | _ -> failwith "unknown project file extension!"
          SolutionProject.Sections = [] }

    let generateSolution (solutionProjects:SolutionProjectHelper list) (otherItems:SolutionItemHelper list) =
        // add all folders
        let solution, pathMapping =
            otherItems 
            |> Seq.map (fun helper -> helper.PathInSolution)
            |> Seq.append (solutionProjects |> Seq.map (fun helper -> helper.PathInSolution))
            |> Seq.fold (fun (sln, (mapping:IDictionary<string,Guid>)) newItem ->
                let rec addPath path solution =
                    if String.IsNullOrEmpty path then
                        None, solution
                    else
                        match mapping.TryGetValue(path) with
                        | true, v -> Some v, solution
                        | _ ->
                        let parent = Path.GetDirectoryName path
                        let parentUid, solution = addPath parent solution
                        let folder = SolutionProject.CreateSolutionFolder (Path.GetFileName path)
                        mapping.Add(path, folder.ProjectGuid)
                        Some folder.ProjectGuid, solution |> SolutionModule.addSolutionProject parentUid (folder)
                (snd (addPath newItem sln), mapping)
                ) (SolutionFile.Empty, new Dictionary<string, Guid>() :> _)
        
        // add all items
        let solution =
            otherItems
            |> Seq.fold (fun (sln:SolutionFile) (helper) -> 
                let rel = helper.PathInSolution
                if (String.IsNullOrEmpty rel) then failwith "Solution items in root are not allowed!"
                let pathGuid = pathMapping.[rel]
                sln.AddSolutionItem pathGuid helper.PathRelativeToSolutionFile
                ) solution

        // Add all project files
        solutionProjects
        |> Seq.fold (fun (sln:SolutionFile) solutionProject ->
            let parent =
                if String.IsNullOrEmpty solutionProject.PathInSolution then None
                else Some (pathMapping.[solutionProject.PathInSolution])
            sln.AddSolutionProject parent solutionProject.Project) solution