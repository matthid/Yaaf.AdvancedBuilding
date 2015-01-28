#if PROJGEN
#else
// Support in VS (not actually required in a build-generation script)
#I "../../../packages/Yaaf.AdvancedBuilding/lib/net40/"
#r "Yaaf.AdvancedBuilding.dll"
open Yaaf.AdvancedBuilding
let projectInfo = Unchecked.defaultof<GlobalProjectInfo>
#endif
let framework_references_net45 =
  [ "mscorlib"
    "FSharp.Core, Version=$(TargetFSharpCoreVersion), Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    "System"; "System.Core"; "System.Numerics"; "System.Xml"; "System.Xml.Linq" ]
  |> List.map (fun ref -> Reference { Include = ref; HintPath = ""; IsPrivate = false })
let info =
    MsBuildHelper.readMsBuildInfo "Test.Yaaf.AdvancedBuilding.fsproj"
    |> MsBuildHelper.fixIncludes "../../../test/Test.Yaaf.AdvancedBuilding"
let generatorConfig =
 { BuildFileList =
    [ "../../net45/test/Test.Yaaf.AdvancedBuilding/.fsproj",
        { projectInfo.DefaultTemplateData "net45" with
            TemplateData.Includes = info.ContentIncludes @ info.ProjectReferenceIncludes @ framework_references_net45
            TemplateData.ProjectGuid = info.ProjectGuid
            TemplateData.ProjectName = info.ProjectName
            TemplateName = "fsproj_net45.cshtml" } ] }
