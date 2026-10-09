import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import {createHash} from 'node:crypto';
import {pathToFileURL} from 'node:url';
import {execFile} from 'node:child_process';
import {promisify} from 'node:util';
import {readStatus} from './reader.mjs';
const studio=process.env.TEST_STUDIO_ROOT;
const {createState,importBundle,setFinalReview}=await import(pathToFileURL(path.join(studio,'domain.mjs')));
async function fixture(t){
 const root=await fs.mkdtemp(path.join(os.tmpdir(),'external-state-'));
 t.after(()=>fs.rm(root,{recursive:true,force:true}));
 const review=path.join(root,'review'),material=path.join(root,'material'),voice=path.join(root,'voice');
 await fs.mkdir(path.join(review,'.local','provider-delivery-log'),{recursive:true});
 await fs.mkdir(path.join(material,'06_자동 제작 결과'),{recursive:true});
 await fs.mkdir(path.join(voice,'output','production_test'),{recursive:true});
 await fs.mkdir(path.join(voice,'audiobooks','01_CURRENT'),{recursive:true});
 const put=(file,value)=>fs.writeFile(file,JSON.stringify(value));
 const productionFile=path.join(material,'06_자동 제작 결과','status.json');
 await put(productionFile,{schema:'threads-auto-batch-v1',entries:[{id:'example-1',status:'generated',outputFolder:'one',images:[{sha256:'a'.repeat(64)}]},{id:'example-2',status:'needs_source',images:[]}]});
 const reviewFile=path.join(review,'.local','state.json');
 let state=importBundle(createState(),{bundle_id:'fixture',posts:[{post_id:'example-1',output_version:'v1',caption:'fixture only',source:{url:'https://example.invalid/post',label:'fixture'},images:[{asset_id:'a'.repeat(64),order:1,mime:'image/png'}]}]});
 state=setFinalReview(state,'example-1','passed',state.posts[0].revision,'2026-10-09T10:00:00Z');
 await put(reviewFile,state);
 const deliveryFile=path.join(review,'.local','provider-delivery-log','verified-buffer-schedule-20261010.json');
 await put(deliveryFile,{schema:1,results:[{postId:'example-1',outputVersion:'v1',platform:'instagram',providerPostId:'provider-example',status:'scheduled',providerVerifiedAt:'2026-10-09T11:00:00Z'}],waiting:[{postId:'example-1',platform:'threads',reason:'queue_capacity'}]});
 await put(path.join(voice,'output','production_active.json'),{edition:'production_test',run_relative:'output/production_test'});
 await put(path.join(voice,'output','production_test','status.json'),{edition:'production_test',completed:1,total:2,stage:'paused',updated_at:1791459235,chapters:{'1':{status:'ready'},'2':{status:'failed'}}});
 await fs.writeFile(path.join(voice,'audiobooks','01_CURRENT','무직전생 듣기.html'),'fixture');
 const registry={schemaVersion:1,threads:{materialRoot:material,reviewRoot:review,reviewModuleRoot:studio},voice:{root:voice,listener:'audiobooks/01_CURRENT/무직전생 듣기.html'},projects:[]};
 return {root,registry,put,state,productionFile,reviewFile,deliveryFile};
}
test('re-reads changed producer and review files with unchanged source bytes',async t=>{
 const f=await fixture(t),before=await fs.readFile(f.reviewFile);
 const first=await readStatus(f.registry);
 assert.equal(first.threads.production.generated,1);assert.equal(first.threads.review.passed,1);
 assert.equal(first.threads.buffer.instagramScheduled,1);assert.equal(first.threads.buffer.threadsScheduled,0);
 assert.equal(first.threads.buffer.pairedScheduled,0);assert.equal(first.threads.buffer.partialPosts,1);
 assert.deepEqual(await fs.readFile(f.reviewFile),before);
 f.state.posts[0].caption='changed after pass';f.state.revision++;
 await f.put(f.reviewFile,f.state);
 await f.put(f.productionFile,{schema:'threads-auto-batch-v1',entries:[]});
 const after=await fs.readFile(f.reviewFile),second=await readStatus(f.registry);
 assert.equal(second.threads.production.generated,0);assert.equal(second.threads.review.passed,0);
 assert.equal(second.threads.review.unreviewed,1);assert.notEqual(first.threads.review.sourceSha256,second.threads.review.sourceSha256);
 assert.deepEqual(await fs.readFile(f.reviewFile),after);
});
test('missing and corrupt delivery is unknown rather than an empty successful queue',async t=>{
 const f=await fixture(t);await fs.unlink(f.deliveryFile);
 let s=await readStatus(f.registry);assert.equal(s.threads.buffer.available,false);assert.equal(s.threads.buffer.instagramScheduled,null);
 await fs.writeFile(f.deliveryFile,'broken');s=await readStatus(f.registry);assert.equal(s.threads.buffer.available,false);assert.equal(s.threads.review.available,true);
});
test('conflicting provider entries cannot count as two accepted tasks',async t=>{
 const f=await fixture(t);const r=JSON.parse(await fs.readFile(f.deliveryFile));r.results.push({...r.results[0],providerPostId:'different'});await f.put(f.deliveryFile,r);
 const s=await readStatus(f.registry);assert.equal(s.threads.buffer.available,false);assert.equal(s.threads.buffer.reason,'conflicting_delivery_identity');
});
test('matching versions with missing fingerprints cannot prove an approved two-platform pair',async t=>{
 const f=await fixture(t);const r=JSON.parse(await fs.readFile(f.deliveryFile));r.results.push({...r.results[0],platform:'threads',providerPostId:'thread-provider'});await f.put(f.deliveryFile,r);
 const s=await readStatus(f.registry);assert.equal(s.threads.buffer.pairedScheduled,0);assert.equal(s.threads.buffer.partialPosts,1);assert.equal(s.threads.buffer.missingBasis,2);
});
test('provider errors remain visible independently for both platforms',async t=>{
 const f=await fixture(t);const r=JSON.parse(await fs.readFile(f.deliveryFile));r.results[0].status='error';r.results.push({...r.results[0],platform:'threads',providerPostId:'thread-provider'});await f.put(f.deliveryFile,r);
 const s=await readStatus(f.registry);assert.equal(s.threads.buffer.instagramErrors,1);assert.equal(s.threads.buffer.threadsErrors,1);assert.equal(s.threads.buffer.published,0);
});
test('voice pointer traversal is refused while listening entry remains separate',async t=>{
 const f=await fixture(t);const p=path.join(f.registry.voice.root,'output','production_active.json');
 let s=await readStatus(f.registry);assert.equal(s.voice.completed,1);assert.equal(s.voice.listenerExists,true);assert.equal(s.voice.stage,'paused');
 await f.put(p,{edition:'production_test',run_relative:'output/../outside'});s=await readStatus(f.registry);assert.equal(s.voice.available,false);assert.equal(s.voice.completed,null);assert.equal(s.voice.listenerExists,true);
});
test('latest listening registration supersedes old batch counts without claiming production completion',async t=>{
 const f=await fixture(t),root=f.registry.voice.root;
 const folder=path.join(root,'output','production_latest');await fs.mkdir(folder);
 await f.put(path.join(folder,'status.json'),{registration_only:true,chapters:{'157':{status:'ready'},'158':{status:'ready'}}});
 await fs.writeFile(path.join(folder,'00_제작목록.html'),'fixture');
 await f.put(path.join(root,'audiobooks','library_latest.json'),{edition:'production_latest',reader:path.join(folder,'00_제작목록.html')});
 const s=await readStatus(f.registry);assert.equal(s.voice.edition,'production_latest');assert.equal(s.voice.registeredReady,2);
 assert.equal(s.voice.completed,null);assert.equal(s.voice.total,null);assert.equal(s.voice.stage,'registration_only');
 await f.put(path.join(root,'audiobooks','library_latest.json'),{edition:'production_latest',reader:path.join(f.root,'outside.html')});
 const bad=await readStatus(f.registry);assert.equal(bad.voice.available,false);assert.equal(bad.voice.completed,null);
});
test('source symlinks are held without reading outside the registered root',async t=>{
 const f=await fixture(t);const elsewhere=path.join(f.root,'outside.json');await fs.writeFile(elsewhere,await fs.readFile(f.productionFile));await fs.unlink(f.productionFile);await fs.symlink(elsewhere,f.productionFile);
 const s=await readStatus(f.registry);assert.equal(s.threads.production.available,false);assert.equal(s.threads.production.reason,'unsafe_path');
});
test('unknown delivery statuses stay visible and scheduled is never published',async t=>{
 const f=await fixture(t);const r=JSON.parse(await fs.readFile(f.deliveryFile));r.results.push({postId:'example-2',outputVersion:'v2',platform:'threads',providerPostId:'unknown',status:'future-status',providerVerifiedAt:'2026-10-09T11:00:00Z'});await f.put(f.deliveryFile,r);
 const s=await readStatus(f.registry);assert.equal(s.threads.buffer.published,0);assert.equal(s.threads.buffer.unknownStatuses,1);assert.equal(s.threads.buffer.instagramScheduled,1);
 assert.equal(JSON.stringify(s).includes('fixture only'),false);assert.equal(JSON.stringify(s).includes('provider-example'),false);
});
test('finite CLI output re-reads files and supplies the unchanged hub JSON consumer',async t=>{
 const f=await fixture(t),registry=path.join(f.root,'registry.json');await f.put(registry,f.registry);
 const run=()=>promisify(execFile)(process.execPath,[new URL('./reader.mjs',import.meta.url).pathname.replace(/^\/([A-Za-z]:)/,'$1'),registry],{windowsHide:true,timeout:15000});
 const first=JSON.parse((await run()).stdout);assert.equal(first.threads.production.generated,1);
 await f.put(f.productionFile,{schema:'threads-auto-batch-v1',entries:[]});
 const second=JSON.parse((await run()).stdout);assert.equal(second.threads.production.generated,0);
 assert.equal(first.schemaVersion,1);assert.equal(second.schemaVersion,1);assert.equal(second.checkedAt.endsWith('Z'),true);
 const proof=process.env.TEST_PROOF_DIRECTORY;if(proof){await fs.writeFile(path.join(proof,'fixture-before.json'),JSON.stringify(first));await fs.writeFile(path.join(proof,'fixture-after.json'),JSON.stringify(second));}
});
