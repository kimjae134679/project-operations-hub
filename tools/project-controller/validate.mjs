import fs from 'node:fs';
import path from 'node:path';
import { createHash } from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const roles = ['source','operating','data','build','cache','backup'];
const norm = p => /^[A-Za-z]:[\\/]/.test(p) ? path.win32.normalize(p).toLowerCase() : path.resolve(p);
const temporary = p => /(?:[\\/]Documents[\\/]Codex[\\/]|[\\/]AppData[\\/]Local[\\/]Temp[\\/]|[\\/]tmp[\\/])/i.test(p);
const sha = b => createHash('sha256').update(b).digest('hex');
const hex = v => typeof v === 'string' && /^[a-f0-9]{64}$/.test(v);
const text = v => typeof v === 'string' && v.length > 0;
const relative = p => text(p) && !path.posix.isAbsolute(p) && !path.win32.isAbsolute(p) && !p.split(/[\\/]/).includes('..');
function regular(p, max = 256 * 1024) {
  const absolute = path.resolve(p);
  // Refuse links/reparse dependencies rather than following them to unrelated data.
  for (let cur = absolute; ; cur = path.dirname(cur)) {
    if (fs.lstatSync(cur).isSymbolicLink()) throw new Error('linked_path');
    if (path.dirname(cur) === cur) break;
  }
  const s = fs.statSync(absolute);
  if (!s.isFile() || s.size > max) throw new Error('invalid_file');
  return fs.readFileSync(absolute);
}
function localProject(project, integration, issues, now) {
  const started=Date.now();
  const lockPath = spec => {
    const root=typeof spec==='string'?project.path:roles.includes(spec?.role)?integration.roles[spec.role]:null;
    const name=typeof spec==='string'?spec:spec?.path;
    return text(root)&&relative(name)?path.join(root,name):null;
  };
  const lockExists = file => {try { fs.lstatSync(file); return true; } catch(e) {return e.code==='ENOENT'?false:true;} };
  const dependencies=[];
  const evidenceFiles=[];
  let sourceHead;
  let runtimeBytes, revisionBytes;
  for (const [role, p] of Object.entries(integration.roles ?? {})) {
    if (!text(p)) continue;
    try {
      if (!fs.statSync(p).isDirectory() || norm(fs.realpathSync(p)) !== norm(p)) issues.push('role_invalid:' + role);
    } catch { issues.push('role_missing_on_pc:' + role); }
  }
  try {
    const source = integration.roles.source;
    const git = args => execFileSync('git', ['-c','core.fsmonitor=false','-C', source, ...args], {
      encoding:'utf8', timeout:5000, maxBuffer:256*1024, windowsHide:true,
      env:{...process.env,GIT_OPTIONAL_LOCKS:'0'}, stdio:['ignore','pipe','ignore']
    }).trim();
    if (norm(git(['rev-parse','--show-toplevel'])) !== norm(source)) issues.push('source_not_git_root');
    if (git(['remote','get-url','origin']).replace(/\.git$/, '') !== integration.repository?.replace(/\.git$/, '')) issues.push('source_repository_mismatch');
    sourceHead = git(['rev-parse','HEAD']);
    if (integration.sourceCommit && sourceHead !== integration.sourceCommit) issues.push('source_commit_mismatch');
    if (integration.sourceRef && git(['branch','--show-current']) !== integration.sourceRef) issues.push('source_ref_mismatch');
    if (git(['status','--porcelain','--untracked-files=no'])) issues.push('source_has_uncommitted_changes');
  } catch { issues.push('source_git_unverified'); }
  if (!relative(integration.receipt)) return;
  const receiptPath = path.join(project.path, integration.receipt);
  let bytes, receipt;
  try { bytes = regular(receiptPath); receipt = JSON.parse(bytes.toString('utf8').replace(/^\uFEFF/,'')); }
  catch { issues.push('receipt_missing'); return; }
  if (!receipt || typeof receipt !== 'object' || !Array.isArray(receipt.locks) || receipt.locks.length > 10
    || !Array.isArray(receipt.artifacts) || receipt.artifacts.length > 1000
    || !Array.isArray(receipt.contracts) || receipt.contracts.length > 20) {
    issues.push('receipt_invalid'); return;
  }
  if (receipt.schemaVersion !== 1 || receipt.projectId !== project.id || receipt.owner !== integration.owner) issues.push('receipt_owner_mismatch');
  if (!sourceHead || receipt.sourceCommit !== sourceHead) issues.push('receipt_source_commit_mismatch');
  if (!/^[a-f0-9]{40}$/.test(integration.sourceCommit ?? '')) issues.push('source_commit_unpinned');
  const date = Date.parse(receipt.checkedAt);
  if (!Number.isFinite(date) || now-date > 15*60*1000 || date-now > 5000) issues.push('receipt_stale');
  if (receipt.complete !== true) issues.push('owner_not_complete');
  if (!Array.isArray(integration.lockPaths) || !integration.lockPaths.length
    || integration.lockPaths.some(pin=>!lockPath(pin))
    || integration.lockPaths.some(pin=>!receipt.locks.some(l=>l?.path===(typeof pin==='string'?pin:pin.path)
      && l.role===(typeof pin==='string'?undefined:pin.role)))) issues.push('locks_unpinned');
  if (!receipt.locks.length) issues.push('locks_unverified');
  else for (const lock of receipt.locks) {
    const file=lockPath(lock?.role===undefined?lock?.path:lock);
    if (!file || typeof lock?.held !== 'boolean') { issues.push('lock_invalid'); continue; }
    if (lock.held || lockExists(file)) issues.push('lock_held');
  }
  if (integration.dependencyArtifacts!==undefined) {
    if (!Array.isArray(integration.dependencyArtifacts)||!integration.dependencyArtifacts.length||integration.dependencyArtifacts.length>20) issues.push('dependency_invalid');
    else for(const dep of integration.dependencyArtifacts) {
      if(!text(dep?.owner)||!roles.includes(dep.role)||!relative(dep.path)||!hex(dep.sha256)
        || dep.bytes!==undefined&&(!Number.isSafeInteger(dep.bytes)||dep.bytes<0||dep.bytes>32*1024*1024)) {issues.push('dependency_invalid');continue;}
      if(!Array.isArray(receipt.dependencies)||!receipt.dependencies.some(r=>r?.owner===dep.owner&&r.role===dep.role&&r.path===dep.path&&r.sha256===dep.sha256)) issues.push('dependency_receipt_missing');
      try {
        const file=path.join(integration.roles[dep.role],dep.path),data=regular(file,32*1024*1024);
        if(sha(data)!==dep.sha256||dep.bytes!==undefined&&data.length!==dep.bytes)issues.push('dependency_hash_mismatch');
        dependencies.push({file,hash:sha(data)});
      }catch{issues.push('dependency_unreadable');}
    }
  }
  if (receipt.revisionBefore !== undefined || receipt.revisionAfter !== undefined) {
    if (!Number.isSafeInteger(receipt.revisionBefore) || receipt.revisionBefore !== receipt.revisionAfter) issues.push('revision_changed');
  }
  if (integration.requiresRevision === true) {
    if (!Number.isSafeInteger(receipt.revisionBefore) || receipt.revisionBefore !== receipt.revisionAfter) issues.push('revision_unverified');
    const rs=integration.revisionStatus;
    if (!rs || !roles.includes(rs.role) || !relative(rs.path) || !text(rs.field)) issues.push('revision_source_unpinned');
    else try {
      revisionBytes=regular(path.join(integration.roles[rs.role],rs.path),32*1024*1024);
      const revision=JSON.parse(revisionBytes.toString('utf8').replace(/^\uFEFF/,''))[rs.field];
      if (!Number.isSafeInteger(revision) || revision!==receipt.revisionAfter) issues.push('actual_revision_mismatch');
    } catch {issues.push('actual_revision_unreadable');}
  }
  const idle=integration.runtimeStatus;
  if (!idle || !roles.includes(idle.role) || !relative(idle.path)
    || !text(idle.checkedAtField) || !text(idle.activeJobsField)) issues.push('idle_evidence_unpinned');
  else {
    try {
      runtimeBytes=regular(path.join(integration.roles[idle.role],idle.path));
      const status=JSON.parse(runtimeBytes.toString('utf8').replace(/^\uFEFF/,''));
      const stamp=Date.parse(status[idle.checkedAtField]);
      if (!Number.isFinite(stamp)||now-stamp>15000||stamp-now>5000||status[idle.activeJobsField]!==0) issues.push('runtime_not_verified_idle');
    } catch { issues.push('runtime_idle_unreadable'); }
  }
  if (!Array.isArray(receipt.artifacts) || !receipt.artifacts.length) issues.push('artifacts_unverified');
  else for (const a of receipt.artifacts) {
    if (!roles.includes(a?.role) || !relative(a.path) || !hex(a.sha256)) { issues.push('artifact_invalid'); continue; }
    if (a.appliedSha256 !== undefined && a.appliedSha256 !== a.sha256) issues.push('applied_bytes_mismatch');
    if (a.role === 'operating' && !hex(a.appliedSha256)) issues.push('applied_bytes_unverified');
    try {
      const file=path.join(integration.roles[a.role],a.path),data=regular(file,128*1024*1024);
      const hash=sha(data);evidenceFiles.push({file,hash,max:128*1024*1024,issue:'artifact_changed'});
      if (hash !== a.sha256) issues.push('artifact_hash_mismatch');
    } catch { issues.push('artifact_unreadable'); }
  }
  if (!receipt.artifacts.some(a=>a?.role==='operating' && hex(a.sha256) && a.appliedSha256===a.sha256)) issues.push('operating_apply_unverified');
  if (!Array.isArray(integration.expectedArtifacts) || !integration.expectedArtifacts.length) issues.push('apply_target_unpinned');
  else for(const expected of integration.expectedArtifacts) {
    if (!expected || expected.role!=='operating' || !relative(expected.path) || !hex(expected.sha256)
      || expected.sourceCommit!==integration.sourceCommit
      || !receipt.artifacts.some(a=>a?.role===expected.role&&a.path===expected.path&&a.sha256===expected.sha256&&a.appliedSha256===expected.sha256)) issues.push('apply_target_mismatch');
  }
  for (const c of integration.contracts ?? []) {
    if (!hex(c.sha256) || !roles.includes(c.role) || !relative(c.path)
      || !receipt.contracts.some(r=>r?.reference===c.reference && r.sha256===c.sha256)) issues.push('contract_unverified');
    else try {const file=path.join(integration.roles[c.role],c.path),data=regular(file),hash=sha(data);evidenceFiles.push({file,hash,max:256*1024,issue:'contract_changed'});if(hash!==c.sha256)issues.push('contract_hash_mismatch');}
    catch {issues.push('contract_unreadable');}
  }
  // Recheck admission evidence after all potentially lengthy reads, using the end time.
  const endNow=now+(Date.now()-started);
  for(const lock of receipt.locks) {const file=lockPath(lock?.role===undefined?lock?.path:lock);if(file&&lockExists(file))issues.push('lock_held');}
  if(runtimeBytes) try {
    const endBytes=regular(path.join(integration.roles[idle.role],idle.path));
    const status=JSON.parse(endBytes.toString('utf8').replace(/^\uFEFF/,''));
    const stamp=Date.parse(status[idle.checkedAtField]);
    if(sha(endBytes)!==sha(runtimeBytes))issues.push('runtime_status_changed');
    if(!Number.isFinite(stamp)||endNow-stamp>15000||stamp-endNow>5000||status[idle.activeJobsField]!==0)issues.push('runtime_not_verified_idle');
  }catch{issues.push('runtime_status_changed');}
  if(revisionBytes) try {
    if(sha(regular(path.join(integration.roles[integration.revisionStatus.role],integration.revisionStatus.path),32*1024*1024))!==sha(revisionBytes))issues.push('actual_revision_changed');
  }catch{issues.push('actual_revision_changed');}
  if(sourceHead) try {
    const git=args=>execFileSync('git',['-c','core.fsmonitor=false','-C',integration.roles.source,...args],{encoding:'utf8',timeout:5000,maxBuffer:256*1024,windowsHide:true,env:{...process.env,GIT_OPTIONAL_LOCKS:'0'},stdio:['ignore','pipe','ignore']}).trim();
    if(git(['rev-parse','HEAD'])!==sourceHead || git(['status','--porcelain','--untracked-files=no']))issues.push('source_changed_during_validation');
  }catch{issues.push('source_changed_during_validation');}
  // Capture races without writing a lock or modifying an owner's receipt.
  try { if (sha(regular(receiptPath)) !== sha(bytes)) issues.push('receipt_changed'); }
  catch { issues.push('receipt_changed'); }
  for(const dep of dependencies) try {if(sha(regular(dep.file,32*1024*1024))!==dep.hash)issues.push('dependency_changed');}catch{issues.push('dependency_changed');}
  for(const e of evidenceFiles) try {if(sha(regular(e.file,e.max))!==e.hash)issues.push(e.issue);}catch{issues.push(e.issue);}
  try {if(sha(regular(receiptPath))!==sha(bytes))issues.push('receipt_changed');}catch{issues.push('receipt_changed');}
  // Git rechecks can take time too; the final idle check uses the actual finish time.
  if(runtimeBytes) try {
    const finalBytes=regular(path.join(integration.roles[idle.role],idle.path));
    const status=JSON.parse(finalBytes.toString('utf8').replace(/^\uFEFF/,''));
    const stamp=Date.parse(status[idle.checkedAtField]), finishNow=now+(Date.now()-started);
    if(sha(finalBytes)!==sha(runtimeBytes))issues.push('runtime_status_changed');
    if(!Number.isFinite(stamp)||finishNow-stamp>15000||stamp-finishNow>5000||status[idle.activeJobsField]!==0)issues.push('runtime_not_verified_idle');
  }catch{issues.push('runtime_status_changed');}
  for(const lock of receipt.locks) {const file=lockPath(lock?.role===undefined?lock?.path:lock);if(file&&lockExists(file))issues.push('lock_held');}
}
export function validateCatalog(catalog, {local=false, now=Date.now()}={}) {
  const catalogStarted=Date.now();
  const errors=[], projects=[];
  if (!catalog || catalog.schemaVersion !== 1 || !Array.isArray(catalog.projects))
    return {schemaVersion:1,errors:['invalid_catalog'],total:0,registered:0,complete:0,projects};
  const c=catalog.controller;
  if (c?.schemaVersion !== 1 || c.owner !== 'integration' || c.manifestAuthority !== 'project.control.json'
    || !Array.isArray(c.requiredRoles) || c.requiredRoles.length!==roles.length || roles.some(r=>!c.requiredRoles.includes(r))) errors.push('invalid_controller_contract');
  const ids=new Set(), paths=new Set();
  for (const p of catalog.projects) {
    const issues=[];
    if (!text(p?.id) || !text(p?.path)) { errors.push('invalid_project'); continue; }
    if(ids.has(p.id.toLowerCase())) errors.push('duplicate_project_id'); ids.add(p.id.toLowerCase());
    if(paths.has(norm(p.path))) errors.push('duplicate_primary_path'); paths.add(norm(p.path));
    const i=p.integration;
    if(!i) { projects.push({id:p.id,state:'pending',issues:['owner_registration_pending']}); continue; }
    if(!text(i.owner) || i.status!=='registered') issues.push('owner_registration_pending');
    if(p.preserveManifest!==true) issues.push('manifest_precedence_missing');
    if(!relative(i.receipt)) issues.push('receipt_outside_project');
    if(!/^https:\/\/github\.com\/[A-Za-z0-9_.-]+\/[A-Za-z0-9_.-]+(?:\.git)?$/.test(i.repository??'')) issues.push('repository_unverified');
    for(const r of roles) {
      if(!text(i.roles?.[r])) issues.push('role_missing:'+r);
      else if(['source','operating','data'].includes(r) && temporary(i.roles[r])) issues.push('temporary_dependency:'+r);
    }
    for(let a=0;a<roles.length;a++)for(let b=a+1;b<roles.length;b++) {
      if(text(i.roles?.[roles[a]]) && text(i.roles?.[roles[b]]) && norm(i.roles[roles[a]])===norm(i.roles[roles[b]])) issues.push('role_collision:'+roles[a]+':'+roles[b]);
    }
    if(!Array.isArray(i.contracts)||!i.contracts.length||i.contracts.some(c=>!text(c?.owner)||!/^https:\/\/github\.com\/.+\/blob\/[a-f0-9]{40}\/.+/.test(c.reference??''))) issues.push('contract_reference_unverified');
    if(local) localProject(p,i,issues,now+(Date.now()-catalogStarted));
    const unique=[...new Set(issues)];
    projects.push({id:p.id,owner:i.owner,state:unique.length?'blocked':local?'complete':'registered',issues:unique});
  }
  return {schemaVersion:1,checkedAt:new Date(now).toISOString(),mode:local?'local-read-only':'structure-only',errors:[...new Set(errors)],total:catalog.projects.length,
    registered:catalog.projects.filter(p=>p?.integration?.status==='registered').length,
    complete:errors.length?0:projects.filter(p=>p.state==='complete').length,projects};
}
if (process.argv[1] && path.resolve(process.argv[1])===fileURLToPath(import.meta.url)) {
  const args=process.argv.slice(2), catalogArg=args.indexOf('--catalog');
  if(catalogArg<0 || !args[catalogArg+1] || args.some((v,n)=>!['--catalog','--local'].includes(v) && n!==catalogArg+1)) {
    console.error('Usage: node validate.mjs --catalog <existing-project.catalog.json> [--local]');process.exitCode=2;
  } else {
    let report;
    try {report=validateCatalog(JSON.parse(regular(args[catalogArg+1],2*1024*1024).toString('utf8').replace(/^\uFEFF/,'')),{local:args.includes('--local')});}
    catch {report={schemaVersion:1,errors:['catalog_unreadable'],complete:0};}
    console.log(JSON.stringify(report,null,2));
    process.exitCode=report.errors.length || report.projects?.some(p=>p.state==='blocked'||p.state==='pending') ? 1 : 0;
  }
}
