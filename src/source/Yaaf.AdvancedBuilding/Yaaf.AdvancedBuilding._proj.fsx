#if PROJGEN
#else
// Support in VS (not actually required in a build-generation script)
#I "../../../packages/Yaaf.AdvancedBuilding/tools/"
#r "Yaaf.AdvancedBuilding.dll"
open Yaaf.AdvancedBuilding
let projectInfo = Unchecked.defaultof<GlobalProjectInfo>
#endif
let framework_references_net45 =
  [ "mscorlib"
    //"FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    "System"; "System.Core"; "System.Numerics"; "System.Xml"; "System.Xml.Linq" ]
  |> List.map (fun ref -> Reference { Include = ref; HintPath = ""; IsPrivate = false })
let info =
    MsBuildHelper.readMsBuildInfo "Yaaf.AdvancedBuilding.fsproj"
    |> MsBuildHelper.fixIncludes "../../../source/Yaaf.AdvancedBuilding"
// Paket doesn't support grouping so we handle RazorEngine ourself (as we have Razor locked to version 2)
// But we need Version 3 here
let razorEngine =
  [ Reference
      { Include = "RazorEngine"
        HintPath = @"..\..\..\..\packages\RazorEngine\lib\net45\RazorEngine.dll"
        IsPrivate = true }
    Reference
      { Include = "System.Web.Razor"
        HintPath = @"..\..\..\..\packages\.nuget\Microsoft.AspNet.Razor.3.2.2\lib\net45\System.Web.Razor.dll"
        IsPrivate = true } ]
let generatorConfig =
 { BuildFileList =
    [ "../../net45/source/Yaaf.AdvancedBuilding/.fsproj",
        { projectInfo.DefaultTemplateData "net45" with
            TemplateData.Includes = info.ContentIncludes @ razorEngine @ info.ProjectReferenceIncludes @ framework_references_net45
            TemplateData.ProjectGuid = info.ProjectGuid
            TemplateData.ProjectName = info.ProjectName
            TemplateName = "fsproj_net45.cshtml" } ] }
