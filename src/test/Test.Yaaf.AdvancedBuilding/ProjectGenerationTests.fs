namespace Test.Yaaf.AdvancedBuilding

open Yaaf.AdvancedBuilding
open NUnit.Framework
open System
open System.IO
open System.Xml
open System.Xml.Linq

[<TestFixture>]
type ProjectGeneratorTests() =
    let (@@) a b = Path.Combine(a, b)
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
                ProjectGuid = System.Guid.NewGuid()
                IsPrivate = true })
          ]
        GlobalData = []
      }
    let templatePath = "templatePath"
    let isMono = try System.Type.GetType("Mono.Runtime") <> null with _ -> false
    let references =
        if isMono then
            // Workaround compiler errors in Razor-ViewEngine
            let d = RazorEngine.Compilation.ReferenceResolver.UseCurrentAssembliesReferenceResolver()
            let loadedList = d.GetReferences() |> Seq.map (fun c -> c.GetFile()) |> Seq.cache
            //// We replace the list and add required items manually as mcs doesn't like duplicates...
            let getItem name =
                match loadedList |> Seq.tryFind (fun l -> l.Contains name) with
                | Some f -> f
                | None -> failwithf "assembly %s is not loaded." name
            [ (getItem "FSharp.Core").Replace("4.3.0.0", "4.3.1.0")  // (if isMono then "/usr/lib64/mono/gac/FSharp.Core/4.3.1.0__b03f5f7f11d50a3a/FSharp.Core.dll" else "FSharp.Core") 
              getItem "Yaaf.FSharp.Scripting.dll"
              getItem "System.Web.Razor.dll"
              getItem "RazorEngine.dll"
              getItem "Yaaf.AdvancedBuilding.dll" ] 
            |> Some
        else None
    let generator = new ProjectGenerator(templatePath, ?references = references)
    let session = generator.FsiSession

    let razorEngineTestHelper files f =
      try
        Directory.CreateDirectory(templatePath) |> ignore
        for name, content in files do
          File.WriteAllText(Path.Combine(templatePath, name), content)
        f ()
      finally
        Directory.Delete(templatePath, true)

    [<Test>]
    member x.``getting the global settings into the fsi works`` () =
      let data = simpleData
      ProjectGeneratorModule.setGlobalSetting session data
      let result = ProjectGeneratorModule.getGlobalSetting session
      test <@ result.ProjectName = data.ProjectName @>

    [<Test>]
    member x.``getting the global settings with advanced data into the fsi works`` () =
      ProjectGeneratorModule.setGlobalSetting session data
      let v = ProjectGeneratorModule.getGlobalSetting session
      test <@ v.ProjectName = data.ProjectName @>
      test <@ v.Includes = data.Includes @>

    [<Test>]
    member x.``check that simple project settings work`` () =
      ProjectGeneratorModule.setGlobalSetting session data
      let expression = "{ BuildFileList = [ \"blub.test\", projectInfo.DefaultTemplateData \"test\" ] }"
      let generatorConfig = ProjectGeneratorModule.projectFileFromExpression session expression
      let expect = { BuildFileList = [ "blub.test", data.DefaultTemplateData "test" ] }
      test <@ generatorConfig = expect @>

    [<Test>]
    member x.``check that we can extract items`` () =
      let projectXml = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
    <RootNamespace>Yaaf.Sasl.Ldap</RootNamespace>
    <AssemblyName>Yaaf.Sasl.Ldap</AssemblyName>
    </PropertyGroup>
    <Import Project="$(SolutionDir)\buildConfig.targets" />
    <ItemGroup>
    <Compile Include="..\..\SharedAssemblyInfo.Ldap.fs">
        <Link>SharedAssemblyInfo.Ldap.fs</Link>
    </Compile>
    <Compile Include="AssemblyInfo.fs" />
    <Compile Include="LdapUserSource.fs" />
    <None Include="Script.fsx" />
    <None Include="packages.config" />
    </ItemGroup>
    <ItemGroup>
    <Reference Include="Mono.Security">
        <HintPath>..\..\..\.nuget\packages\Mono.Security.3.2.3.0\lib\net45\Mono.Security.dll</HintPath>
        <Private>True</Private>
    </Reference>
    <Reference Include="Novell.Directory.Ldap">
        <HintPath>..\..\..\.nuget\packages\Novell.Directory.Ldap.2.2.1\lib\Novell.Directory.Ldap.dll</HintPath>
        <Private>True</Private>
    </Reference>
    <Reference Include="$(FSHARP_CORE_INCLUDE)">
        <HintPath>$(FSHARP_CORE_HINTPATH)</HintPath>
    </Reference>
    </ItemGroup>
    <ItemGroup>
    <ProjectReference Include="$(SolutionDir)\source\Yaaf.Sasl\Yaaf.Sasl.$(CustomBuildName).fsproj">
        <Name>Yaaf.Sasl.$(CustomBuildName)</Name>
        <Project>{acb4d3e9-0627-48c7-9790-10aa6926fb6f}</Project>
        <Private>True</Private>
    </ProjectReference>
    </ItemGroup>
