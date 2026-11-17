"""Durable local tasks, workflow runs, directory aliases and worker schedules."""
import base64,copy,datetime as dt,hashlib,json,os,re,time,threading
from pathlib import Path
from tool_catalog import MANAGEMENT,check,validate_operation

UTC=dt.timezone.utc
ZONES={'UTC':UTC,'Asia/Seoul':dt.timezone(dt.timedelta(hours=9))}
TERMINAL={'completed','failed','cancelled','interrupted','timed_out','stopped','failed_to_start'}
def utcnow():return dt.datetime.now(UTC)
def encode(value):return json.dumps(value,ensure_ascii=False,sort_keys=True,separators=(',',':'))
def fingerprint(value):return hashlib.sha256(encode(value).encode()).hexdigest()
def timestamp(value):
    try:result=dt.datetime.fromisoformat(value.replace('Z','+00:00'))
    except (ValueError,AttributeError):raise ValueError('invalid_schedule_time')
    if result.tzinfo is None:raise ValueError('schedule_time_requires_offset')
    return result.astimezone(UTC)
def next_daily(timing,after):
    zone=ZONES[timing.get('timezone','Asia/Seoul')]
    hour,minute=map(int,timing['time'].split(':'));local=after.astimezone(zone)
    days=timing.get('daysOfWeek',list(range(1,8)))
    for offset in range(8):
        candidate=dt.datetime.combine(local.date()+dt.timedelta(days=offset),dt.time(hour,minute),zone)
        if candidate.isoweekday() in days and candidate>local:return candidate.astimezone(UTC)
    raise ValueError('invalid_schedule_days')
def validate_timing(timing,at):
    kind=timing['kind'];required={'once':{'kind','at'},'interval':{'kind','everySeconds'},'daily':{'kind','time'}}[kind]
    allowed={'once':required,'interval':required|{'startAt'},'daily':required|{'timezone','daysOfWeek'}}[kind]
    if not required<=set(timing) or set(timing)-allowed:raise ValueError('invalid_schedule_timing_fields')
    if kind=='daily':
        days=timing.get('daysOfWeek',list(range(1,8)))
        if not days or len(set(days))!=len(days):raise ValueError('invalid_schedule_days')
        return next_daily(timing,at)
    value=timestamp(timing['at']) if kind=='once' else timestamp(timing['startAt']) if 'startAt' in timing else at+dt.timedelta(seconds=timing['everySeconds'])
    if value<=at:raise ValueError('schedule_first_run_must_be_future')
    return value
