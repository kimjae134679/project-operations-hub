// Metadata only. No HTTP, writes, timers, producer jobs or Buffer calls.
import fs from 'node:fs/promises';
import path from 'node:path';
import {createHash} from 'node:crypto';
import {pathToFileURL,fileURLToPath} from 'node:url';
const fail=reason=>{throw Object.assign(new Error(reason),{reason});};
const sha=bytes=>createHash('sha256').update(bytes).digest('hex');
const count=(v,min=0,max=10000)=>{if(!Number.isSafeInteger(v)||v<min||v>max)fail('invalid_counts');return v;};
const list=v=>{if(!Array.isArray(v)||v.length>10000)fail('invalid_rows');return v;};
function inside(root,relative){
 if(!path.isAbsolute(root)||typeof relative!=='string'||path.isAbsolute(relative))fail('unsafe_path');
 const file=path.resolve(root,relative),rel=path.relative(root,file);
 if(!rel||rel==='..'||rel.startsWith('..'+path.sep)||path.isAbsolute(rel))fail('unsafe_path');return file;
}
async function safe(file){
 if(!path.isAbsolute(file)||file.startsWith('\\\\'))fail('unsafe_path');
 for(let p=file;;p=path.dirname(p)){
  const s=await fs.lstat(p);if(s.isSymbolicLink())fail('unsafe_path');
  if(p===path.dirname(p))break;
 }
}
async function read(file,max=20*1024*1024){
 await safe(file);const h=await fs.open(file,'r');
 try{const s=await h.stat();if(!s.isFile()||s.size>max)fail('source_size');
  const bytes=await h.readFile();if(bytes.length>max)fail('source_size');
  await safe(file);const data=JSON.parse(bytes.toString('utf8').replace(/^\uFEFF/,''));
  return {data,file,sourceSha256:sha(bytes),sourceUpdatedAt:s.mtime.toISOString()};
 }finally{await h.close();}
}
async function stable(sources){for(const s of sources)if((await read(s.file)).sourceSha256!==s.sourceSha256)fail('source_changed_during_read');}
async function guarded(empty,operation){try{return await operation();}catch(e){return {...empty,available:false,reason:e.reason||({'ENOENT':'source_missing','EACCES':'source_access_denied'}[e.code])||(e instanceof SyntaxError?'source_invalid_json':'source_invalid')};}}
async function production(root){return guarded({generated:null,total:null},async()=>{
 const s=await read(inside(root,'06_�ڵ� ���� ���/status.json')),d=s.data;
 if(d.schema!=='threads-auto-batch-v1')fail('production_schema');
 const rows=list(d.entries),ids=new Set();for(const r of rows){if(typeof r.id!=='string'||ids.has(r.id))fail('duplicate_production_id');ids.add(r.id);}
 await stable([s]);return {available:true,total:rows.length,generated:rows.filter(r=>r.outputFolder&&Array.isArray(r.images)&&r.images.length).length,
  needsSource:rows.filter(r=>r.status==='needs_source').length,waitingForImage:rows.filter(r=>r.status==='waiting_for_image').length,
  sourceUpdatedAt:s.sourceUpdatedAt,sourceSha256:s.sourceSha256};
 });}
async function review(config){return guarded({passed:null,unreviewed:null,revision:null},async()=>{
 const s=await read(inside(config.reviewRoot,'.local/state.json'));const d=s.data;
 if(d.schema!==1)fail('review_schema');count(d.revision);list(d.posts);
 const module=inside(config.reviewModuleRoot||config.reviewRoot,'review-handoff.mjs');await safe(module);
 // Reuse the owner's pure projection, including exact caption/order/version approval invalidation.
 const {reviewHandoffProjection}=await import(pathToFileURL(module));const p=reviewHandoffProjection(d);
 await stable([s]);return {available:true,revision:d.revision,total:d.posts.length,passed:p.eligible_post_ids.length,
  unreviewed:p.review_statuses.filter(r=>r.active&&r.decision==='unreviewed').length,
  revise:p.review_statuses.filter(r=>r.active&&r.decision==='revise').length,discard:p.review_statuses.filter(r=>r.active&&r.decision==='discard').length,
  sourceSha256:s.sourceSha256,sourceUpdatedAt:s.sourceUpdatedAt,handoffId:p.handoff_id};
 });}
