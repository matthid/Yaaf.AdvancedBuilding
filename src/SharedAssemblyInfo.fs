﻿namespace System
open System.Reflection

[<assembly: AssemblyCompanyAttribute("Yaaf.AdvancedBuilding")>]
[<assembly: AssemblyProductAttribute("Yaaf.AdvancedBuilding")>]
[<assembly: AssemblyCopyrightAttribute("Yaaf.AdvancedBuilding Copyright © Matthias Dittrich 2015")>]
[<assembly: AssemblyVersionAttribute("0.14.1")>]
[<assembly: AssemblyFileVersionAttribute("0.14.1")>]
[<assembly: AssemblyInformationalVersionAttribute("0.14.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.14.1"
    let [<Literal>] InformationalVersion = "0.14.1"
