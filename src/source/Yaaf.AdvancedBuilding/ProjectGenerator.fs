namespace Yaaf.AdvancedBuilding
open System
open System.Collections.Generic
open Yaaf.FSharp.Scripting

type ReferenceItem =
  { Include : string
    HintPath : string
    IsPrivate : bool }

type ProjectReferenceItem =
  { Include : string
    Name : string
    ProjectGuid : string
    IsPrivate : bool }

type ItemGroupItem =
    | Compile of string
    | LinkCompile of string * string
    | NoneItem of string
    | NoneItemLink of string * string
    | Content of string
    | ContentLink of string * string 
    | Reference of ReferenceItem
    | ProjectReference of ProjectReferenceItem with
    member x.IsAnyReference =
        match x with
        | Reference _ -> true
        | ProjectReference _ -> true
        | _ -> false
    member x.ReferenceItem =
        match x with
        | Reference ref -> ref
        | _ -> failwith "no reference item!"
    member x.ProjectReferenceItem =
        match x with
        | ProjectReference ref -> ref
        | _ -> failwith "no project reference item!"
    member x.IsLink = 
        match x with
        | LinkCompile _ -> true
        | NoneItemLink _ -> true
        | ContentLink _ -> true
        | _ -> false
    member x.Include = 
        match x with
        | Compile name
        | LinkCompile (_, name) 
        | NoneItem name
        | NoneItemLink (_, name)
        | Content name
        | ContentLink (_, name) -> name
        | Reference { Include = name } -> name
        | ProjectReference { Include = name } -> name
    member x.LinkName = 
        match x with
        | LinkCompile (linkName, _) 
        | NoneItemLink (linkName, _)
        | ContentLink (linkName, _) -> linkName
        | _ -> failwith "item is no link!"

type TemplateData =
  { Includes : ItemGroupItem list
    TemplateName : string
    ProjectName : string
    AdditionalData : (string * string) list }

//[<StructuredFormatDisplay("{AsString}")>]
type GlobalProjectInfo = 
  { BuildTemplates : (string * string) list
    ProjectName : string
    Includes : ItemGroupItem list
    GlobalData : (string * string) list

    } with
    member x.DefaultTemplateData buildName =
      let buildTemplates = x.BuildTemplates |> dict
      { Includes = x.Includes
        TemplateName = if buildTemplates.ContainsKey buildName then buildTemplates.[buildName] else "unknown"
        ProjectName = x.ProjectName
        AdditionalData = []
      }

type ProjectGeneratorConfig =
  { BuildFileList : (string * TemplateData) list
     }

open System
open System.IO
open System.Text

/// Documentation for my library
///
/// ## Example
///
///     let h = Library.hello 1
///     printfn "%d" h
///
module ProjectGenerator = 
  let session = ScriptHost.CreateNew()
  do
    session.Open ("System")
    session.Open ("System.Collections.Generic")
    session.Reference (System.Reflection.Assembly.GetExecutingAssembly().Location)
    session.Open ("Yaaf.AdvancedBuilding")
    
  let setGlobalSetting (g:GlobalProjectInfo) =
    session.Let "projectInfo" g
    //fsiSession.EvalInteraction (sprintf "let projectInfo = \n %s" ((sprintf "%A" g).Replace("\n", "\n ")))

  let getGlobalSetting () =
    session.EvalExpression<GlobalProjectInfo> ("projectInfo")

  let projectFileFromExpression contents =
    session.EvalExpression<ProjectGeneratorConfig> (contents)

  let generateProjectFile buildFilename templateData = 
    ()

  /// Returns 42
  ///
  /// ## Parameters
  ///  - `num` - whatever
  let generateProjectFiles (globalInfo:GlobalProjectInfo) (settingsFile:string) = 
    let contents = File.ReadAllText settingsFile
    let config = projectFileFromExpression contents
    for buildFilename, templateData in config.BuildFileList do
      generateProjectFile buildFilename templateData