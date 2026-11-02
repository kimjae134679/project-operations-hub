import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {execFileSync} from 'node:child_process';
import {createHash} from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {validateCatalog} from './validate.mjs';
const hash=b=>createHash('sha256').update(b).digest('hex');
function fixture(fn){
 const base=path.join(path.dirname(fileURLToPath(import.meta.url)),'.local');fs.mkdirSync(base,{recursive:true});
 const p=fs.mkdtempSync(path.join(base,'complete-fixture-'));
 try{
  const make=id=>{
   const root=path.join(p,id);fs.mkdirSync(root);fs.mkdirSync(path.join(root,'.local'));
   const roles=Object.fromEntries(['source','operating','data','build','cache','backup'].map(r=>[r,path.join(root,r)]));for(const r of Object.values(roles))fs.mkdirSync(r);
   const git=args=>execFileSync('git',['-c','core.hooksPath='+path.join(root,'no-hooks'),'-c','commit.gpgsign=false','-c','user.name=Validator Test','-c','user.email=validator@example.invalid','-C',roles.source,...args],{encoding:'utf8',windowsHide:true,env:{...process.env,GIT_OPTIONAL_LOCKS:'0'},stdio:['ignore','pipe','pipe']}).trim();
   git(['init','--quiet','--initial-branch=main']);fs.writeFileSync(path.join(roles.source,'code'),'code');git(['add','code']);git(['commit','--quiet','-m','fixture']);git(['remote','add','origin','https://github.com/example/project.git']);const commit=git(['rev-parse','HEAD']);
   const reference='https://github.com/example/project/blob/'+commit+'/CONTRACT.md';
   fs.writeFileSync(path.join(roles.operating,'app'),'app');fs.writeFileSync(path.join(roles.data,'contract'),'contract');
   const status=path.join(roles.data,'status.json');fs.writeFileSync(status,JSON.stringify({checkedAt:new Date().toISOString(),activeJobs:0}));
   const i={owner:'review',status:'registered',roles,repository:'https://github.com/example/project.git',sourceRef:'main',sourceCommit:commit,receipt:'.local/project-controller.json',lockPaths:['.local/actual-lock'],runtimeStatus:{role:'data',path:'status.json',checkedAtField:'checkedAt',activeJobsField:'activeJobs'},contracts:[{owner:'review',reference,role:'data',path:'contract',sha256:hash('contract')}],expectedArtifacts:[{role:'operating',path:'app',sha256:hash('app'),sourceCommit:commit}]};
   const receipt={schemaVersion:1,projectId:id,owner:'review',sourceCommit:commit,checkedAt:new Date().toISOString(),complete:true,locks:[{path:'.local/actual-lock',held:false}],artifacts:[{role:'operating',path:'app',sha256:hash('app'),appliedSha256:hash('app')}],contracts:[{reference,sha256:hash('contract')}]};
   const save=()=>fs.writeFileSync(path.join(root,'.local/project-controller.json'),JSON.stringify(receipt));save();
   return {id,root,i,receipt,save,status,project:{id,path:root,preserveManifest:true,integration:i}};
  };
  const one=make('one'),two=make('two');const catalog={schemaVersion:1,controller:{schemaVersion:1,owner:'integration',manifestAuthority:'project.control.json',requiredRoles:Object.keys(one.i.roles)},projects:[one.project,two.project]};
  fn({one,two,catalog,run:opts=>validateCatalog(catalog,{local:true,...opts})});
 }finally{fs.rmSync(p,{recursive:true,force:true});}
}
test('real clean Git receipts and exact artifacts can complete',()=>fixture(({run})=>{const r=run();assert.equal(r.complete,2,JSON.stringify(r));}));
test('later project fresh status uses its actual start time after earlier delay',()=>fixture(({one,two,run})=>{
 const originalRead=fs.readFileSync,originalNow=Date.now;const epoch=Date.now();let clock=epoch,changed=false;
 try{Date.now=()=>clock;fs.readFileSync=function(file,...args){if(!changed&&path.resolve(String(file))===path.join(one.i.roles.operating,'app')){changed=true;clock+=20000;fs.writeFileSync(two.status,JSON.stringify({checkedAt:new Date(clock).toISOString(),activeJobs:0}));}return originalRead.call(fs,file,...args);};assert.equal(run({now:epoch}).projects[1].state,'complete');}finally{fs.readFileSync=originalRead;Date.now=originalNow;}
}));
test('later project old status cannot borrow catalog start freshness',()=>fixture(({one,run})=>{
 const originalRead=fs.readFileSync,originalNow=Date.now;const epoch=Date.now();let clock=epoch,changed=false;
 try{Date.now=()=>clock;fs.readFileSync=function(file,...args){if(!changed&&path.resolve(String(file))===path.join(one.i.roles.operating,'app')){changed=true;clock+=20000;}return originalRead.call(fs,file,...args);};const r=run({now:epoch}).projects[1];assert.equal(r.state,'blocked');assert.ok(r.issues.includes('runtime_not_verified_idle'));}finally{fs.readFileSync=originalRead;Date.now=originalNow;}
}));
test('operating artifact changed after initial hash cannot complete',()=>fixture(({one,run})=>{
 const app=path.join(one.i.roles.operating,'app'),aux=path.join(one.i.roles.operating,'aux');fs.writeFileSync(aux,'aux');one.receipt.artifacts.push({role:'operating',path:'aux',sha256:hash('aux'),appliedSha256:hash('aux')});one.save();
 const original=fs.readFileSync;try{fs.readFileSync=function(file,...args){if(path.resolve(String(file))===aux)fs.writeFileSync(app,'changed');return original.call(fs,file,...args);};const r=run().projects[0];assert.equal(r.state,'blocked');assert.ok(r.issues.includes('artifact_changed'));}finally{fs.readFileSync=original;}
}));
test('contract changed after initial read cannot complete',()=>fixture(({one,run})=>{
 const contract=path.join(one.i.roles.data,'contract'),original=fs.readFileSync;let changed=false;
 try{fs.readFileSync=function(file,...args){const data=original.call(fs,file,...args);if(!changed&&path.resolve(String(file))===contract){changed=true;fs.writeFileSync(contract,'changed');}return data;};const r=run().projects[0];assert.equal(r.state,'blocked');assert.ok(r.issues.includes('contract_changed'));}finally{fs.readFileSync=original;}
}));
