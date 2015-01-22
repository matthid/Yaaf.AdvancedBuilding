module Test.Yaaf.AdvancedBuilding.ProjectGeneratorTests
open Swensen.Unquote
open Yaaf.AdvancedBuilding
open NUnit.Framework
let simpleData =
    { BuildTemplates = []
      ProjectName = "ProjectName"
      Includes = []
      GlobalData = []
    }
let data = 
  { BuildTemplates = [ ("test", "blub")]
    ProjectName = "ProjectName"
    Includes = 
      [ Content("test")
        ContentLink("blub", "../test")
        NoneItem("none")
        NoneItemLink("nonelink", "../othertest")
        Reference(
          { Include = "FSharp.Core"
            HintPath = "Path to file"
            IsPrivate = true })
        ProjectReference(
          { Include = "C:/test/FSharp.Core"
            Name = "FSharp.Core"
            ProjectGuid = System.Guid.NewGuid().ToString()
            IsPrivate = true })
      ]
    GlobalData = []
  }
[<Test>]
let ``getting the global settings into the fsi works`` () =
  let data = simpleData
  ProjectGenerator.setGlobalSetting data
  let result = ProjectGenerator.getGlobalSetting ()
  test <@ result.ProjectName = data.ProjectName @>
  
[<Test>]
let ``getting the global settings with advanced data into the fsi works`` () =
  ProjectGenerator.setGlobalSetting data
  let v = ProjectGenerator.getGlobalSetting ()
  test <@ v.ProjectName = data.ProjectName @>
  test <@ v.Includes = data.Includes @>
  
[<Test>]
let ``check that simple project settings work`` () =
  ProjectGenerator.setGlobalSetting data
  let expression = "{ BuildFileList = [ \"blub.test\", projectInfo.DefaultTemplateData \"test\" ] }"
  let generatorConfig = ProjectGenerator.projectFileFromExpression expression
  let expect = { BuildFileList = [ "blub.test", data.DefaultTemplateData "test" ] }
  test <@ generatorConfig = expect @>