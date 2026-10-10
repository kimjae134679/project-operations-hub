import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {tmpdir} from 'node:os';
import {createHash} from 'node:crypto';
import {validateCatalog} from './validate.mjs';
const hash=b=>createHash('sha256').update(b).digest('hex');
function fixture(fn){
 const p=fs.mkdtempSync(path.join(tmpdir(),'controller-dependencies-'));
 try{
  const roles=Object.fromEntries(['source','operating','data','build','cache','backup'].map(r=>[r,path.join(p,r)]));
  for(const r of Object.values(roles))fs.mkdirSync(r);fs.mkdirSync(path.join(p,'.local'));fs.mkdirSync(path.join(roles.operating,'.local'));
  const i={owner:'review',status:'registered',roles,repository:'https://github.com/example/project.git',receipt:'.local/project-controller.json',contracts:[],lockPaths:[{role:'operating',path:'.local/.state-lock'}]};
  const c={schemaVersion:1,controller:{schemaVersion:1,owner:'integration',manifestAuthority:'project.control.json',requiredRoles:Object.keys(roles)},projects:[{id:'P',path:p,preserveManifest:true,integration:i}]};
  const receipt={schemaVersion:1,projectId:'P',owner:'review',checkedAt:new Date().toISOString(),complete:false,locks:[{role:'operating',path:'.local/.state-lock',held:false}],artifacts:[],contracts:[],dependencies:[]};
  const save=()=>fs.writeFileSync(path.join(p,'.local/project-controller.json'),JSON.stringify(receipt));save();
  fn({p,c,i,receipt,save,run:()=>validateCatalog(c,{local:true}).projects[0]});
 }finally{fs.rmSync(p,{recursive:true,force:true});}
}
test('actual operating-role lock blocks even when project-root lock is absent',()=>fixture(({i,run})=>{
 fs.writeFileSync(path.join(i.roles.operating,'.local/.state-lock'),'held');
 assert.ok(run().issues.includes('lock_held'));
}));
test('operating lock pin requires receipt with identical role',()=>fixture(({receipt,save,run})=>{
 receipt.locks[0].role='data';save();assert.ok(run().issues.includes('locks_unpinned'));
}));
test('malformed null lock receipt is refused without throwing',()=>fixture(({receipt,save,run})=>{
 receipt.locks.push(null);save();assert.ok(run().issues.includes('lock_invalid'));
}));
test('matching absent operating-role lock is a valid pinned lock observation',()=>fixture(({run})=>{
 const issues=run().issues;assert.ok(!issues.includes('locks_unpinned'));assert.ok(!issues.includes('lock_invalid'));
}));
test('dependency traversal and unknown role are rejected',()=>fixture(({i,run})=>{
 i.dependencyArtifacts=[{owner:'producer',role:'data',path:'../other.json',sha256:hash('pinned')},{owner:'producer',role:'other',path:'delivery.json',sha256:hash('pinned')}];
 assert.ok(run().issues.includes('dependency_invalid'));
}));
test('operating role lock starting during artifact read is detected again',()=>fixture(({i,receipt,save,run})=>{
 const app=path.join(i.roles.operating,'app');receipt.artifacts=[{role:'operating',path:'app',sha256:hash('app'),appliedSha256:hash('app')}];save();fs.writeFileSync(app,'app');
 const original=fs.readFileSync;try{fs.readFileSync=function(file,...args){if(path.resolve(String(file))===app)fs.writeFileSync(path.join(i.roles.operating,'.local/.state-lock'),'held');return original.call(fs,file,...args);};assert.ok(run().issues.includes('lock_held'));}finally{fs.readFileSync=original;}
}));
test('matching pinned dependency bytes and owner receipt have no dependency error',()=>fixture(({i,receipt,save,run})=>{
 i.dependencyArtifacts=[{owner:'producer',role:'data',path:'delivery.json',sha256:hash('pinned')}];receipt.dependencies=[{...i.dependencyArtifacts[0]}];save();fs.writeFileSync(path.join(i.roles.data,'delivery.json'),'pinned');assert.ok(!run().issues.some(x=>x.startsWith('dependency_')));
}));
test('dependency pin detects different actual producer bytes',()=>fixture(({i,receipt,save,run})=>{
 i.dependencyArtifacts=[{owner:'producer',role:'data',path:'delivery.json',sha256:hash('pinned')}];
 receipt.dependencies=[{...i.dependencyArtifacts[0]}];save();fs.writeFileSync(path.join(i.roles.data,'delivery.json'),'changed');
 assert.ok(run().issues.includes('dependency_hash_mismatch'));
}));
test('dependency bytes alone cannot replace owner receipt acknowledgement',()=>fixture(({i,run})=>{
 i.dependencyArtifacts=[{owner:'producer',role:'data',path:'delivery.json',sha256:hash('pinned')}];
 fs.writeFileSync(path.join(i.roles.data,'delivery.json'),'pinned');assert.ok(run().issues.includes('dependency_receipt_missing'));
}));
test('producer dependency modified during operating artifact read is rejected',()=>fixture(({i,receipt,save,run})=>{
 const dep=path.join(i.roles.data,'delivery.json'),app=path.join(i.roles.operating,'app');
 i.dependencyArtifacts=[{owner:'producer',role:'data',path:'delivery.json',sha256:hash('pinned')}];receipt.dependencies=[{...i.dependencyArtifacts[0]}];
 receipt.artifacts=[{role:'operating',path:'app',sha256:hash('app'),appliedSha256:hash('app')}];save();fs.writeFileSync(dep,'pinned');fs.writeFileSync(app,'app');
 const original=fs.readFileSync;try{fs.readFileSync=function(file,...args){if(path.resolve(String(file))===app)fs.writeFileSync(dep,'changed');return original.call(fs,file,...args);};assert.ok(run().issues.includes('dependency_changed'));}finally{fs.readFileSync=original;}
}));
