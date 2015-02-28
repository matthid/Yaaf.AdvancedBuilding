namespace Test.Yaaf.AdvancedBuilding

open Yaaf.AdvancedBuilding
open NUnit.Framework
open System
open System.IO
open System.Xml
open System.Xml.Linq

[<TestFixture>]
type PortableTests() =
    [<Test>]
    member x.``check that we can convert a project to a portable project`` () =
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
        () 
