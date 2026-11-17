"""Typed MCP catalog and matching server-side validation (stdlib only)."""
import re

def obj(properties,required=()):
    return {'type':'object','properties':properties,'required':list(required),'additionalProperties':False}
def string(maximum=1024,**extra):return {'type':'string','maxLength':maximum,**extra}
def integer(low,high):return {'type':'integer','minimum':low,'maximum':high}
def array(items,maximum=30):return {'type':'array','items':items,'maxItems':maximum}
ID=string(80,pattern=r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$')
SCOPE={'projectId':string(120,pattern=r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,119}$'),'toolId':string(120,pattern=r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,119}$')}
HASH=string(64,pattern=r'^[a-f0-9]{64}$')
PATH=string(32767)
BOOL={'type':'boolean'}
TRANSFER_ID=string(32,pattern=r'^[a-f0-9]{32}$')
TAGS=array(string(40),12)
OP_ARGS={
 'capabilities':obj({}),
 'read_file':obj({'path':PATH,'maxBytes':integer(1,524288),'offsetBytes':integer(0,2**63-1),'encoding':{'enum':['auto','base64']},'includeSha256':BOOL,'expectedFileVersion':array(integer(0,2**64-1),4)},['path']),
 'file_info':obj({'path':PATH,'includeSha256':BOOL},['path']),
 'write_file':obj({'path':PATH,'expectedSha256':{'anyOf':[HASH,{'type':'null'}]},'contentUtf8':string(524288),'contentBase64':string(699052)},['path','expectedSha256']),
 'list_dir':obj({'path':PATH,'limit':integer(1,5000),'cursor':string(2048)},['path']),
 'find_files':obj({'path':PATH,'pattern':string(256),'extensions':array(string(20),20),'recursive':BOOL,'limit':integer(1,500),'cursor':string(2048)},['path']),
 'make_dir':obj({'path':PATH},['path']),
 'move_file':obj({'path':PATH,'destination':PATH,'expectedSha256':HASH},['path','destination','expectedSha256']),
 'delete_file':obj({'path':PATH,'expectedSha256':HASH},['path','expectedSha256']),
 'restore_file':obj({'trashId':TRANSFER_ID,'expectedSha256':HASH},['trashId','expectedSha256']),
 'run_command':obj({'argv':array(string(32768),128),'python':string(256*1024),'powershell':string(256*1024),'cwd':PATH,'timeoutSeconds':integer(1,86400)}),
 'start_process':obj({'argv':array(string(32768),128),'python':string(256*1024),'powershell':string(256*1024),'cwd':PATH,'timeoutSeconds':integer(1,86400)}),
 'process_status':obj({'processId':TRANSFER_ID},['processId']),
 'stop_process':obj({'processId':TRANSFER_ID},['processId']),
 'upload_begin':obj({'path':PATH,'expectedSha256':{'anyOf':[HASH,{'type':'null'}]},'totalBytes':integer(0,1024**4),'sha256':HASH},['path','expectedSha256','totalBytes','sha256']),
 'upload_chunk':obj({'uploadId':TRANSFER_ID,'offsetBytes':integer(0,1024**4),'contentBase64':string(699052),'chunkSha256':HASH},['uploadId','offsetBytes','contentBase64','chunkSha256']),
 'upload_status':obj({'uploadId':TRANSFER_ID},['uploadId']),
 'upload_commit':obj({'uploadId':TRANSFER_ID},['uploadId']),
 'upload_abort':obj({'uploadId':TRANSFER_ID},['uploadId']),
}
UI_ARGS={
 'screen_capture':obj({'primary':BOOL,'crop':obj({'x':integer(-100000,100000),'y':integer(-100000,100000),'width':integer(1,32000),'height':integer(1,32000)},['x','y','width','height']),'maxWidth':integer(320,1920),'maxHeight':integer(240,2160),'quality':integer(20,85)}),
 'image_preview':obj({'path':PATH,'maxWidth':integer(320,1920),'maxHeight':integer(240,2160),'quality':integer(20,85)},['path']),
 'window_list':obj({'limit':integer(1,200)}),
 'mouse_move':obj({'x':integer(-100000,100000),'y':integer(-100000,100000),'expectedHwnd':string(24)},['x','y']),
 'mouse_click':obj({'x':integer(-100000,100000),'y':integer(-100000,100000),'button':{'enum':['left','right','middle']},'clicks':integer(1,2),'expectedHwnd':string(24)},['x','y']),
 'mouse_drag':obj({'x':integer(-100000,100000),'y':integer(-100000,100000),'toX':integer(-100000,100000),'toY':integer(-100000,100000),'durationMs':integer(50,3000),'button':{'enum':['left','right','middle']},'expectedHwnd':string(24)},['x','y','toX','toY']),
 'mouse_scroll':obj({'amount':integer(-120,120),'horizontal':BOOL,'expectedHwnd':string(24)},['amount']),
 'type_text':obj({'text':string(4000),'expectedHwnd':string(24)},['text']),
 'send_keys':obj({'keys':string(80),'expectedHwnd':string(24)},['keys']),
 'focus_window':obj({'hwnd':string(24)},['hwnd'])
}
UI_ARGS.update({
 'ui_elements':obj({'hwnd':string(24),'limit':integer(1,200),'depth':integer(1,8)},['hwnd']),
 'ui_invoke':obj({'hwnd':string(24),'elementId':string(1024),'expectedName':string(500),'expectedProcessId':integer(1,2**31-1)},['hwnd','elementId','expectedName','expectedProcessId']),
 'ui_set_value':obj({'hwnd':string(24),'elementId':string(1024),'expectedName':string(500),'expectedProcessId':integer(1,2**31-1),'value':string(4000)},['hwnd','elementId','expectedName','expectedProcessId','value'])
})
OP_ARGS['ui_control']=obj({'action':{'enum':list(UI_ARGS)},'args':{'type':'object'}},['action','args'])
META={'id':ID,'title':string(160),'tags':TAGS,'dependsOn':array(ID),'resourceKeys':array(SCOPE['projectId']),**SCOPE}
READ_ACTIONS={'capabilities','read_file','file_info','list_dir','find_files','process_status','upload_status'}
READ_UI={'screen_capture','image_preview','window_list','ui_elements'}
STEP=obj({'id':string(32,pattern=r'^[A-Za-z0-9][A-Za-z0-9_-]{0,31}$'),'action':{'enum':list(OP_ARGS)},'args':{'type':'object'},'dependsOn':array(string(32),30)},['id','action','args'])
STEPS=array(STEP,30)
MANAGEMENT={
 'pc_jobs_list':obj({**SCOPE,'query':string(160),'state':{'enum':['queued','started','starting','running','completed','failed','cancelled','interrupted','timed_out','stopped']},'action':{'enum':list(OP_ARGS)},'tag':string(40),'limit':integer(1,100),'cursor':string(2048)}),
 'pc_job_wait':obj({'id':ID,'waitSeconds':integer(0,25),**SCOPE},['id']),
 'pc_job_cancel':obj({'id':ID,**SCOPE},['id']),
 'pc_job_log':obj({'id':ID,'stream':{'enum':['stdout','stderr']},'offsetBytes':integer(0,2**63-1),'maxBytes':integer(1,65536),**SCOPE},['id']),
 'pc_task_put':obj({'taskId':ID,'title':string(160),'tags':TAGS,'steps':STEPS,'expectedRevision':integer(0,2**31-1),**SCOPE},['taskId','title','steps','expectedRevision']),
 'pc_task_get':obj({'taskId':ID,**SCOPE},['taskId']),
 'pc_tasks_list':obj({**SCOPE}),
 'pc_task_delete':obj({'taskId':ID,'expectedRevision':integer(1,2**31-1),**SCOPE},['taskId','expectedRevision']),
 'pc_task_run':obj({'taskId':ID,'runId':string(40,pattern=r'^[A-Za-z0-9][A-Za-z0-9_-]{0,39}$'),**SCOPE},['taskId','runId']),
 'pc_runs_list':obj({'taskId':ID,'limit':integer(1,100),'cursor':string(2048),**SCOPE}),
 'pc_run_cancel':obj({'runId':ID,'stopRunning':BOOL,**SCOPE},['runId']),
 'pc_run_status':obj({'runId':ID,**SCOPE},['runId']),
 'pc_schedule_put':obj({'scheduleId':ID,'title':string(160),'taskId':ID,'expectedRevision':integer(0,2**31-1),'timing':obj({'kind':{'enum':['once','interval','daily']},'at':string(48),'everySeconds':integer(60,365*86400),'startAt':string(48),'time':string(5,pattern=r'^([01][0-9]|2[0-3]):[0-5][0-9]$'),'timezone':{'enum':['Asia/Seoul','UTC']},'daysOfWeek':array(integer(1,7),7)},['kind']),'missedPolicy':{'enum':['skip','run_once']},'graceSeconds':integer(0,86400),'paused':BOOL,**SCOPE},['scheduleId','title','taskId','expectedRevision','timing']),
 'pc_schedules_list':obj({**SCOPE}),
 'pc_schedule_set':obj({'scheduleId':ID,'expectedRevision':integer(1,2**31-1),'paused':BOOL,**SCOPE},['scheduleId','expectedRevision','paused']),
 'pc_schedule_delete':obj({'scheduleId':ID,'expectedRevision':integer(1,2**31-1),**SCOPE},['scheduleId','expectedRevision']),
 'pc_alias_put':obj({'alias':string(60,pattern=r'^[^@/\\:]+$'),'path':PATH,'expectedRevision':integer(0,2**31-1),**SCOPE},['alias','path','expectedRevision']),
 'pc_aliases_list':obj({**SCOPE}),
 'pc_alias_delete':obj({'alias':string(60),'expectedRevision':integer(1,2**31-1),**SCOPE},['alias','expectedRevision'])
}
DESCRIPTIONS={
 'pc_jobs_list':'List/search all jobs in this project/tool scope; filters, tags and stable cursor pages. No command/file payloads.',
 'pc_job_wait':'Wait up to 25 seconds for actual completion; detached commands remain running until their owned child ends. Returns immediately for terminal jobs.',
 'pc_job_cancel':'Cancel a queued job only. For a running owned process use pc_stop_process with its processId.',
 'pc_job_log':'Read the next bounded stdout/stderr bytes from a completed/accepted command job using nextOffsetBytes.',
 'pc_task_put':'Create/update a durable named workflow, up to 30 ordered steps with dependencies. expectedRevision=0 creates; updates require current revision.',
 'pc_task_get':'Get a stored workflow and its exact operations.',
 'pc_tasks_list':'List saved tasks and revisions in this project/tool scope.',
 'pc_task_delete':'Delete a saved task; existing schedules retain their frozen task snapshot.',
 'pc_task_run':'Run a stored workflow. Choose a stable runId once and reuse it on network retry; a new runId deliberately runs again.',
 'pc_runs_list':'List workflow run history, progress and IDs in this scope with cursor pagination.',
 'pc_run_cancel':'Cancel queued workflow steps. Optional stopRunning requests stop only for recorded owned processes; unsupported running jobs are reported, never killed by name.',
 'pc_run_status':'Return workflow progress by finished step count, actual process states and job IDs.',
 'pc_schedule_put':'Create/update a persistent PC-worker schedule from a frozen saved task. once/interval/daily, Seoul or UTC, missed-run policy, no overlap. No scheduled desktop input or automatic mutation retries.',
 'pc_schedules_list':'List schedules including next time, revision, last run and last error.',
 'pc_schedule_set':'Pause/resume a schedule with revision checking; already submitted jobs are unaffected.',
 'pc_schedule_delete':'Delete a schedule with revision checking; already submitted jobs are unaffected.',
 'pc_alias_put':'Save an existing absolute directory as @alias; @alias/relative/path is resolved per project/tool scope. Traversal outside the alias root is rejected.',
 'pc_aliases_list':'List registered directory aliases and revisions.',
 'pc_alias_delete':'Delete a directory alias with revision checking.'
}
READ_MANAGEMENT={'pc_jobs_list','pc_job_wait','pc_job_log','pc_task_get','pc_tasks_list','pc_run_status','pc_schedules_list','pc_aliases_list','pc_runs_list'}
TYPED={}
for action in OP_ARGS:
    if action in ('ui_control','capabilities'):continue
    TYPED['pc_'+action]=(action,None)
