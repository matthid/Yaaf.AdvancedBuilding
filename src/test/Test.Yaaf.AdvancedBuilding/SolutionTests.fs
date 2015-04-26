namespace Test.Yaaf.AdvancedBuilding

open Yaaf.AdvancedBuilding
open NUnit.Framework
open System.IO

[<TestFixture>]
type SolutionGeneratorTests() =
    [<Test>]
    member __.``check that we read and write a solution file`` () =
        let solutionFile = """Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2013
VisualStudioVersion = 12.0.31101.0
MinimumVisualStudioVersion = 10.0.40219.1
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = ".paket", ".paket", "{63297B98-5CED-492C-A5B7-A5B4F73CF142}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "docs", "docs", "{A6A6AF7D-D6E3-442D-9B1E-58CC91879BE1}"
EndProject
Project("{F2A71F9B-5D33-465A-A702-920D77279786}") = "Yaaf.AdvancedBuilding", "source\Yaaf.AdvancedBuilding\Yaaf.AdvancedBuilding.fsproj", "{74CD69FA-7D48-4FC4-A478-A1F00A08E963}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "project", "project", "{BF60BC93-E09B-4E5F-9D85-95A519479D54}"
	ProjectSection(SolutionItems) = preProject
		build.fsx = build.fsx
		README.md = README.md
		..\nuget\Yaaf.AdvancedBuilding.nuspec = ..\nuget\Yaaf.AdvancedBuilding.nuspec
	EndProjectSection
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "tools", "tools", "{83F16175-43B1-4C90-A1EE-8E351C33435D}"
EndProject
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "content", "content", "{8E6D5255-776D-4B61-85F9-73C37AA1FB9A}"
	ProjectSection(SolutionItems) = preProject
		docs\content\index.fsx = docs\content\index.fsx
		docs\content\tutorial.fsx = docs\content\tutorial.fsx
	EndProjectSection
EndProject
Project("{F2A71F9B-5D33-465A-A702-920D77279786}") = "Test.Yaaf.AdvancedBuilding", "test\Test.Yaaf.AdvancedBuilding\Test.Yaaf.AdvancedBuilding.fsproj", "{A01FB502-0A1D-4745-9313-EFAA85207866}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{74CD69FA-7D48-4FC4-A478-A1F00A08E963}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{74CD69FA-7D48-4FC4-A478-A1F00A08E963}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{74CD69FA-7D48-4FC4-A478-A1F00A08E963}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{74CD69FA-7D48-4FC4-A478-A1F00A08E963}.Release|Any CPU.Build.0 = Release|Any CPU
		{A01FB502-0A1D-4745-9313-EFAA85207866}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A01FB502-0A1D-4745-9313-EFAA85207866}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A01FB502-0A1D-4745-9313-EFAA85207866}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A01FB502-0A1D-4745-9313-EFAA85207866}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{83F16175-43B1-4C90-A1EE-8E351C33435D} = {A6A6AF7D-D6E3-442D-9B1E-58CC91879BE1}
		{8E6D5255-776D-4B61-85F9-73C37AA1FB9A} = {A6A6AF7D-D6E3-442D-9B1E-58CC91879BE1}
	EndGlobalSection
EndGlobal
"""
        let solution = SolutionModule.parse (new StringReader(solutionFile))
        test <@ solution.FileVersion = "12.00" @>
        let writer = new StringWriter()
        SolutionModule.write solution writer
        writer.Flush()
        let written = writer.GetStringBuilder().ToString()
        let solution2 = SolutionModule.parse (new StringReader(written))
        test <@ solution = solution2 @>

    [<Test>]
    member __.``check that we can generate a solution`` () =
        File.WriteAllText ("file.txt", "test")
        let solution = SolutionGenerator.generateSolution [] [ { PathInSolution = "test"; PathRelativeToSolutionFile = "file.txt" }]
        test <@ solution.Projects |> Seq.length = 1 @>
        test <@ solution.Projects |> Seq.collect (fun pro -> pro.Sections) |> Seq.collect (fun sec -> sec.Items) |> Seq.length = 1 @>
        test <@ solution.GlobalSections |> Seq.length = 0 @>

    [<Test>]
    member __.``check that we can generate a solution (2)`` () =
        File.WriteAllText ("file.txt", "test")
        File.WriteAllText ("file2.txt", "test2")
        let solution =
            SolutionGenerator.generateSolution []
                [ { PathInSolution = "test/sub"; PathRelativeToSolutionFile = "file.txt" }
                  { PathInSolution = "test"; PathRelativeToSolutionFile = "file.txt" }
                  { PathInSolution = "test"; PathRelativeToSolutionFile = "file2.txt" }
                  { PathInSolution = "test/sub"; PathRelativeToSolutionFile = "file2.txt" } ]
        test <@ solution.Projects |> Seq.length = 2 @>
        test <@ solution.Projects |> Seq.collect (fun pro -> pro.Sections) |> Seq.collect (fun sec -> sec.Items) |> Seq.length = 4 @>
        test <@ solution.GlobalSections |> Seq.length = 1 @>

    [<Test>]
    member __.``check that the relative function works`` () =
        test <@ SolutionGenerator.makeRelative "./dir" "./project.fsx" = "..\\project.fsx" @>
        test <@ SolutionGenerator.makeRelative "./dir" "./dir/project.fsx" = "project.fsx" @>
        test <@ SolutionGenerator.makeRelative "./dir/sub" "./other/project.fsx" = "..\\..\\other\\project.fsx" @>
        test <@ SolutionGenerator.makeRelative "./src/" "./src/source/project.fsx" = "source\\project.fsx" @>