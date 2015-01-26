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
open BuildConfigDef
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
let buildConfig =
 // Read release notes document
 let release = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "doc/ReleaseNotes.md")
 { BuildConfiguration.Defaults with
    ProjectName = "Yaaf.AdvancedBuilding"
    CopyrightNotice = "Yaaf.AdvancedBuilding Copyright © Matthias Dittrich 2015"
    ProjectSummary = "Yaaf.AdvancedBuilding provides several build scripts for you to use."
    ProjectDescription =
      "Yaaf.AdvancedBuilding is a loose collection of build scripts. " +
      "The benefit is that you can automatically update your build by updating this package, " +
      "when you don't need your own crazy build script. But even than it is possible " +
      "that you could re-use some scripts within this package."
    ProjectAuthors = ["Matthias Dittrich"]
    // Defaults to "https://www.nuget.org/packages/%(ProjectName)/"
    //NugetUrl = "https://www.nuget.org/packages/Yaaf.AdvancedBuilding/"
    NugetTags = "building C# F# dotnet .net"
    PageAuthor = "Matthias Dittrich"
    GithubUser = "matthid"
    // Defaults to ProjectName if unset
    // GithubProject = "Yaaf.AdvancedBuilding"
    Version = release.NugetVersion
(**
Setup which nuget packages are created.
*)
    NugetPackages =
      [ "Yaaf.AdvancedBuilding.nuspec", (fun config p ->
          { p with
              Version = config.Version
              NoDefaultExcludes = true
              ReleaseNotes = toLines release.Notes
              Dependencies =
                [ "Yaaf.FSharp.Scripting", "1.0.0"
                  "FSharp.Formatting", "2.6.3"
                  "FSharp.Compiler.Service", "0.0.82"
                  "FAKE", "3.14.8" ] }) ]
(**
With `UseNuget` you can specify if Yaaf.AdvancedBuilding should restore nuget packages
before running the build (if you only use paket, you leave the default setting = false).
*)
    UseNuget = true
(**
## The `GeneratedFileList` property

The `GeneratedFileList` list is used to specify which files are copied over to the release directory.
This list is also used for documentation generation. 
Defaults to [ x.ProjectName + ".dll"; x.ProjectName + ".xml" ] which is only enough for very simple projects.
*)
    GeneratedFileList =
      [ "Yaaf.AdvancedBuilding.dll"
        "Yaaf.AdvancedBuilding.xml" ]

(**
You can change which AssemblyInfo files are generated for you.
On default "./src/SharedAssemblyInfo.fs" and "./src/SharedAssemblyInfo.cs" are created.
*)
    SetAssemblyFileVersions = (fun config ->
      let info =
        [ Attribute.Company config.ProjectName
          Attribute.Product config.ProjectName
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version config.Version
          Attribute.FileVersion config.Version
          Attribute.InformationalVersion config.Version]
      CreateFSharpAssemblyInfo "./src/SharedAssemblyInfo.fs" info)
  }

(**
## FAKE settings

You can setup FAKE variables as well.
*)

if isMono then
    monoArguments <- "--runtime=v4.0 --debug"
    //monoArguments <- "--runtime=v4.0"

(**
## Remove ME!
This is specific to the Yaaf.AdvancedBuilding project, you can safely remove everything below.
*)
if buildConfig.ProjectName = "Yaaf.AdvancedBuilding" then
  if File.Exists "./buildConfig.fsx" then
    // We copy the buildConfig to ./doc so that we can generate a html page from this file
    File.Copy ("./buildConfig.fsx", "./doc/buildConfig.fsx", true)
  // Copy templates to their normal path.
  CopyRecursive "./src/source/Yaaf.AdvancedBuilding/templates" "./src/templates"
  |> ignore
    
    
    