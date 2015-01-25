namespace Yaaf.AdvancedBuilding
open System
type Razor_TemplateModel = 
    {
       ProjectName : string
       ProjectGuid : Guid
       DefineConstants : string seq
       Includes : ItemGroupItem seq
    }

open System.IO
open RazorEngine
open RazorEngine.Templating
type RazorManager(templatePath, ?references) =
  let config = new Configuration.TemplateServiceConfiguration()
  do
    config.TemplateManager <- 
      { new ITemplateManager with
        member x.Resolve key = 
          let path = Path.Combine (templatePath, key.Name)
          if File.Exists path then new LoadedTemplateSource(File.ReadAllText path, path) :> _
          else failwithf "template %s was not found" path
        member x.GetKey(name, resolveType, context) = new NameOnlyTemplateKey(name, resolveType, context) :> _
        member x.AddDynamic(key, source) = failwith "AddDynamic is not supported!" }
    references |> Option.iter (fun (refs : string list) ->
      config.ReferenceResolver <-
        { new RazorEngine.Compilation.ReferenceResolver.IReferenceResolver with
          member x.GetReferences (context, inc) = refs |> Seq.map (RazorEngine.Compilation.ReferenceResolver.CompilerReference.From) })
  let service = RazorEngineService.Create(config)

  member x.CreateProjectFile(data:TemplateData, outfile:string) =
    use file = File.OpenWrite(outfile)
    use writer = new StreamWriter(file)
    service.RunCompile(
        data.TemplateName,
        writer,
        typeof<Razor_TemplateModel>, 
        { ProjectName = data.ProjectName
          ProjectGuid = data.ProjectGuid
          DefineConstants = data.DefineConstants |> Seq.ofList
          Includes = data.Includes |> Seq.ofList },
        new DynamicViewBag(data.AdditionalData |> dict))