async function buffer(root){return guarded({instagramScheduled:null,threadsScheduled:null,pairedScheduled:null,partialPosts:null,published:null},async()=>{
 const s=await read(inside(root,'.local/provider-delivery-log/verified-buffer-schedule-20261010.json')),d=s.data;
 if(d.schema!==1)fail('delivery_schema');const rows=list(d.results),seen=new Map(),providerIds=new Map();
 for(const r of rows){
  if(typeof r.postId!=='string'||typeof r.outputVersion!=='string'||!['instagram','threads'].includes(r.platform)||typeof r.providerPostId!=='string'||!r.providerPostId||!Number.isFinite(Date.parse(r.providerVerifiedAt)))fail('invalid_delivery_identity');
  const key=JSON.stringify([r.postId,r.platform]),provider=JSON.stringify([r.platform,r.providerPostId]);
  if(seen.has(key)&&JSON.stringify(seen.get(key))!==JSON.stringify(r)||providerIds.has(provider)&&providerIds.get(provider)!==key)fail('conflicting_delivery_identity');
  seen.set(key,r);providerIds.set(provider,key);
 }
 const unique=[...seen.values()],scheduled=unique.filter(r=>r.status==='scheduled'),pairs=new Map();
 for(const r of scheduled){const pair=pairs.get(r.postId)||[];pair.push(r);pairs.set(r.postId,pair);}
 const basis=r=>typeof r.fingerprint==='string'&&/^[a-f0-9]{64}$/.test(r.fingerprint);
 const paired=[...pairs.values()].filter(rs=>rs.length===2&&basis(rs[0])&&basis(rs[1])&&rs[0].outputVersion===rs[1].outputVersion&&rs[0].fingerprint===rs[1].fingerprint).length;
 await stable([s]);return {available:true,instagramScheduled:scheduled.filter(r=>r.platform==='instagram').length,
  threadsScheduled:scheduled.filter(r=>r.platform==='threads').length,pairedScheduled:paired,partialPosts:pairs.size-paired,
  published:unique.filter(r=>r.status==='sent').length,instagramErrors:unique.filter(r=>r.platform==='instagram'&&r.status==='error').length,
  threadsErrors:unique.filter(r=>r.platform==='threads'&&r.status==='error').length,missingBasis:unique.filter(r=>!basis(r)).length,
  unknownStatuses:unique.filter(r=>!['draft','needs_approval','scheduled','sending','sent','error'].includes(r.status)).length,
  waitingPlatformTasks:list(d.waiting||[]).length,recordedAt:typeof d.recordedAt==='string'?d.recordedAt:null,
  sourceSha256:s.sourceSha256,sourceUpdatedAt:s.sourceUpdatedAt,scope:'stored provider observations; not a live queue read'};
 });}
async function voice(config){
 let listenerExists=null;
 try{const file=inside(config.root,config.listener);await safe(file);listenerExists=(await fs.stat(file)).isFile();}catch(e){listenerExists=e.code==='ENOENT'?false:null;}
 return guarded({completed:null,total:null,listenerExists},async()=>{
  const latestFile=inside(config.root,'audiobooks/library_latest.json');
  let latest=null;try{latest=await read(latestFile,8192);}catch(e){if(e.code!=='ENOENT')throw e;}
  if(latest){
   const p=latest.data;if(typeof p.edition!=='string'||!/^production_[A-Za-z0-9_-]{1,100}$/.test(p.edition)||typeof p.reader!=='string')fail('voice_pointer_invalid');
   const expected=inside(config.root,'output/'+p.edition+'/00_���۸��.html');
   if(path.resolve(p.reader)!==expected)fail('voice_pointer_invalid');await safe(expected);
   const state=await read(inside(config.root,'output/'+p.edition+'/status.json'),1024*1024),d=state.data;
   if(d.registration_only!==true||!d.chapters||typeof d.chapters!=='object'||Array.isArray(d.chapters)||Object.keys(d.chapters).length>10000)fail('voice_registration_schema');
   await stable([latest,state]);return {available:true,edition:p.edition,registeredReady:Object.values(d.chapters).filter(c=>c.status==='ready').length,
    completed:null,total:null,stage:'registration_only',listenerExists,sourceSha256:state.sourceSha256,sourceUpdatedAt:state.sourceUpdatedAt,
    scope:'latest listening registration; no production denominator or worker liveness'};
  }
  const pointer=await read(inside(config.root,'output/production_active.json'),8192),p=pointer.data;
  if(typeof p.edition!=='string'||!/^production_[A-Za-z0-9_-]{1,100}$/.test(p.edition)||p.run_relative!=='output/'+p.edition)fail('voice_pointer_invalid');
  const state=await read(inside(config.root,p.run_relative+'/status.json'),1024*1024),d=state.data;
  if(d.edition!==p.edition)fail('voice_edition_mismatch');const total=count(d.total,1),completed=count(d.completed,0,total);
  if(!d.chapters||typeof d.chapters!=='object'||Array.isArray(d.chapters)||Object.keys(d.chapters).length>total)fail('voice_chapters_invalid');
  if(Object.values(d.chapters).filter(c=>c.status==='ready').length!==completed)fail('voice_count_mismatch');
  await stable([pointer,state]);return {available:true,edition:p.edition,completed,total,stage:typeof d.stage==='string'?d.stage:'unknown',listenerExists,
   sourceSha256:state.sourceSha256,sourceUpdatedAt:state.sourceUpdatedAt,scope:'production record; not worker liveness'};
 });
}
export async function readStatus(registry){
 if(registry.schemaVersion!==1)fail('registry_schema');
 const [produced,reviewed,delivered,listening]=await Promise.all([production(registry.threads.materialRoot),review(registry.threads),buffer(registry.threads.reviewRoot),voice(registry.voice)]);
 return {schemaVersion:1,checkedAt:new Date().toISOString(),threads:{production:produced,review:reviewed,buffer:delivered},voice:listening};
}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
 try{if(process.argv.length!==3)fail('registry_argument');const registry=await read(path.resolve(process.argv[2]),1024*1024);console.log(JSON.stringify(await readStatus(registry.data)));}
 catch(e){console.log(JSON.stringify({schemaVersion:1,checkedAt:new Date().toISOString(),error:e.reason||'registry_unavailable'}));process.exitCode=1;}
}