for action in UI_ARGS:TYPED['pc_'+action]=('ui_control',action)

def additions():
    tools=[]
    for name,(action,ui) in TYPED.items():
        schema=UI_ARGS[ui] if ui else OP_ARGS[action]
        props={**schema['properties'],**META}
        read=ui in READ_UI if ui else action in READ_ACTIONS
        text='Submit '+(ui or action)+' and poll pc_result(id) or pc_job_wait(id). Paths accept @alias. Optional title/tags identify the job.'
        if ui in ('ui_elements','ui_invoke','ui_set_value'):text+=' UI Automation is limited to this hwnd. Refresh pc_ui_elements; actions require its elementId, exact expectedName and expectedProcessId, and the window must already be foreground. Password values are omitted/refused; verify state after invoke.'
        if ui=='focus_window':text+=' One activation attempt only. foreground_activation_denied is a window-activation failure, not a global permission denial. Refresh window_list once; continue independent file/API work. Do not ask for repeated manual clicks or bypass a host policy denial.'
        if ui in ('mouse_move','mouse_click','mouse_drag','mouse_scroll','type_text','send_keys'):text+=' Supply expectedHwnd from a fresh window_list. A chat reply can change foreground; never send input to a stale target. foreground_window_changed means no input was sent.'
        if action in ('run_command','start_process'):text+=' Exactly one of argv, python, powershell is required; prefer pc_start_process for long work.'
        tools.append({'name':name,'description':text,'inputSchema':obj(props,schema.get('required',[])),'outputSchema':{'type':'object'},'annotations':{'readOnlyHint':read,'destructiveHint':not read,'idempotentHint':False,'openWorldHint':action in ('run_command','start_process')}})
    for name,schema in MANAGEMENT.items():
        read=name in READ_MANAGEMENT
        tools.append({'name':name,'description':DESCRIPTIONS[name],'inputSchema':schema,'outputSchema':{'type':'object'},'annotations':{'readOnlyHint':read,'destructiveHint':not read,'idempotentHint':read,'openWorldHint':name in ('pc_task_run','pc_schedule_put')}})
    return tools

