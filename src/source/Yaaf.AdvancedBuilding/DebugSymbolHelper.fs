namespace Yaaf.AdvancedBuilding

open Mono.Cecil
open Mono.Cecil.Mdb
open Mono.Cecil.Pdb

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

  let writeMdbFromPdb assemblyPath = writeSymbolsFromTo (new PdbReaderProvider()) (new MdbWriterProvider()) assemblyPath
  let writePdbFromMdb assemblyPath = writeSymbolsFromTo (new MdbReaderProvider()) (new PdbWriterProvider()) assemblyPath
