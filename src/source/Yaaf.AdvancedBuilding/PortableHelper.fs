namespace Yaaf.AdvancedBuilding

open System
open System.Xml
open System.Xml.Linq
open System.IO
open System.Collections.Generic

type PlatformConfig =
  { PlatformString : string
    ConfigurationString : string
    }
// Name, TargetFrameworkVersion, TargetFrameworkProfile, TargetFrameworkIdentifier
// Debug|Net40,v4.0, Profile111
// Debug|Net40,v4.0, Profile111, .NETFramework

module Portable =
    let makeSingleProjectPortable (parsedProject:XDocument) =
        
        ()

    let makePortable (parsedSolution:SolutionFile) (parsedProject:XDocument list) =
        
        ()
    let makeProjectPortable solutionFile projectFile =
        ()