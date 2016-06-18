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
  printfn "Codepage: %d" Console.OutputEncoding.CodePage

  use prov = new CSharpCodeProvider()
  let source = @"
using Fake;
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
  p.TempFiles.AddFile(assemblyName, true)
  let refs =
    AppDomain.CurrentDomain.GetAssemblies()
    |> Array.filter (fun a -> not a.IsDynamic && not (isNull a.Location))
    |> Array.map (fun a -> a.Location)
  p.ReferencedAssemblies.AddRange(refs) 
  Console.OutputEncoding <- System.Text.Encoding.GetEncoding(1200)
  let results = prov.CompileAssemblyFromSource(p, [| source |])
  if isNull results.Errors |> not && results.Errors.HasErrors then
    printfn "Results: %A" results
    for e in results.Errors do
      printfn " - %s: (%d, %d) %s" e.ErrorNumber e.Line e.Column e.ErrorText
      let enc = System.Text.Encoding.GetEncoding(1200)
      let b = enc.GetBytes(e.ErrorText)
      printfn "Decoded: %s" (System.Text.Encoding.UTF8.GetString b)

    printfn "Native return value: %d" results.NativeCompilerReturnValue
    for m in results.Output do
      printfn "Message: %s" m
    for ref in refs do
      printfn "Reference: %s" ref
    printfn "ResultAssembly: %s" results.PathToAssembly
    printfn "Exists: %s" (if File.Exists results.PathToAssembly then "true" else "false")
    failwith "Compilation failed"
  else 
    printfn "Success"
)

// start build
RunTargetOrDefault "All"