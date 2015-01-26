// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This is a very simple FAKE script to bootstrap a working Yaaf.AdvancedBuilding.dll for our real build script.
*)

#I "packages/FAKE/tools/"
#r @"FakeLib.dll"
open System.Collections.Generic
open System.IO

open Fake

Target "Build" (fun _ -> 
    let buildDir = "packages/Yaaf.AdvancedBuilding/lib/net40"
    CleanDirs [ buildDir ]
    // build app
    !! "src/source/**/*.fsproj"
        |> MSBuild buildDir "Build"
            [   "Configuration", "Release"
                "CustomBuildName", "" ]
        |> Log "BOOTSTRAP: "
)
// start boostrapping
RunTargetOrDefault "Build"
