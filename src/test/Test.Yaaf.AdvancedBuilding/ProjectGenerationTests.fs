namespace Test.Yaaf.AdvancedBuilding

open Yaaf.AdvancedBuilding
open NUnit.Framework
open System
open System.IO
open System.Xml.Linq

[<TestFixture>]
type ProjectGeneratorTests() =
    let (@@) a b = Path.Combine(a, b)

    [<Test>]
    member x.``check that we can extract items`` () =
      x.DoTheTest()

    member __.DoTheTest() =
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
    member __.``test getprojectfilename`` () =
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
    member __.``test getprojectfilename resulting folder`` () =
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
    member __.``check that we can extract properties`` () =
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