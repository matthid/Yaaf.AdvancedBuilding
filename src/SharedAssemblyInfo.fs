namespace System
open System.Reflection

[<assembly: AssemblyCompanyAttribute("Yaaf.AdvancedBuilding")>]
[<assembly: AssemblyProductAttribute("Yaaf.AdvancedBuilding")>]
[<assembly: AssemblyCopyrightAttribute("Yaaf.AdvancedBuilding Copyright © Matthias Dittrich 2015")>]
[<assembly: AssemblyVersionAttribute("0.4.0")>]
[<assembly: AssemblyFileVersionAttribute("0.4.0")>]
[<assembly: AssemblyInformationalVersionAttribute("0.4.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.4.0"
