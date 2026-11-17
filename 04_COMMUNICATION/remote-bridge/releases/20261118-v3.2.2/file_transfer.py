"""Bounded, resumable binary uploads. State is isolated by worker project/tool scope."""
import base64, hashlib, json, os, shutil, time, uuid
from pathlib import Path

ACTIONS = ('file_info','upload_begin','upload_chunk','upload_status','upload_commit','upload_abort')

def perform(action, args, config, state):
    import universal_actions as u
    fields = {
        'file_info': {'path','includeSha256'},
        'upload_begin': {'path','expectedSha256','totalBytes','sha256'},
        'upload_chunk': {'uploadId','offsetBytes','contentBase64','chunkSha256'},
        'upload_status': {'uploadId'}, 'upload_commit': {'uploadId'}, 'upload_abort': {'uploadId'}}
    if set(args)-fields[action]: raise u.ActionError('unknown_transfer_field')
    state = Path(state)
    if action == 'file_info':
        path = u.absolute_path(args.get('path'))
        include = args.get('includeSha256', True)
        if type(include) is not bool: raise u.ActionError('invalid_include_hash')
        if not path.is_file(): raise u.ActionError('file_not_found')
        before = path.stat()
        digest = u.file_sha(path) if include else None
        after = path.stat()
        if stamp(before) != stamp(after): raise u.ActionError('file_changed_during_read')
        return {'path':str(path),'totalBytes':after.st_size,'sha256':digest,'fileVersion':stamp(after)}
    state=u.mutation_path(str(state.absolute()),config)
    folder=u.mutation_path(str(state/'uploads'),config); folder.mkdir(parents=True, exist_ok=True)
    if action == 'upload_begin':
        if set(args) != fields[action]: raise u.ActionError('upload_preconditions_required')
        path = u.mutation_path(args['path'], config)
        total = args['totalBytes']; limit = config.get('maxUploadBytes',20*1024**3)
        if type(limit) is not int or not 1 <= limit <= 1024**4: raise u.ActionError('invalid_upload_limit')
        if type(total) is not int or not 0 <= total <= limit: raise u.ActionError('upload_too_large')
        if not isinstance(args['sha256'],str) or not u.HASH.fullmatch(args['sha256']): raise u.ActionError('invalid_upload_hash')
        expected = args['expectedSha256']
        if expected is not None and (not isinstance(expected,str) or not u.HASH.fullmatch(expected)): raise u.ActionError('invalid_expected_hash')
        with u.file_lock(folder/'quota.lock'):
            rows = [json.loads(p.read_text(encoding='utf-8')) for p in folder.glob('*.json')]
            active = [r for r in rows if r['stage'] in ('uploading','committing')]
            if len(active)>=8 or sum(r['totalBytes'] for r in active)+total>limit: raise u.ActionError('upload_quota_exceeded_abort_old_sessions')
            if u.file_sha(path)!=expected or (path.exists() and not path.is_file()): raise u.ActionError('file_sha_conflict')
            path.parent.mkdir(parents=True,exist_ok=True)
            if shutil.disk_usage(path.parent).free < total+64*1024**2: raise u.ActionError('insufficient_disk_space')
            uid = uuid.uuid4().hex
            part = path.with_name('.'+path.name+'.bridge-upload-'+uid+'.part')
            with part.open('xb') as out: out.flush(); os.fsync(out.fileno())
            row = {**args,'uploadId':uid,'part':str(part),'stage':'uploading','expiresAt':time.time()+7*86400}
            try: u.durable_json(folder/(uid+'.json'),row)
            except BaseException: part.unlink(missing_ok=True); raise
            return summary(row,0)
    uid = args.get('uploadId')
    if not isinstance(uid,str) or len(uid)!=32 or any(c not in '0123456789abcdef' for c in uid): raise u.ActionError('invalid_upload_id')
    meta = folder/(uid+'.json')
    with u.file_lock(folder/(uid+'.lock')):
        if not meta.is_file(): raise u.ActionError('upload_not_found_in_scope')
        row = json.loads(meta.read_text(encoding='utf-8'))
        path = u.mutation_path(row['path'],config)
        part = u.mutation_path(row['part'],config)
        if part != path.with_name('.'+path.name+'.bridge-upload-'+uid+'.part'): raise u.ActionError('invalid_upload_state')
        received = part.stat().st_size if part.is_file() else 0
        if row['stage']=='committing':
            # Recover a crash between atomic publication and metadata completion.
            same=part.is_file() and path.is_file() and os.path.samefile(part,path)
            if path.is_file() and u.file_sha(path)==row['sha256'] and (not part.exists() or same):
                if same: part.unlink()
                row.update(stage='completed',receivedBytes=row['totalBytes']);u.durable_json(meta,row)
            elif part.is_file() and u.file_sha(path)==row['expectedSha256']:
                row['stage']='uploading';u.durable_json(meta,row)
            else: raise u.ActionError('upload_commit_recovery_conflict')
        if action=='upload_status': return summary(row,received if row['stage']=='uploading' else row.get('receivedBytes',0))
        if action=='upload_abort':
            if row['stage']=='completed': raise u.ActionError('upload_already_committed')
            part.unlink(missing_ok=True); row['stage']='aborted'; row['receivedBytes']=received
            u.durable_json(meta,row); return summary(row,received)
        if row['stage']=='completed' and action=='upload_commit':
            if u.file_sha(path)!=row['sha256']: raise u.ActionError('committed_file_changed')
            return summary(row,row['totalBytes'])
        if row['stage']!='uploading': raise u.ActionError('upload_not_active')
        if time.time()>row['expiresAt']: raise u.ActionError('upload_expired_abort_required')
        if not part.is_file(): raise u.ActionError('upload_part_missing')
        if action=='upload_chunk':
            if set(args)!=fields[action]: raise u.ActionError('chunk_fields_required')
            offset=args['offsetBytes']; encoded=args['contentBase64']
            if type(offset) is not int or offset<0: raise u.ActionError('invalid_offset')
            if not isinstance(encoded,str) or len(encoded)>((u.MAX_BYTES+2)//3)*4: raise u.ActionError('chunk_too_large')
            try: data=base64.b64decode(encoded,validate=True)
            except ValueError: raise u.ActionError('invalid_content')
            if not 0<len(data)<=u.MAX_BYTES: raise u.ActionError('invalid_chunk_size')
            if u.sha(data)!=args['chunkSha256']: raise u.ActionError('chunk_hash_mismatch')
            if offset+len(data)>row['totalBytes']: raise u.ActionError('chunk_beyond_total')
            if offset<received:
                with part.open('rb') as inp: inp.seek(offset); previous=inp.read(len(data))
                if offset+len(data)>received or previous!=data: raise u.ActionError('chunk_retry_conflict')
                return {**summary(row,received),'replayed':True}
            if offset!=received: raise u.ActionError('chunk_offset_conflict')
            with part.open('r+b') as out: out.seek(offset); out.write(data); out.flush(); os.fsync(out.fileno())
            return summary(row,received+len(data))
        if received!=row['totalBytes']: raise u.ActionError('upload_incomplete')
        if u.file_sha(part)!=row['sha256']: raise u.ActionError('upload_hash_mismatch')
        identity=u.sha(os.path.normcase(str(path)).encode())
        with u.file_lock(state/'file_locks'/(identity+'.lock')):
            current=u.file_sha(path)
            if current!=row['expectedSha256'] or (path.exists() and not path.is_file()): raise u.ActionError('file_sha_conflict')
            backup=None
            if current is not None:
                backup=state/'file_backups'/identity/(str(time.time_ns())+'-'+uuid.uuid4().hex[:8]+'.bak')
                backup.parent.mkdir(parents=True,exist_ok=True); shutil.copy2(path,backup)
                u.durable_json(backup.with_suffix('.json'),{'path':str(path),'sha256':current,'createdAt':time.time()})
            u.mutation_path(str(path),config); u.mutation_path(str(part),config)
            if u.file_sha(path)!=current: raise u.ActionError('file_changed_during_write')
            row.update(stage='committing',receivedBytes=received,backup=str(backup) if backup else None)
            u.durable_json(meta,row)
            if current is None:
                # Exclusive publication: an external new file is never overwritten.
                os.link(part,path); part.unlink()
            else: os.replace(part,path)
            row.update(stage='completed',receivedBytes=received,backup=str(backup) if backup else None)
            u.durable_json(meta,row)
        return summary(row,received)

def stamp(info):
    return [info.st_size,info.st_mtime_ns,info.st_ctime_ns,info.st_ino]

def summary(row,received):
    return {k:row.get(k) for k in ('uploadId','path','totalBytes','sha256','stage','expiresAt','backup')} | {'receivedBytes':received,'nextOffsetBytes':received,'maxChunkBytes':512*1024}
