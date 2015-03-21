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
    let buildDir = "packages/Yaaf.AdvancedBuilding/tools"
    CleanDirs [ buildDir ]
    // build app
    !! "src/**/*.sln"
        |> MSBuild buildDir "Build"
            [ "Configuration", "Release"
              "Platform", "Sol_Net45" ]
        |> Log "BOOTSTRAP: "
    // They aren't available on end systems!
    [ "FSharp.Compiler.Service.dll"; "FSharp.Core.dll"
      "nunit.framework.dll"; "Test.Yaaf.AdvancedBuilding.dll"
      "Unquote.dll" ]
    |> Seq.iter (fun file ->
      trace (sprintf "DELETING: %s" file)
      File.Delete(sprintf "packages/Yaaf.AdvancedBuilding/tools/%s" file))
)
// start boostrapping
RunTargetOrDefault "Build"
