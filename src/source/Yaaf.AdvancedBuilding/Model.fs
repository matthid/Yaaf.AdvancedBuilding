namespace Yaaf.AdvancedBuilding

open System

type ReferenceItem =
  { Include : string
    HintPath : string
    IsPrivate : bool }

type ProjectReferenceItem =
  { Include : string
    Name : string
    ProjectGuid : Guid
    IsPrivate : bool }
//type WhenItem =
//  { Condition : String
//    Includes : ItemGroupItem list }
type ItemGroupItem =
    | Compile of string
    | CompileLink of string * string
    | NoneItem of string
    | NoneItemLink of string * string
    | Content of string
    | ContentLink of string * string
    | Reference of ReferenceItem
    //| Choose of WhenItem list
    | ProjectReference of ProjectReferenceItem with
    member x.IsAnyItem =
        match x with
        | Compile _
        | CompileLink _
        | NoneItem _
        | NoneItemLink _ 
        | Content _
        | ContentLink _ -> true
        | _ -> false
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
        | CompileLink _ -> true
        | NoneItemLink _ -> true
        | ContentLink _ -> true
        | _ -> false
    member x.Include = 
        match x with
        | Compile name
        | CompileLink (_, name) 
        | NoneItem name
        | NoneItemLink (_, name)
        | Content name
        | ContentLink (_, name) -> name
        | Reference { Include = name } -> name
        | ProjectReference { Include = name } -> name
        | _ -> failwith "item has no include property!"
    member x.LinkName = 
        match x with
        | CompileLink (linkName, _) 
        | NoneItemLink (linkName, _)
        | ContentLink (linkName, _) -> linkName
        | _ -> failwith "item is no link!"

type TemplateData =
  { Includes : ItemGroupItem list
    DefineConstants : string list
    TemplateName : string
    ProjectName : string
    ProjectGuid : Guid
    AdditionalData : (string * obj) list }
