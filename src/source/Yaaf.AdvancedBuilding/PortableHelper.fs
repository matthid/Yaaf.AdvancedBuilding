namespace Yaaf.AdvancedBuilding

open System.Xml.Linq

type PlatformConfig =
  { PlatformString : string
    ConfigurationString : string
    }
// Name, TargetFrameworkVersion, TargetFrameworkProfile, TargetFrameworkIdentifier
// Debug|Net40,v4.0, Profile111
// Debug|Net40,v4.0, Profile111, .NETFramework

module Portable =
    let makeSingleProjectPortable (parsedProject:XDocument) =
        ignore parsedProject
        ()

    let makePortable (parsedSolution:SolutionFile) (parsedProject:XDocument list) =
        ignore parsedProject
        ignore parsedSolution
        ()
    let makeProjectPortable solutionFile projectFile =
        ignore solutionFile
        ignore projectFile
        ()