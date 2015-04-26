namespace Yaaf.AdvancedBuilding

open Mono.Cecil
open Mono.Cecil.Mdb
open Mono.Cecil.Pdb

/// Helper functions to convert platform specific symbol files.
module DebugSymbolHelper =
  let writeAdditionalSymbols readerParams writerParams (assemblyPath:string) =
    let assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParams)
    assemblyDefinition.Write(assemblyPath, writerParams)

  let getDefaultResolver assemblyLocation =
    let assemblyResolver = new DefaultAssemblyResolver()
    assemblyResolver.AddSearchDirectory(assemblyLocation)
    assemblyResolver

  let writeSymbolsFromTo fromReaderProvider toWriterProvider assemblyPath =
    let assemblyResolver = getDefaultResolver assemblyPath
    let readerParameters = new ReaderParameters(AssemblyResolver = assemblyResolver)
    let writerParameters = new WriterParameters()
    writerParameters.SymbolWriterProvider <- toWriterProvider
    let symbolReaderProvider = fromReaderProvider 
    readerParameters.SymbolReaderProvider <- symbolReaderProvider
    readerParameters.ReadSymbols <- true
    writerParameters.WriteSymbols <- true
    writeAdditionalSymbols readerParameters writerParameters assemblyPath

  /// Create a mdb file from an existing pdb file.
  let writeMdbFromPdb assemblyPath = writeSymbolsFromTo (new PdbReaderProvider()) (new MdbWriterProvider()) assemblyPath
  /// Create a pdb file from an existing mdb file.
  let writePdbFromMdb assemblyPath = writeSymbolsFromTo (new MdbReaderProvider()) (new PdbWriterProvider()) assemblyPath
