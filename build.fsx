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
open System
open System.IO
open System.CodeDom
open System.CodeDom.Compiler

#r "Microsoft.CSharp"
open Microsoft.CSharp
open Microsoft.CSharp.RuntimeBinder

Target "Travis" (fun _ ->
  use prov = new CSharpCodeProvider()
  let source = @"
namespace MyNamespace {
  public class MyClass {
    public static string MyMethod () { return ""data""; }
  }
}
"
  let tempDirectory = Path.Combine(Path.GetTempPath(), "RazorEngine_" + Path.GetRandomFileName())
  Directory.CreateDirectory(tempDirectory) |> ignore
  let p = 
    new CompilerParameters(
      GenerateInMemory = false,
      GenerateExecutable = false,
      IncludeDebugInformation = true,
      TreatWarningsAsErrors = false,
      TempFiles = new TempFileCollection(tempDirectory, true),
      CompilerOptions = String.Format("/target:library /optimize /define:RAZORENGINE {0}", ""))
  
  let tempDir = p.TempFiles.TempDir
  let assemblyName = Path.Combine(tempDir, String.Format("{0}.dll", "MyNamespace"))
  p.TempFiles.AddFile(assemblyName, true);
  let results = prov.CompileAssemblyFromSource(p, [| source |])
  if isNull results.Errors |> not && results.Errors.HasErrors then
    printfn "Results: %A" results
    for e in results.Errors do
      printfn " - %s: (%d, %d) %s" e.ErrorNumber e.Line e.Column e.ErrorText
  else 
    printfn "Success"
)

// start build
RunTargetOrDefault "All"