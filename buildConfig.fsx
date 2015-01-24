// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
(**

# Yaaf.AdvancedBuilding buildConfig.fsx configuration

This file handles the configuration of the Yaaf.AdvancedBuilding build script.

The first step is handled in `build.sh` and `build.cmd` by restoring either paket dependencies or bootstrapping a NuGet.exe and
executing NuGet to resolve all build dependencies (dependencies required for the build to work, for example FAKE).
For details of paket look into http://fsprojects.github.io/Paket/ .

To be able to update `build.sh` and `build.cmd` with Yaaf.AdvancedBuilding the `build.sh` and `build.cmd` files in the project root directory delegate to
`packages/Yaaf.AdvancedBuilding/content/build.sh` and `packages/Yaaf.AdvancedBuilding/content/build.cmd`, which do the actual work.

When using NuGet instead of paket (or when using both), nuget packages from a `packages.config` file are restored.
This only works if there is a NuGet.exe found, you can just drop the `downloadNuget.fsx` file in your project root and Yaaf.AdvancedBuilding will make sure to
bootstrap a NuGet.exe into `packages/Nuget.CommandLine`.


The second step is to invoke FAKE with `build.fsx` which loads the current `buildConfig.fsx` file and delegates the work to
`packages/Yaaf.AdvancedBuilding/content/buildInclude.fsx`. For details about FAKE look into http://fsharp.github.com/FAKE .

In the remainder of this document we will explain the various configuration options for Yaaf.Configuration.

*)

(**
## Required config start

First We need to load some dependencies and open some namespaces.
*)
#I @"packages/FAKE/tools/"
#r @"FakeLib.dll"

open System.Collections.Generic
open System.IO

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

(**
## Main project configuration

Then we need to set some general properties of the project.
*)
// properties (main)
let projectName = "Yaaf.AdvancedBuilding"
let copyrightNotice = "Yaaf.AdvancedBuilding Copyright © Matthias Dittrich 2015"
let projectSummary = "Yaaf.AdvancedBuilding provides several build scripts for you to use."
let projectDescription =
  "Yaaf.AdvancedBuilding is a loose collection of build scripts. " +
  "The benefit is that you can automatically update your build by updating this package, " +
  "when you don't need your own crazy build script. But even than it is possible " +
  "that you could re-use some scripts within this package."
let authors = ["Matthias Dittrich"]
let page_author = "Matthias Dittrich"
let mail = "matthi.d@gmail.com"
// Read release notes document
let release = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "doc/ReleaseNotes.md")
let version = release.AssemblyVersion
let version_nuget = release.NugetVersion


let github_user = "matthid"
let github_project = "Yaaf.AdvancedBuilding"
let nuget_url = "https://www.nuget.org/packages/Yaaf.AdvancedBuilding/"

let tags = "building C# F# dotnet .net"

(**
Setup which nuget packages are created.
*)
let nugetPackages =
  [ "Yaaf.AdvancedBuilding.nuspec", (fun p ->
      { p with
          Authors = authors
          Project = projectName
          Summary = projectSummary
          Description = projectDescription
          Version = version_nuget
          NoDefaultExcludes = true
          ReleaseNotes = toLines release.Notes
          Tags = tags
          Dependencies =
            [ "Yaaf.FSharp.Scripting", "1.0.0"
              "FSharp.Formatting", "2.6.3"
              "FSharp.Compiler.Service", "0.0.82"
              "FAKE", "3.14.8" ] }) ]

(**
## The `generated_file_list` property

The `generated_file_list` list is used to specify which files are copied over to the release directory.
This list is also used for documentation generation.
*)
let generated_file_list =
  [ "Yaaf.AdvancedBuilding.dll"
    "Yaaf.AdvancedBuilding.xml" ]

(**
## The `BuildParams` Type

You can define your own type for building, the only limitation is that this type needs the `SimpleBuildName` and `CustomBuildName` properties.
The `SimpleBuildName` property is used for the generated FAKE target for this build `(sprintf "Build_%s" build.SimpleBuildName)`.
The `CustomBuildName` is used as a parameter for msbuild/xbuild and can be used within the fsproj and csproj files to define custom builds.
(IE. custom defines / targets and so on).
The `CustomBuildName` property is also used as the name of the sub-directory within the `buildDir` (see below).
*)
type BuildParams =
    {
        SimpleBuildName : string
        CustomBuildName : string
    }

(**
## Building
You can define all the builds you want to do.
If you only want to do one build use the `emptyParams` from below (and empty `CustomBuildName` means to build directly into `buildDir`).
*)
//let profile111Params = { SimpleBuildName = "profile111"; CustomBuildName = "portable-net45+netcore45+wpa81+MonoAndroid1+MonoTouch1" }
let emptyParams = { SimpleBuildName = ""; CustomBuildName = "" }
//let net45Params = { SimpleBuildName = "net45"; CustomBuildName = "net45" }

(**
And then add them to the `allParams` list so Yaaf.AdvancedBuilding can find them.
*)
let allParams = [ emptyParams ]

(**
With `use_nuget` you can specify if Yaaf.AdvancedBuilding should restore nuget packages
before running the build (if you only use paket, you can set it to false).
*)
let use_nuget = true

(**
Some general build directories you can setup.
*)
let buildDir = "./build/"
let outLibDir = "./release/lib/"
let outDocDir = "./release/documentation/"
let outNugetDir = "./release/nuget/"
let docTemplatesDir = "./doc/templates/"
let testDir  = "./test/"

let buildMode = "Release" // if isMono then "Release" else "Debug"


(**
## More settings

In this section you don't normally need to change anything
*)
let github_url = sprintf "https://github.com/%s/%s" github_user github_project

if isMono then
    monoArguments <- "--runtime=v4.0 --debug"
    //monoArguments <- "--runtime=v4.0"

(**
You can change where Yaaf.AdvancedBuilding tries to find templates (for the documentation generation)
*)
let layoutRoots =
    [ docTemplatesDir; 
      docTemplatesDir @@ "reference" ]

(**
You can change which AssemblyInfo files are generated for you.
*)
let setVersion () = 
  let info =
      [Attribute.Company projectName
       Attribute.Product projectName
       Attribute.Copyright copyrightNotice
       Attribute.Version version
       Attribute.FileVersion version
       Attribute.InformationalVersion version_nuget]
  CreateFSharpAssemblyInfo "./src/SharedAssemblyInfo.fs" info
  let info =
      [Attribute.Company projectName
       Attribute.Product projectName
       Attribute.Copyright copyrightNotice
       Attribute.Version version
       Attribute.FileVersion version
       Attribute.InformationalVersion version_nuget]
  CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.cs" info

(**
The following functions tell Yaaf.AdvancedBuilding where it needs to look for project files.
*)
let findProjectFiles (buildParams:BuildParams) =
    !! (sprintf "src/source/**/*.fsproj")
    ++ (sprintf "src/source/**/*.csproj")

let findTestFiles (buildParams:BuildParams) =
    !! (sprintf "src/test/**/Test.*.fsproj")
    ++ (sprintf "src/test/**/Test.*.csproj")

let unitTestDlls testDir (buildParams:BuildParams) =
    !! (testDir + "/Test.*.dll")

(**
## Remove ME!
This is specific to the Yaaf.AdvancedBuilding project, you can safely remove everything below.
*)
if projectName = "Yaaf.AdvancedBuilding" then
  if File.Exists "./buildConfig.fsx" then
    // We copy the buildConfig to ./doc so that we can generate a html page from this file
    File.Copy ("./buildConfig.fsx", "./doc/buildConfig.fsx", true)
