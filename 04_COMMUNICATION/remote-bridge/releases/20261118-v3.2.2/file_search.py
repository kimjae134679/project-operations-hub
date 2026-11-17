"""Bounded directory snapshots with scope-local, expiring cursor pages."""
import fnmatch,json,os,re,stat,time,uuid
from pathlib import Path

def perform(action,args,config,state):
    import universal_actions as u
    from tool_catalog import validate_operation
    validate_operation(action,args)
    root=u.absolute_path(args['path'])
    if not root.is_dir():raise u.ActionError('directory_not_found')
    limit=args.get('limit',1000 if action=='list_dir' else 100);cursor=args.get('cursor')
    criteria={k:v for k,v in args.items() if k not in ('limit','cursor')}
    criteria['path']=str(root)
    folder=u.mutation_path(str((Path(state)/'directory_pages').absolute()),config);folder.mkdir(parents=True,exist_ok=True)
    if cursor:
        match=re.fullmatch(r'([a-f0-9]{32}):([0-9]{1,7})',cursor)
        if not match:raise u.ActionError('invalid_directory_cursor')
        uid,offset=match.group(1),int(match.group(2))
        try:page=json.loads((folder/(uid+'.json')).read_text(encoding='utf-8'))
        except (OSError,ValueError):raise u.ActionError('directory_cursor_not_found_in_scope')
        if time.time()>page['expiresAt']:raise u.ActionError('directory_cursor_expired')
        if page['criteria']!=criteria or page['action']!=action:raise u.ActionError('directory_cursor_filter_conflict')
        if action=='list_dir' and page['rootVersion']!=version(root):raise u.ActionError('directory_changed_restart_listing')
    else:
        existing=list(folder.glob('*.json'))
        for old in existing:
            if not re.fullmatch(r'[a-f0-9]{32}\.json',old.name) or old.is_symlink():continue
            try:
                if json.loads(old.read_text(encoding='utf-8'))['expiresAt']<time.time():old.unlink()
            except (OSError,ValueError,KeyError):pass
        if len(list(folder.glob('*.json')))>=64:raise u.ActionError('search_cursor_capacity_retry_after_expiry')
        start_version=version(root)
        uid=uuid.uuid4().hex;offset=0;rows=[];scanned=0;unreadable=0
        stack=[root];deadline=time.monotonic()+10
        pattern=args.get('pattern','*').lower();extensions={s.lower().lstrip('.') for s in args.get('extensions',[])}
        while stack:
            directory=stack.pop()
            try:
                with os.scandir(directory) as entries:
                    for entry in entries:
                        scanned+=1
                        if scanned>50000 or time.monotonic()>deadline:raise u.ActionError('search_too_broad_choose_narrower_root')
                        try:
                            info=entry.stat(follow_symlinks=False)
                            link=entry.is_symlink() or bool(getattr(info,'st_file_attributes',0)&0x400)
                            isdir=stat.S_ISDIR(info.st_mode)
                            if action=='find_files':
                                if isdir:
                                    if args.get('recursive',True) and not link:stack.append(Path(entry.path))
                                    continue
                                if link or not stat.S_ISREG(info.st_mode):continue
                                if not fnmatch.fnmatchcase(entry.name.lower(),pattern):continue
                                if extensions and Path(entry.name).suffix.lower().lstrip('.') not in extensions:continue
                            rows.append({'name':entry.name,'path':entry.path,'directory':isdir,'bytes':info.st_size if not isdir else None,'modifiedAtUnix':info.st_mtime,'symlink':link})
                        except OSError:unreadable+=1
            except (PermissionError,FileNotFoundError):unreadable+=1
            if action=='list_dir':break
        if action=='list_dir' and version(root)!=start_version:raise u.ActionError('directory_changed_restart_listing')
        rows.sort(key=lambda row:(row['path'].lower(),row['path']))
        page={'action':action,'criteria':criteria,'entries':rows,'rootVersion':version(root),'createdAt':time.time(),'expiresAt':time.time()+600,'unreadable':unreadable,'scanned':scanned}
        u.durable_json(folder/(uid+'.json'),page)
        # Bound retained cursor snapshots; cleanup only owned regular metadata.
        for old in folder.glob('*.json'):
            if not re.fullmatch(r'[a-f0-9]{32}\.json',old.name) or old.is_symlink():continue
            try:
                if json.loads(old.read_text(encoding='utf-8'))['expiresAt']<time.time():old.unlink()
            except (OSError,ValueError,KeyError):pass
    rows=page['entries']
    if offset>len(rows):raise u.ActionError('directory_cursor_offset_invalid')
    end=min(len(rows),offset+limit)
    return {'path':str(root),'entries':rows[offset:end],'totalEntries':len(rows),'nextCursor':uid+':'+str(end) if end<len(rows) else None,'truncated':end<len(rows),'snapshotAtUnix':page['createdAt'],'expiresAtUnix':page['expiresAt'],'scannedEntries':page['scanned'],'unreadableEntries':page['unreadable']}

def version(path):
    info=path.stat();return [info.st_mtime_ns,info.st_ino]