</Project>"""
      let doc = XDocument.Parse(projectXml)
      let resultList = MsBuildHelper.readItemGroupItemsFromDocument doc
      let expectedList =
        [ CompileLink ("SharedAssemblyInfo.Ldap.fs", @"..\..\SharedAssemblyInfo.Ldap.fs")
          Compile("AssemblyInfo.fs")
          Compile("LdapUserSource.fs")
          NoneItem ("Script.fsx")
          NoneItem ("packages.config")
          Reference { Include = "Mono.Security"; HintPath = @"..\..\..\.nuget\packages\Mono.Security.3.2.3.0\lib\net45\Mono.Security.dll"; IsPrivate = true}
          Reference { Include = "Novell.Directory.Ldap"; HintPath = @"..\..\..\.nuget\packages\Novell.Directory.Ldap.2.2.1\lib\Novell.Directory.Ldap.dll"; IsPrivate = true}
          Reference { Include = "$(FSHARP_CORE_INCLUDE)"; HintPath = @"$(FSHARP_CORE_HINTPATH)"; IsPrivate = false}
          ProjectReference { Include = @"$(SolutionDir)\source\Yaaf.Sasl\Yaaf.Sasl.$(CustomBuildName).fsproj"; Name = @"Yaaf.Sasl.$(CustomBuildName)"; ProjectGuid = Guid.Parse("{acb4d3e9-0627-48c7-9790-10aa6926fb6f}"); IsPrivate = true }
          ]
      test <@ resultList = expectedList @>

    [<Test>]
    member x.``test getprojectfilename`` () =
      test <@ ProjectGeneratorModule.getProjectFileName "test._proj" "moretest" = "test.moretest" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test.fsproj._proj" "moretest" = "test.moretest.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test.fsproj._proj" "net.fsproj" = "test.net.fsproj.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test._proj" "net.fsproj" = "test.net.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test._proj" ".fsproj" = "test.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "._proj" "MyProject.net40.fsproj" = "MyProject.net40.fsproj" @>
      Assert.AreEqual(("subdir" @@ "test.net.fsproj"), ProjectGeneratorModule.getProjectFileName ("subdir" @@ "test._proj") "net.fsproj")

      // We ignore exisiting fsx extensions
      test <@ ProjectGeneratorModule.getProjectFileName "test._proj.fsx" "moretest" = "test.moretest" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test.fsproj._proj.fsx" "moretest" = "test.moretest.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test.fsproj._proj.fsx" "net.fsproj" = "test.net.fsproj.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test._proj.fsx" "net.fsproj" = "test.net.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "test._proj.fsx" ".fsproj" = "test.fsproj" @>
      test <@ ProjectGeneratorModule.getProjectFileName "._proj.fsx" "MyProject.net40.fsproj" = "MyProject.net40.fsproj" @>
      Assert.AreEqual(("subdir" @@ "test.net.fsproj"), ProjectGeneratorModule.getProjectFileName ("subdir" @@ "test._proj.fsx") "net.fsproj")

    [<Test>]
    member x.``test getprojectfilename resulting folder`` () =
      Assert.AreEqual(".." @@ "test.moretest", ProjectGeneratorModule.getProjectFileName "test._proj" "../moretest")
      Assert.AreEqual(
        ".." @@ ".." @@ "source_net40" @@ "Project_net40" @@ "test.net40.fsproj",
        ProjectGeneratorModule.getProjectFileName "test._proj" "../../source_net40/Project_net40/net40.fsproj")

      Assert.AreEqual(
        "subdir" @@ ".." @@ ".." @@ "source_net40" @@ "Project_net40" @@ "test.net40.fsproj",
        ProjectGeneratorModule.getProjectFileName ("subdir" @@ "test._proj") "../../source_net40/Project_net40/net40.fsproj")
      Assert.AreEqual(
        ".." @@ ".." @@ ".." @@ "source_net40" @@ "Project_net40" @@ "test.net40.fsproj",
        ProjectGeneratorModule.getProjectFileName (".." @@ "test._proj") "../../source_net40/Project_net40/net40.fsproj")


    [<Test>]
    member x.``test razorengine`` () =
      let files =
        [ "net40_template.fsproj", "@Model.ProjectName"
          "setting._proj", """
