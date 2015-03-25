namespace System
open System.Reflection

[<assembly: AssemblyCompanyAttribute("Yaaf.AdvancedBuilding")>]
[<assembly: AssemblyProductAttribute("Yaaf.AdvancedBuilding")>]
[<assembly: AssemblyCopyrightAttribute("Yaaf.AdvancedBuilding Copyright © Matthias Dittrich 2015")>]
[<assembly: AssemblyVersionAttribute("0.5.2")>]
[<assembly: AssemblyFileVersionAttribute("0.5.2")>]
[<assembly: AssemblyInformationalVersionAttribute("0.5.2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.5.2"
