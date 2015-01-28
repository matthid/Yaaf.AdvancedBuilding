// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
open System.Collections.Generic
open System.IO
open System

(**
## The `BuildParams` Type

You can define your own type for building, the only limitation is that this type needs the `SimpleBuildName` and `CustomBuildName` properties.
The `SimpleBuildName` property is used for the generated FAKE target for this build `(sprintf "Build_%s" build.SimpleBuildName)`.
The `CustomBuildName` is used as a parameter for msbuild/xbuild and can be used within the fsproj and csproj files to define custom builds.
(IE. custom defines / targets and so on).
The `CustomBuildName` property is also used as the name of the sub-directory within the `buildDir` (see below).
*)
type NuGetParams = string
type NUnitParams = string
type Attribute =
    | Company of string
    | Product  of string
    | Copyright  of string
    | Version of string
    | FileVersion  of string
    | InformationalVersion of string
module Operators =
    let (!!) s = [s]
    let (++) l s = s :: l
    let (@@) l s = s + l
    let CreateFSharpAssemblyInfo l r = ()
    let CreateCSharpAssemblyInfo l r = ()
open Operators
type BuildParams =
  { SimpleBuildName : string
    CustomBuildName : string
    BuildMode : string
    FindProjectFiles : unit -> string seq
    FindTestFiles : unit -> string seq
    FindUnitTestDlls : string -> string seq }
  static member Empty = 
    { SimpleBuildName = ""
      BuildMode = "Release"
      CustomBuildName = ""
      FindProjectFiles = (fun () -> 
        !! (sprintf "src/source/**/*.fsproj")
        ++ (sprintf "src/source/**/*.csproj")
        :> _)
      FindTestFiles = (fun () -> 
        !! (sprintf "src/test/**/Test.*.fsproj")
        ++ (sprintf "src/test/**/Test.*.csproj")
        :> _)
      FindUnitTestDlls = (fun testDir -> 
        !! (testDir + "/Test.*.dll")
        :> _) }

type BuildConfiguration =
  { // Metadata
    ProjectName : string
    ProjectSummary : string
    CopyrightNotice : string
    ProjectDescription : string
    ProjectAuthors : string list
    GithubUser : string
    /// Defaults to ProjectName
    GithubProject : string
    PageAuthor : string
    /// Defaults to github issues
    IssuesUrl : string
    /// Defaults to github new issue page
    FileNewIssueUrl : string
    /// Defaults to github master branch "/blob/master/"
    SourceReproUrl : string
    
    // Nuget configuration
    /// Defaults to sprintf "https://www.nuget.org/packages/%s/" x.ProjectName
    NugetUrl : string
    NugetTags : string
    NugetPackages : (string * (BuildConfiguration -> NuGetParams -> NuGetParams)) list
    // Defaults to "./release/nuget/"
    OutNugetDir : string
    
    // Pre build
    Version : string
    /// Defaults to setting up a "./src/SharedAssemblyInfo.fs" and "./src/SharedAssemblyInfo.cs"
    SetAssemblyFileVersions : BuildConfiguration -> unit
    
    // Build configuration
    /// Defaults to [ x.ProjectName + ".dll"; x.ProjectName + ".xml" ]
    GeneratedFileList : string list
    /// Defaults to false (support for nuget msbuild integration)
    UseNuget : bool
    BuildTargets : BuildParams list
    /// Defaults to "./build/"
    BuildDir : string
    /// Defaults to "./release/lib/"
    OutLibDir : string
    NugetPackageDir : string
    GlobalPackagesDir : string
    
    // Test
    /// Defaults to "./test/"
    TestDir : string
    SetupNUnit : (NUnitParams -> NUnitParams)
    
    // Documentation generation
    /// Defaults to "./release/documentation/"
    OutDocDir : string
    /// Defaults to "./doc/templates/"
    DocTemplatesDir : string
    LayoutRoots : string list }
  static member Empty =
    { ProjectName = "" 
      ProjectSummary = ""
      CopyrightNotice = ""
      ProjectDescription = ""
      UseNuget = false
      ProjectAuthors = []
      BuildTargets = [ BuildParams.Empty ]
      NugetUrl = ""
      NugetTags = ""
      PageAuthor = ""
      GithubUser = ""
      GithubProject = ""
      SetAssemblyFileVersions = (fun config ->
        let info =
          [ Attribute.Company config.ProjectName
            Attribute.Product config.ProjectName
            Attribute.Copyright config.CopyrightNotice
            Attribute.Version config.Version
            Attribute.FileVersion config.Version
            Attribute.InformationalVersion config.Version]
        CreateFSharpAssemblyInfo "./src/SharedAssemblyInfo.fs" info
        CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.cs" info)
      Version = ""
      IssuesUrl = ""
      FileNewIssueUrl = ""
      SourceReproUrl = ""
      NugetPackages = []
      SetupNUnit = id
      GeneratedFileList = []
      BuildDir = "./build/"
      OutLibDir = "./release/lib/"
      OutDocDir = "./release/documentation/"
      OutNugetDir = "./release/nuget/"
      DocTemplatesDir = "./doc/templates/"
      LayoutRoots = [ ]
      TestDir  = "./test/"
      GlobalPackagesDir = "./packages"
      NugetPackageDir = "./packages/.nuget" }
  member x.GithubUrl = sprintf "https://github.com/%s/%s" x.GithubUser x.GithubProject
  member x.WithDefaults () =
    { x with
        NugetUrl = 
          if String.IsNullOrEmpty x.NugetUrl then sprintf "https://www.nuget.org/packages/%s/" x.ProjectName else x.NugetUrl
        GithubProject = if String.IsNullOrEmpty x.GithubProject then x.ProjectName else x.GithubProject
        IssuesUrl = if String.IsNullOrEmpty x.IssuesUrl then sprintf "%s/issues" x.GithubUrl else x.IssuesUrl
        FileNewIssueUrl = 
          if String.IsNullOrEmpty x.FileNewIssueUrl then sprintf "%s/issues/new" x.GithubUrl else x.FileNewIssueUrl
        SourceReproUrl = 
          if String.IsNullOrEmpty x.SourceReproUrl then x.GithubUrl + "/blob/master/" else x.SourceReproUrl
        GeneratedFileList =
          if x.GeneratedFileList |> List.isEmpty |> not then x.GeneratedFileList
          else [ x.ProjectName + ".dll"; x.ProjectName + ".xml" ]
        LayoutRoots =
          if x.LayoutRoots.IsEmpty then [ x.DocTemplatesDir; x.DocTemplatesDir @@ "reference" ]
          else x.LayoutRoots}
let blub = "blub"