def next_after(timing,due,at):
    if timing['kind']=='once':return None
    if timing['kind']=='daily':return next_daily(timing,at)
    seconds=timing['everySeconds'];count=max(1,int((at-due).total_seconds()//seconds)+1)
    return due+dt.timedelta(seconds=count*seconds)

class Manager:
    def __init__(self,worker):
        from universal_worker import read
        self.worker=worker;self.path=worker.state/'control_plane.json'
        self.db=read(self.path,{'schemaVersion':1,'scopes':{}})
        if self.db.get('schemaVersion')!=1:raise ValueError('invalid_control_state_version')
        self.next_tick=0;self.wait_slots=threading.BoundedSemaphore(8)
    def save(self):
        from universal_worker import atomic
        atomic(self.path,self.db)
    def scope(self,args):
        project=args.get('projectId','Control-Tower');tool=args.get('toolId','ProjectBridge')
        key=fingerprint([project,tool])[:32]
        return self.db['scopes'].setdefault(key,{'projectId':project,'toolId':tool,'tasks':{},'schedules':{},'aliases':{},'runs':{}})
    def revision(self,rows,key,expected):
        prior=rows.get(key)
        if (prior['revision'] if prior else 0)!=expected:raise ValueError('revision_conflict_refresh_list')
        return prior
    def resolve_args(self,args,scope):
        result=copy.deepcopy(args)
        for key in ('path','destination','cwd'):
            if isinstance(result.get(key),str) and result[key].startswith('@'):
                result[key]=self.resolve_alias(result[key],scope)
        if isinstance(result.get('args'),dict):result['args']=self.resolve_args(result['args'],scope)
        return result
    def resolve_alias(self,value,scope):
        name,_,relative=value[1:].replace('\\','/').partition('/')
        row=scope['aliases'].get(name)
        if row is None:raise ValueError('alias_not_found_in_scope')
        root=Path(row['path']).resolve();path=(root/relative).resolve()
        if path!=root and not path.is_relative_to(root):raise ValueError('alias_path_outside_root')
        return str(path)
    def steps(self,steps):
        if not steps:raise ValueError('workflow_requires_steps')
        known=set()
        for step in steps:
            if step['id'] in known:raise ValueError('duplicate_step_id')
            deps=step.get('dependsOn',[])
            if len(set(deps))!=len(deps) or not set(deps)<=known:raise ValueError('dependencies_must_reference_prior_steps')
            validate_operation(step['action'],step['args']);known.add(step['id'])
        if len(encode(steps).encode())>256*1024:raise ValueError('workflow_too_large')
        return copy.deepcopy(steps)
    def owned_job(self,jid,scope):
        row=self.worker.journal.get(jid)
        if row is None:raise ValueError('job_not_found')
        job=row.get('job',{})
        if job.get('projectId','Control-Tower')!=scope['projectId'] or job.get('toolId','ProjectBridge')!=scope['toolId']:raise ValueError('job_not_found_in_scope')
        return row
    def job_summary(self,jid,row):
        job=row['job'];process=self.worker.live_process(row)
        state=self.worker.effective_state(row,process)
        data=row.get('result',{}).get('data',{})
        return {'id':jid,'title':job.get('title',job['action']),'tags':job.get('tags',[]),'action':job['action'],'state':state,'projectId':job.get('projectId','Control-Tower'),'toolId':job.get('toolId','ProjectBridge'),'createdAt':job['createdAt'],'startedAt':row.get('startedAt'),'finishedAt':row.get('result',{}).get('finishedAt'),'processId':data.get('processId'),'errorCode':data.get('code') or data.get('error',{}).get('code') if isinstance(data.get('error',{}),dict) else data.get('code'),'done':state in TERMINAL}
    def launch(self,scope,run_id,task,source='manual',at=None):
        at=at or utcnow();request=fingerprint({'taskId':task['taskId'],'revision':task['revision'],'steps':task['steps']})
        prior=scope['runs'].get(run_id)
        if prior:
            if prior['requestHash']!=request:raise ValueError('run_id_reused')
            self.dispatch(scope,prior);return self.run_status(scope,run_id)
        if len(scope['runs'])>=10000:raise ValueError('run_history_limit')
        jobs=[]
        for step in task['steps']:
            jid=run_id+'-'+step['id']
            job={'id':jid,'target':'PC','deviceId':self.worker.device,'action':step['action'],'args':self.resolve_args(step['args'],scope),'projectId':scope['projectId'],'toolId':scope['toolId'],'title':task['title']+' / '+step['id'],'tags':task.get('tags',[]),'batchId':run_id,'createdAt':at.isoformat(),'expiresAt':(at+dt.timedelta(days=1)).isoformat(),'dependsOn':[run_id+'-'+dep for dep in step.get('dependsOn',[])]}
            from universal_worker import validate_job
            validate_job(job,self.worker.device,at);jobs.append(job)
        if sum(r.get('state') in ('queued','started') for r in self.worker.journal.values())+len(jobs)>250:raise ValueError('local_queue_full')
        run={'runId':run_id,'taskId':task['taskId'],'taskRevision':task['revision'],'title':task['title'],'requestHash':request,'source':source,'jobs':jobs,'createdAt':at.isoformat(),'dispatchState':'dispatching'}
        scope['runs'][run_id]=run;self.save();self.dispatch(scope,run)
        return self.run_status(scope,run_id)
    def dispatch(self,scope,run):
        if run['dispatchState']=='submitted':return
        for job in run['jobs']:self.worker.submit(job,source='local')
        run['dispatchState']='submitted';self.save()
    def run_status(self,scope,run_id):
        run=scope['runs'].get(run_id)
        if run is None:raise ValueError('run_not_found_in_scope')
        rows=[self.job_summary(job['id'],self.worker.journal[job['id']]) if job['id'] in self.worker.journal else {'id':job['id'],'state':'dispatching','done':False} for job in run['jobs']]
        done=sum(row['done'] for row in rows);finished=done==len(rows)
        state='completed' if finished and all(row['state']=='completed' for row in rows) else 'failed' if finished else 'running'
        return {'runId':run_id,'taskId':run['taskId'],'taskRevision':run['taskRevision'],'title':run['title'],'source':run['source'],'state':state,'done':finished,'progress':{'completedSteps':done,'totalSteps':len(rows),'percent':round(done*100/len(rows),1)},'jobs':rows}
    def call(self,name,args):
        if not isinstance(name,str) or name not in MANAGEMENT:raise ValueError('unknown_management_tool')
        check(args,MANAGEMENT[name])
        if name=='pc_job_wait':
            if not self.wait_slots.acquire(blocking=False):raise ValueError('wait_capacity_retry_later')
            try:return self.wait(args)
            finally:self.wait_slots.release()
        with self.worker.lock:
            scope=self.scope(args)
            if name=='pc_jobs_list':return self.list_jobs(scope,args)
            if name=='pc_job_log':return self.log(scope,args)
            if name=='pc_job_cancel':
                row=self.owned_job(args['id'],scope)
                if row['state']=='queued':
                    row['result']=self.worker.make_result(args['id'],row['jobHash'],'cancelled',{'code':'cancelled_before_start'});row['state']='finished'
                    self.worker.save_record(args['id'],row);self.worker.checkpoint();self.worker.ui()
                elif self.worker.effective_state(row,self.worker.live_process(row)) not in TERMINAL:raise ValueError('queued_only_use_pc_stop_process')
                return self.job_summary(args['id'],row)
            if name=='pc_tasks_list':return {'tasks':[{k:r[k] for k in ('taskId','title','tags','revision','updatedAt')}|{'stepCount':len(r['steps'])} for r in scope['tasks'].values()]}
            if name=='pc_task_get':
                row=scope['tasks'].get(args['taskId'])
                if row is None:raise ValueError('task_not_found_in_scope')
                return copy.deepcopy(row)
            if name=='pc_task_put':
                if not args['title'].strip():raise ValueError('task_title_required')
                rows=scope['tasks'];prior=self.revision(rows,args['taskId'],args['expectedRevision'])
                if prior is None and len(rows)>=200:raise ValueError('task_limit')
                row={'taskId':args['taskId'],'title':args['title'],'tags':args.get('tags',[]),'steps':self.steps(args['steps']),'revision':args['expectedRevision']+1,'updatedAt':utcnow().isoformat()}
                rows[args['taskId']]=row;self.save();return copy.deepcopy(row)
            if name=='pc_task_delete':
                rows=scope['tasks'];prior=self.revision(rows,args['taskId'],args['expectedRevision'])
                if prior is None:raise ValueError('task_not_found_in_scope')
                del rows[args['taskId']];self.save();return {'deleted':True,'taskId':args['taskId']}
            if name=='pc_task_run':
                task=scope['tasks'].get(args['taskId'])
                if task is None:raise ValueError('task_not_found_in_scope')
                return self.launch(scope,args['runId'],task)
            if name=='pc_runs_list':
                rows=[]
                for run_id,run in scope['runs'].items():
                    if args.get('taskId') and run['taskId']!=args['taskId']:continue
                    summary=self.run_status(scope,run_id);summary.pop('jobs')
                    summary['createdAt']=run['createdAt'];rows.append(summary)
                rows.sort(key=lambda row:(row['createdAt'],row['runId']),reverse=True)
                criteria=fingerprint({k:v for k,v in args.items() if k not in ('cursor','limit')})
                if args.get('cursor'):
                    try:cursor=json.loads(base64.urlsafe_b64decode(args['cursor']).decode())
                    except (ValueError,UnicodeError):raise ValueError('invalid_run_cursor')
                    if not isinstance(cursor,dict) or cursor.get('filter')!=criteria or not isinstance(cursor.get('anchor'),list) or len(cursor['anchor'])!=2:raise ValueError('run_cursor_filter_conflict')
                    rows=[row for row in rows if (row['createdAt'],row['runId'])<tuple(cursor['anchor'])]
                limit=args.get('limit',50);page=rows[:limit]
                cursor=base64.urlsafe_b64encode(encode({'filter':criteria,'anchor':[page[-1]['createdAt'],page[-1]['runId']]}).encode()).decode() if len(rows)>limit else None
                return {'runs':page,'nextCursor':cursor}
            if name=='pc_run_cancel':
                import universal_actions as actions
                run=scope['runs'].get(args['runId'])
                if run is None:raise ValueError('run_not_found_in_scope')
                cancelled=[];stop_requested=[];not_stopped=[]
                for job in run['jobs']:
                    row=self.worker.journal.get(job['id'])
                    if row is None:
                        # A partial dispatch must not enqueue missing steps after cancel.
                        from universal_worker import hashed
                        key=hashed(job)
                        row={'jobHash':key,'state':'finished','job':copy.deepcopy(job),'source':'local','action':job['action'],'projectId':job['projectId'],'createdAt':job['createdAt'],'result':self.worker.make_result(job['id'],key,'cancelled',{'code':'cancelled_before_dispatch'})}
                        self.worker.journal[job['id']]=row;self.worker.save_record(job['id'],row);cancelled.append(job['id']);continue
                    if row['state']=='queued':
                        row['result']=self.worker.make_result(job['id'],row['jobHash'],'cancelled',{'code':'cancelled_before_start'});row['state']='finished'
                        self.worker.save_record(job['id'],row);cancelled.append(job['id'])
                    elif self.worker.effective_state(row,self.worker.live_process(row)) not in TERMINAL:
                        pid=row.get('result',{}).get('data',{}).get('processId')
                        if args.get('stopRunning',False) and isinstance(pid,str):
                            _,state=self.worker.scoped_job(row['job']);actions.stop_process(pid,state);stop_requested.append(pid)
                        else:not_stopped.append(job['id'])
                run['dispatchState']='submitted';run['cancelRequestedAt']=utcnow().isoformat();self.save();self.worker.checkpoint()
                return {'runId':args['runId'],'cancelledQueuedIds':cancelled,'stopRequestedProcessIds':stop_requested,'runningJobsNotStopped':not_stopped}
            if name=='pc_run_status':return self.run_status(scope,args['runId'])
            if name=='pc_aliases_list':return {'aliases':list(scope['aliases'].values())}
            if name=='pc_alias_put':
                rows=scope['aliases'];self.revision(rows,args['alias'],args['expectedRevision'])
                if len(rows)>=100 and args['alias'] not in rows:raise ValueError('alias_limit')
                path=Path(args['path'])
                if not path.is_absolute() or not path.is_dir():raise ValueError('alias_requires_existing_absolute_directory')
                row={'alias':args['alias'],'path':str(path.resolve()),'revision':args['expectedRevision']+1}
                rows[args['alias']]=row;self.save();return row
            if name=='pc_alias_delete':
                rows=scope['aliases'];self.revision(rows,args['alias'],args['expectedRevision'])
                del rows[args['alias']];self.save();return {'deleted':True,'alias':args['alias']}
            if name=='pc_schedules_list':return {'schedules':[self.schedule_summary(row) for row in scope['schedules'].values()]}
            if name=='pc_schedule_put':
                if not args['title'].strip():raise ValueError('schedule_title_required')
                rows=scope['schedules'];prior=self.revision(rows,args['scheduleId'],args['expectedRevision'])
                if prior is None and len(rows)>=200:raise ValueError('schedule_limit')
                task=scope['tasks'].get(args['taskId'])
                if task is None:raise ValueError('task_not_found_in_scope')
                if any(step['action'] in ('ui_control','stop_process','process_status','upload_begin','upload_chunk','upload_commit','upload_abort','upload_status','restore_file') for step in task['steps']):raise ValueError('schedule_requires_noninteractive_durable_operations')
                due=validate_timing(args['timing'],utcnow())
                snapshot=copy.deepcopy(task)
                for step in snapshot['steps']:step['args']=self.resolve_args(step['args'],scope)
                row={'scheduleId':args['scheduleId'],'title':args['title'],'task':snapshot,'timing':copy.deepcopy(args['timing']),'missedPolicy':args.get('missedPolicy','run_once'),'graceSeconds':args.get('graceSeconds',60),'paused':args.get('paused',False),'revision':args['expectedRevision']+1,'nextAt':due.isoformat(),'lastRunId':prior.get('lastRunId') if prior else None,'lastError':None}
                rows[args['scheduleId']]=row;self.save();return self.schedule_summary(row)
            if name=='pc_schedule_set':
                row=self.revision(scope['schedules'],args['scheduleId'],args['expectedRevision'])
                if row is None:raise ValueError('schedule_not_found_in_scope')
                row['paused']=args['paused'];row['revision']+=1;self.save();return self.schedule_summary(row)
            if name=='pc_schedule_delete':
                rows=scope['schedules'];row=self.revision(rows,args['scheduleId'],args['expectedRevision'])
                if row is None:raise ValueError('schedule_not_found_in_scope')
                del rows[args['scheduleId']];self.save();return {'deleted':True,'scheduleId':args['scheduleId']}
        raise ValueError('unknown_management_tool')
    def schedule_summary(self,row):
        return {k:copy.deepcopy(row.get(k)) for k in ('scheduleId','title','timing','missedPolicy','graceSeconds','paused','revision','nextAt','lastRunId','lastError','lastDecision')}|{'taskId':row['task']['taskId'],'taskRevision':row['task']['revision']}
    def list_jobs(self,scope,args):
        rows=[]
        for jid,row in self.worker.journal.items():
            job=row.get('job',{})
            if job.get('projectId','Control-Tower')!=scope['projectId'] or job.get('toolId','ProjectBridge')!=scope['toolId']:continue
            summary=self.job_summary(jid,row)
            if args.get('state') and summary['state']!=args['state']:continue
            if args.get('action') and summary['action']!=args['action']:continue
            if args.get('tag') and args['tag'] not in summary['tags']:continue
            if args.get('query') and args['query'].casefold() not in (summary['title']+' '+jid+' '+' '.join(summary['tags'])).casefold():continue
            rows.append(summary)
        rows.sort(key=lambda row:(row['createdAt'],row['id']),reverse=True)
        criteria=fingerprint({k:v for k,v in args.items() if k not in ('cursor','limit')})
        if args.get('cursor'):
            try:cursor=json.loads(base64.urlsafe_b64decode(args['cursor']).decode())
            except (ValueError,UnicodeError):raise ValueError('invalid_job_cursor')
            if not isinstance(cursor,dict) or cursor.get('filter')!=criteria or not isinstance(cursor.get('anchor'),list) or len(cursor['anchor'])!=2:raise ValueError('job_cursor_filter_conflict')
            anchor=tuple(cursor['anchor']);rows=[row for row in rows if (row['createdAt'],row['id'])<anchor]
        limit=args.get('limit',50);page=rows[:limit]
        cursor=base64.urlsafe_b64encode(encode({'filter':criteria,'anchor':[page[-1]['createdAt'],page[-1]['id']]}).encode()).decode() if len(rows)>limit else None
        return {'jobs':page,'nextCursor':cursor,'remainingMatches':len(rows),'scope':{'projectId':scope['projectId'],'toolId':scope['toolId']}}
    def wait(self,args):
        deadline=time.monotonic()+args.get('waitSeconds',20)
        while True:
            with self.worker.lock:
                scope=self.scope(args);row=self.owned_job(args['id'],scope);summary=self.job_summary(args['id'],row)
                if summary['done'] or time.monotonic()>=deadline:
                    return {'job':summary,'done':summary['done'],'timedOutWaiting':not summary['done'],'result':self.worker.get_job(args['id']).get('result')}
            time.sleep(.1)
    def log(self,scope,args):
        import universal_actions as u
        row=self.owned_job(args['id'],scope);data=row.get('result',{}).get('data',{})
        pid=data.get('processId')
        if not isinstance(pid,str):raise ValueError('job_log_not_available_use_start_process')
        _,state=self.worker.scoped_job(row['job'])
        u.process_status(pid,state,maximum=0)
        path=u.folder_for(state,pid)/(args.get('stream','stdout')+'.log')
        size=path.stat().st_size if path.is_file() else 0;offset=args.get('offsetBytes',0)
        if offset>size:raise ValueError('log_offset_beyond_end')
        content=b''
        if path.is_file():
            with path.open('rb') as inp:inp.seek(offset);content=inp.read(args.get('maxBytes',65536))
        return {'id':args['id'],'processId':pid,'stream':args.get('stream','stdout'),'contentUtf8':u.redact(content.decode('utf-8',errors='replace')),'offsetBytes':offset,'nextOffsetBytes':offset+len(content),'totalBytes':size,'eof':offset+len(content)>=size}
    def tick(self,at=None,force=False):
        if not force and time.monotonic()<self.next_tick:return
        self.next_tick=time.monotonic()+1;at=at or utcnow()
        with self.worker.lock:
            changed=False
            for scope in self.db['scopes'].values():
                for run in list(scope['runs'].values()):
                    if run['dispatchState']=='dispatching':
                        try:self.dispatch(scope,run)
                        except Exception:pass
                for row in scope['schedules'].values():
                    if row['paused'] or not row['nextAt']:continue
                    due=timestamp(row['nextAt'])
                    if due>at:continue
                    if row.get('lastRunId') and not self.run_status(scope,row['lastRunId'])['done']:continue
                    if row['missedPolicy']=='skip' and (at-due).total_seconds()>row['graceSeconds']:
                        row['lastDecision']='missed_skipped'
                    else:
                        run_id='s-'+fingerprint([scope['projectId'],scope['toolId'],row['scheduleId'],row['revision'],row['nextAt']])[:28]
                        try:
                            self.launch(scope,run_id,row['task'],'schedule',at);row['lastRunId']=run_id;row['lastError']=None;row['lastDecision']='submitted'
                        except Exception as exc:
                            code=str(exc);row['lastError']=code if re.fullmatch(r'[A-Za-z0-9_]{1,100}',code) else 'schedule_dispatch_failed'
                            row['lastDecision']='dispatch_failed_review_required'
                            if run_id in scope['runs']:row['lastRunId']=run_id
                            elif code=='local_queue_full':
                                row['lastDecision']='queue_wait';changed=True;continue
                            else:row['paused']=True
                    following=next_after(row['timing'],due,at);row['nextAt']=following.isoformat() if following else None;changed=True
            if changed:self.save()
