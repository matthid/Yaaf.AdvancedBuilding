// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#load @"buildConfig.fsx"
#load "packages/Yaaf.AdvancedBuilding/content/buildInclude.fsx"

open Fake
let config = BuildInclude.config
// Define your FAKE targets here
// This is specific for Yaaf.AdvancedBuilding
open System.IO
if config.ProjectName = "Yaaf.AdvancedBuilding" then
  // Safeguard because this file get into scaffold as initial example.
  BuildInclude.MyTarget "CopyToPackagePath" (fun _ ->
    trace "Copying to packages/Yaaf.AdvancedBuilding/lib because test was OK."
    let outLibDir = config.GlobalPackagesDir @@ "Yaaf.AdvancedBuilding/lib"
    CleanDirs [ outLibDir ]
    Directory.CreateDirectory(outLibDir) |> ignore

    config.BuildTargets
        |> Seq.map (fun buildParam -> buildParam.CustomBuildName)
        |> Seq.map (fun t -> config.BuildDir @@ t, t)
        |> Seq.filter (fun (p, t) -> Directory.Exists p)
        |> Seq.iter (fun (source, buildName) ->
            let outDir = outLibDir @@ buildName 
            ensureDirectory outDir
            config.GeneratedFileList
            |> Seq.filter (fun (file) -> File.Exists (source @@ file))
            |> Seq.iter (fun (file) ->
                let newfile = outDir @@ Path.GetFileName file
                File.Copy(source @@ file, newfile))
        )
  )
  "CopyToRelease"
    ==> "CopyToPackagePath"
    |> ignore
  "CopyToPackagePath"
    ==> "All"
    |> ignore

// start build
RunTargetOrDefault "All"