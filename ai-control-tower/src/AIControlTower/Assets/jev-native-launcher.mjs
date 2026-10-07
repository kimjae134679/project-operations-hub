import {spawn} from 'node:child_process';
import {existsSync,mkdirSync,lstatSync} from 'node:fs';
import {homedir} from 'node:os';
import {join,resolve,dirname} from 'node:path';
import {pathToFileURL} from 'node:url';

const MODEL='gpt-6.1-sol';
function prepareRuntime(directory){
  const full=resolve(directory),root=resolve('D:/A_KJ/AI')+'\\';
  if(!full.toLowerCase().startsWith(root.toLowerCase()))throw Error('invalid runtime path');
  for(let p=full;;p=dirname(p)){if(existsSync(p)&&lstatSync(p).isSymbolicLink())throw Error('reparse runtime path');if(p===dirname(p))break;}
  mkdirSync(join(full,'state'),{recursive:true});mkdirSync(join(full,'logs'),{recursive:true});return full;
}
async function installedRuntime(){
  const home=homedir(),npm=join(home,'AppData','Roaming','npm','node_modules'),src=join(npm,'jev-router','src');
  const cli=await import(pathToFileURL(join(src,'codex-cli.mjs')).href);
  const proxy=await import(pathToFileURL(join(src,'codex-proxy.mjs')).href);
  return {loadEnv:cli.loadEnv,codexArgs:cli.codexArgs,startCodexProxy:proxy.startCodexProxy,
    nativeCli:join(npm,'@openai','codex','node_modules','@openai','codex-win32-x64','vendor','x86_64-pc-windows-msvc','bin','codex.exe'),
    runtimeDirectory:join('D:/A_KJ/AI/ControlTowerData/jev-runtime',String(Date.now())+'-'+process.pid),home,spawn,env:process.env,cwd:process.cwd(),input:process.stdin,stdout:process.stdout,stderr:process.stderr};
}
export async function runJevTask(argv,deps){
  // Only the application's registered, explicit-model writer contract is accepted.
  if(JSON.stringify(argv)!==JSON.stringify(['--model',MODEL,'--json','--sandbox','workspace-write','-']))return 1;
  deps??=await installedRuntime();
  deps.loadEnv();
  if(deps.env.JEV_DUMP||deps.env.JEV_DEBUG||deps.env.OPENAI_API_KEY)return 1;
  if(!deps.env.JEV_API_KEY&&!deps.env.TYPESAFE_API_KEY)return 1;
  if(deps.home&&(homedir()!==deps.home||(deps.env.CODEX_HOME&&resolve(deps.env.CODEX_HOME)!==resolve(join(deps.home,'.codex')))))return 1;
  if(deps.home&&!existsSync(deps.nativeCli))return 1;
  let proxy;
  try{
    proxy=await deps.startCodexProxy({apiBaseURL:'http://127.0.0.1:1/',route:async()=>{throw Error('automatic routing prohibited');}});
    const common=[];
    if(deps.runtimeDirectory){const runtime=prepareRuntime(deps.runtimeDirectory);common.push('-c','sqlite_home='+JSON.stringify(join(runtime,'state')),'-c','log_dir='+JSON.stringify(join(runtime,'logs')));}
    for(const feature of ['hooks','memories','plugins','apps','multi_agent'])common.push('--disable',feature);
    for(const config of ['approval_policy="never"','web_search="disabled"','notify=[]','history.persistence="none"','model_reasoning_effort="low"',
      'model_providers.jev.request_max_retries=0','model_providers.jev.stream_max_retries=0','model_providers.jev.stream_idle_timeout_ms=30000'])common.push('-c',config);
    const args=deps.codexArgs(`http://127.0.0.1:${proxy.port}`,[...common,'exec','--ignore-user-config','--ephemeral','--model',MODEL,'--json','--sandbox','workspace-write','--cd',deps.cwd,'-']);
    const child=deps.spawn(deps.nativeCli,args,{cwd:deps.cwd,windowsHide:true,shell:false,stdio:['pipe','pipe','pipe'],env:deps.env});
    child.stdout.on('data',b=>deps.stdout.write(b));child.stderr.on('data',b=>deps.stderr.write(b));
    let settleInput;
    const inputDelivered=new Promise(resolveInput=>{settleInput=resolveInput;child.stdin.once('finish',()=>resolveInput(true));child.stdin.once('error',()=>resolveInput(false));deps.input.once('error',()=>resolveInput(false));});
    const closed=new Promise(resolveClose=>{child.once('error',()=>{settleInput(false);resolveClose(1);});child.once('close',(code,signal)=>resolveClose(signal?1:(code??1)));});
    deps.input.pipe(child.stdin);
    const [code,delivered]=await Promise.all([closed,inputDelivered]);
    return delivered?code:1;
  }finally{proxy?.close();}
}
if(process.argv[1]&&import.meta.url===pathToFileURL(resolve(process.argv[1])).href){
  let code=1;try{code=await runJevTask(process.argv.slice(2));}catch{process.stderr.write('Jev 실행 사전 검사 또는 연결 실패\n');}
  // Installed proxy can retain idle HTTPS handles; exit only this owned launcher after native termination.
  process.exit(code);
}
