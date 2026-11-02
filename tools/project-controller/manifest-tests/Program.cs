using AIControlTower.Services;
using AIControlTower.Models;
using System.Text.Json;
var root=Path.Combine(Path.GetTempPath(),"manifest-contract-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
int failures=0,passes=0;
void Check(string name,Action action){try{action();Console.WriteLine("PASS "+name);passes++;}catch(Exception e){Console.WriteLine("FAIL "+name+": "+e.Message);failures++;}}
void Require(bool ok,string message){if(!ok)throw new Exception(message);}
try{
 var manifest=Path.Combine(root,"project.control.json");
 File.WriteAllText(manifest,"""
 {"schemaVersion":1,"id":"Threads","name":"Owner project","functions":[
 {"id":"upload-studio-controls","name":"Review server","programs":[
 {"id":"start","name":"Start","path":".","workingDirectory":".","commands":[{"fileName":"fixture-never-run","arguments":["start"],"timeoutSeconds":90}]},
 {"id":"open","name":"Open","commands":[{"fileName":"fixture-never-run","arguments":["open"],"timeoutSeconds":90}]},
 {"id":"stop","name":"Stop","commands":[{"fileName":"fixture-never-run","arguments":["stop"],"timeoutSeconds":90}]},
 {"id":"status","name":"Status","commands":[],"control":{"status":{"fileName":"fixture-never-run","arguments":["status"],"timeoutSeconds":90},"fields":[{"path":"healthy","label":"Health"}],"options":[]}}]},
 {"id":"same","name":"Owner tools","programs":[{"id":"owner","name":"Owner program","description":"Owner semantics","commands":[{"fileName":"fixture-never-run","arguments":["old"],"timeoutSeconds":90}]}]}]}
 """);
 var before=File.ReadAllBytes(manifest);
 var discovery=new ProjectDiscoveryService();
 var service=new ProjectCatalogService();
 CatalogDefinition Catalog(bool preserve=false)=>new(){AnchorRoot=root,Projects=[new(){Id="Threads",Path=root,Name="Public name",PreserveManifest=preserve,Functions=[new(){Id="same",Name="Catalog tools",Programs=[new(){Id="owner",Name="Stale",Commands=[new(){FileName="missing-command",Arguments=["stale"],TimeoutSeconds=90}]}]}]}]};
 ProjectItem Read()=>discovery.ReadManifest(root,manifest);
 Check("installed manifest wins without preserve flag",()=>{var p=service.Apply([Read()],root,Catalog()).Single();Require(p.Functions.SelectMany(f=>f.Programs).Count()==5,"owner actions lost");});
 Check("preserve flag keeps every function",()=>{var p=service.Apply([Read()],root,Catalog(true)).Single();Require(p.Functions.Any(f=>f.Id=="upload-studio-controls"),"control function discarded");});
 Check("same IDs use owner commands and labels",()=>{var p=service.Apply([Read()],root,Catalog(true)).Single();var x=p.Functions.SelectMany(f=>f.Programs).Single(x=>x.Id=="Threads/owner");Require(x.Name=="Owner program" && x.Commands.Single().Arguments.Single()=="old","stale command replaced current command");});
 Check("discovery retains external status metadata",()=>{var x=Read().Functions.SelectMany(f=>f.Programs).Single(x=>x.Id=="Threads/status");var c=typeof(ProgramItem).GetProperty("Control")?.GetValue(x);Require(c is not null,"control.status dropped by discovery");Require(x.Commands.Count==1 && x.Commands[0].Arguments.Single()=="status","status action not mapped");});
 Check("fresh rescan recreates all four server actions",()=>{for(int i=0;i<3;i++){var p=service.Apply(discovery.Scan(root).Projects,root,Catalog()).Single();Require(p.Functions.Single(f=>f.Id=="upload-studio-controls").Programs.Count==4,"rescan lost server controls");}});
 Check("catalog does not mutate supplied manifest items",()=>{var p=Read();var b=JsonSerializer.Serialize(p);service.Apply([p],root,Catalog(true));Require(JsonSerializer.Serialize(p)==b,"discovered owner model mutated");});
 Check("related folders remain linked while manifest wins",()=>{var old=Directory.CreateDirectory(Path.Combine(root,"old")).FullName;var c=Catalog();c.Projects[0].RelatedFolders=[new(){Id="old",Path=old,Name="Archive"}];var p=service.Apply([Read()],root,c).Single();Require(p.Functions.Any(f=>f.Id=="related-folders") && p.Functions.Any(f=>f.Id=="upload-studio-controls"),"grouping lost manifest controls");});
 Check("planned catalog cannot rewrite owner program descriptions",()=>{var p=Read();var before=JsonSerializer.Serialize(p);var c=Catalog(true);c.Projects[0].Role="planned";service.Apply([p],root,c);Require(JsonSerializer.Serialize(p)==before,"planned presentation changed owner metadata");});
 Check("related folder cannot remove another registered project",()=>{var other=Directory.CreateDirectory(Path.Combine(root,"registered-other")).FullName;var registered=new ProjectItem{Id="Other",Name="Other",Path=other,ManifestPath=Path.Combine(other,"project.control.json"),Functions=[new(){Id="owned",Programs=[new(){Id="Other/start",Name="Start"}]}]};var c=Catalog();c.Projects[0].RelatedFolders=[new(){Id="old",Path=other,Name="Outdated archive label"}];var all=service.Apply([Read(),registered],root,c);Require(all.Any(x=>x.Id=="Other" && x.Functions.Any(f=>f.Id=="owned")),"related-folder grouping removed registered project");});
 Check("stale invalid catalog path cannot block valid owner manifest",()=>{var c=Catalog();c.Projects[0].Functions[0].Programs[0].Path=Path.GetTempPath();var p=service.Apply([Read()],root,c).Single();Require(p.Functions.Any(f=>f.Id=="upload-studio-controls"),"stale catalog path blocked owner manifest");});
 Check("owner files never rewritten and no command launched",()=>Require(File.ReadAllBytes(manifest).SequenceEqual(before),"manifest bytes changed"));
}finally{Directory.Delete(root,true);}
Console.WriteLine($"RESULT {passes} PASS / {failures} FAIL; service-only, native UI not run");
return failures==0?0:1;