def check(value,schema):
    if 'anyOf' in schema:
        for option in schema['anyOf']:
            try:check(value,option);return
            except ValueError:pass
        raise ValueError('invalid_argument_type')
    if 'enum' in schema and value not in schema['enum']:raise ValueError('invalid_argument_enum')
    kind=schema.get('type')
    valid={'object':isinstance(value,dict),'array':isinstance(value,list),'string':isinstance(value,str),'integer':type(value) is int,'boolean':type(value) is bool,'null':value is None}
    if kind and not valid.get(kind,False):raise ValueError('invalid_argument_type')
    if kind=='object':
        if not set(schema.get('required',[]))<=set(value):raise ValueError('required_argument_missing')
        if schema.get('additionalProperties') is False and set(value)-set(schema['properties']):raise ValueError('unknown_argument')
        for key,item in value.items():
            if key in schema.get('properties',{}):check(item,schema['properties'][key])
    if kind=='array':
        if len(value)>schema.get('maxItems',100000):raise ValueError('too_many_items')
        for item in value:check(item,schema['items'])
    if kind=='string':
        if len(value)>schema.get('maxLength',1000000) or '\x00' in value:raise ValueError('invalid_argument_string')
        if 'pattern' in schema and not re.fullmatch(schema['pattern'],value):raise ValueError('invalid_argument_format')
    if kind=='integer' and not schema.get('minimum',value)<=value<=schema.get('maximum',value):raise ValueError('argument_out_of_range')

def validate_operation(action,args):
    if action not in OP_ARGS:raise ValueError('unsupported_action')
    check(args,OP_ARGS[action])
    if action=='ui_control':
        check(args['args'],UI_ARGS[args['action']])
    if action in ('run_command','start_process') and sum(k in args for k in ('argv','python','powershell'))!=1:raise ValueError('one_command_kind_required')
    if action=='write_file' and sum(k in args for k in ('contentUtf8','contentBase64'))!=1:raise ValueError('one_content_kind_required')