let generatorConfig =
 { BuildFileList =
    [ "net40.fsproj",
        { projectInfo.DefaultTemplateData "net40" with
            TemplateName = "net40_template.fsproj" } ] }""" ]
      razorEngineTestHelper files (fun () ->
        generator.GenerateProjectFiles({GlobalProjectInfo.Empty with ProjectName = "testProject"}, Path.Combine(templatePath, "setting._proj"))
        let text = File.ReadAllText(Path.Combine(templatePath, "setting.net40.fsproj"))
        test <@ text = "testProject" @>)

    [<Test>]
    member x.``test that we can parse MSBuild files in the settings file`` () =
      let files =
        [ "net40_template", """@foreach (var includeItem in Model.Includes)
{
    if (includeItem.IsCompile)
    {
<Compile Include="@includeItem.Include" />}}"""
          "MyProject.fsproj", """<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <ItemGroup>
    <Compile Include="..\..\SharedAssemblyInfo.Ldap.fs">
        <Link>SharedAssemblyInfo.Ldap.fs</Link>
    </Compile>
    <Compile Include="AssemblyInfo.fs" />
    <Compile Include="LdapUserSource.fs" />
    <None Include="Script.fsx" />
    <None Include="packages.config" />
    </ItemGroup>
</Project>"""
          "._proj", """
let readItems = MsBuildHelper.readContentItems "MyProject.fsproj"
let generatorConfig =
 { BuildFileList =
    [ "MyProject.net40.fsproj",
       { projectInfo.DefaultTemplateData "net40" with
          Includes = readItems
          TemplateName = "net40_template" } ] }""" ]
      razorEngineTestHelper files (fun () ->
        generator.GenerateProjectFiles({GlobalProjectInfo.Empty with ProjectName = "testProject"}, Path.Combine(templatePath, "._proj"))
        let text = File.ReadAllText(Path.Combine(templatePath, "MyProject.net40.fsproj"))
        let expected = """<Compile Include="AssemblyInfo.fs" /><Compile Include="LdapUserSource.fs" />"""
        Assert.AreEqual(expected, text))

    [<Test>]
    member x.``check that we can extract properties`` () =
      let projectXml = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <RootNamespace>Yaaf.Sasl.Ldap</RootNamespace>
        <AssemblyName>Yaaf.Sasl.Ldap</AssemblyName>
    </PropertyGroup>
    <Import Project="$(SolutionDir)\buildConfig.targets" />
    <ItemGroup>
    <Compile Include="..\..\SharedAssemblyInfo.Ldap.fs">
        <Link>SharedAssemblyInfo.Ldap.fs</Link>
    </Compile>
    <Compile Include="AssemblyInfo.fs" />
    <Compile Include="LdapUserSource.fs" />
    <None Include="Script.fsx" />
    <None Include="packages.config" />
    </ItemGroup>
    <ItemGroup>
    <Reference Include="Mono.Security">
        <HintPath>..\..\..\.nuget\packages\Mono.Security.3.2.3.0\lib\net45\Mono.Security.dll</HintPath>
        <Private>True</Private>
    </Reference>
    <Reference Include="Novell.Directory.Ldap">
        <HintPath>..\..\..\.nuget\packages\Novell.Directory.Ldap.2.2.1\lib\Novell.Directory.Ldap.dll</HintPath>
        <Private>True</Private>
    </Reference>
    <Reference Include="$(FSHARP_CORE_INCLUDE)">
        <HintPath>$(FSHARP_CORE_HINTPATH)</HintPath>
    </Reference>
    </ItemGroup>
    <ItemGroup>
    <ProjectReference Include="$(SolutionDir)\source\Yaaf.Sasl\Yaaf.Sasl.$(CustomBuildName).fsproj">
        <Name>Yaaf.Sasl.$(CustomBuildName)</Name>
        <Project>{acb4d3e9-0627-48c7-9790-10aa6926fb6f}</Project>
        <Private>True</Private>
    </ProjectReference>
    </ItemGroup>
</Project>"""
      let doc = XDocument.Parse(projectXml)
      let assemblyName = MsBuildHelper.tryFindProperty "AssemblyName" doc
      test <@ Some "Yaaf.Sasl.Ldap" = assemblyName @>