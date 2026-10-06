"""Detached, bounded process owner. No network or arbitrary remote API.

Tickets contain private command/output data. Callers must keep them on the PC
and in the authenticated PRIVATE relay, never the public communication hub.
"""
import ctypes
import hashlib
import json
import os
from pathlib import Path
import signal
import subprocess
import sys
import time
import uuid

def replace_state_file(temporary,path):
    """Windows readers can briefly deny DELETE sharing on JSON snapshots."""
    for attempt in range(60):
        try:os.replace(temporary,path);return
        except OSError as error:
            if os.name!='nt' or getattr(error,'winerror',None) not in (5,32,33) or attempt==59:raise
            time.sleep(min(.002*(2**min(attempt,5)),.05))

def atomic(path, value):
    path=Path(path); path.parent.mkdir(parents=True,exist_ok=True)
    temporary=path.with_name(path.name+'.'+uuid.uuid4().hex+'.tmp')
    try:
        temporary.write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf-8')
        replace_state_file(temporary,path)
    finally:
        if temporary.exists():temporary.unlink()

def load(path, default=None):
    try:return json.loads(Path(path).read_text(encoding='utf-8-sig'))
    except (OSError,ValueError):return default

def birth(pid):
    """Kernel creation identity; PID alone never establishes ownership."""
    if os.name=='nt':
        kernel=ctypes.WinDLL('kernel32',use_last_error=True)
        kernel.OpenProcess.argtypes=[ctypes.c_uint32,ctypes.c_int,ctypes.c_uint32]
        kernel.OpenProcess.restype=ctypes.c_void_p
        kernel.GetProcessTimes.argtypes=[ctypes.c_void_p,*([ctypes.POINTER(ctypes.c_uint64)]*4)]
        kernel.GetProcessTimes.restype=ctypes.c_int
        kernel.CloseHandle.argtypes=[ctypes.c_void_p]
        handle=kernel.OpenProcess(0x1000,False,int(pid))
        if not handle:return None
        try:
            creation,exit_time,kernel_time,user_time=(ctypes.c_uint64() for _ in range(4))
            if not kernel.GetProcessTimes(handle,ctypes.byref(creation),ctypes.byref(exit_time),ctypes.byref(kernel_time),ctypes.byref(user_time)):return None
            return str(creation.value)
        finally:kernel.CloseHandle(handle)
    try:
        # Some test/work runtimes virtualize Python PIDs while mounting a host
        # /proc. Never mistake a host process with the same integer for ours.
        if int(os.readlink('/proc/self'))!=os.getpid():return None
        # comm can contain spaces/parentheses; split after the final closing ).
        tail=Path('/proc/'+str(int(pid))+'/stat').read_text().rsplit(')',1)[1].split()
        return tail[19] # field 22, starttime
    except (OSError,ValueError,IndexError):return None

def alive(pid, expected_birth):
    if type(pid) is not int or pid<=0 or expected_birth is None:return False
    if os.name!='nt':return birth(pid)==str(expected_birth)
    kernel=ctypes.WinDLL('kernel32',use_last_error=True)
    kernel.OpenProcess.argtypes=[ctypes.c_uint32,ctypes.c_int,ctypes.c_uint32];kernel.OpenProcess.restype=ctypes.c_void_p
    kernel.WaitForSingleObject.argtypes=[ctypes.c_void_p,ctypes.c_uint32];kernel.WaitForSingleObject.restype=ctypes.c_uint32
    kernel.CloseHandle.argtypes=[ctypes.c_void_p]
    handle=kernel.OpenProcess(0x100000|0x1000,False,pid)
    if not handle:return False
    try:
        # Creation time remains readable after exit while a retained Popen
        # handle keeps the kernel object alive. It does not prove liveness.
        return kernel.WaitForSingleObject(handle,0)==258 and birth(pid)==str(expected_birth) and kernel.WaitForSingleObject(handle,0)==258
    finally:kernel.CloseHandle(handle)

class WindowsJob:
    """A new unnamed job contains this runner's child tree only."""
    def __init__(self, child):
        self.handle=None; self.attached=False; self.info=None
        if os.name!='nt':return
        from ctypes import wintypes
        class Basic(ctypes.Structure):
            _fields_=[('PerProcessUserTimeLimit',ctypes.c_int64),('PerJobUserTimeLimit',ctypes.c_int64),
                ('LimitFlags',wintypes.DWORD),('MinimumWorkingSetSize',ctypes.c_size_t),
                ('MaximumWorkingSetSize',ctypes.c_size_t),('ActiveProcessLimit',wintypes.DWORD),
                ('Affinity',ctypes.c_size_t),('PriorityClass',wintypes.DWORD),('SchedulingClass',wintypes.DWORD)]
        class IO(ctypes.Structure):
            _fields_=[(name,ctypes.c_uint64) for name in ('ReadOperationCount','WriteOperationCount',
                'OtherOperationCount','ReadTransferCount','WriteTransferCount','OtherTransferCount')]
        class Extended(ctypes.Structure):
            _fields_=[('BasicLimitInformation',Basic),('IoInfo',IO),('ProcessMemoryLimit',ctypes.c_size_t),
                ('JobMemoryLimit',ctypes.c_size_t),('PeakProcessMemoryUsed',ctypes.c_size_t),('PeakJobMemoryUsed',ctypes.c_size_t)]
        try:
            self.kernel=ctypes.WinDLL('kernel32',use_last_error=True)
            self.kernel.CreateJobObjectW.argtypes=[ctypes.c_void_p,wintypes.LPCWSTR];self.kernel.CreateJobObjectW.restype=wintypes.HANDLE
            self.kernel.SetInformationJobObject.argtypes=[wintypes.HANDLE,ctypes.c_int,ctypes.c_void_p,wintypes.DWORD]
            self.kernel.AssignProcessToJobObject.argtypes=[wintypes.HANDLE,wintypes.HANDLE]
            self.kernel.TerminateJobObject.argtypes=[wintypes.HANDLE,wintypes.UINT]
            self.kernel.CloseHandle.argtypes=[wintypes.HANDLE]
            self.handle=self.kernel.CreateJobObjectW(None,None)
            info=Extended();info.BasicLimitInformation.LimitFlags=0x2000 # KILL_ON_JOB_CLOSE
            self.info=info
            if not self.handle or not self.kernel.SetInformationJobObject(self.handle,9,ctypes.byref(info),ctypes.sizeof(info)):
                self.close();return
            self.attached=bool(self.kernel.AssignProcessToJobObject(self.handle,int(child._handle)))
            if not self.attached:self.close()
        except (OSError,AttributeError):self.close()
    def terminate(self):
        if self.handle and self.attached:self.kernel.TerminateJobObject(self.handle,1)
    def preserve_descendants(self):
        # A successful launcher can exit while its GUI/service keeps running.
        # Retain kill-on-close during execution/crash/stop, then remove only our
        # own flag before normal successful completion closes the unnamed job.
        if not self.handle or not self.attached:return True
        previous=self.info.BasicLimitInformation.LimitFlags
        self.info.BasicLimitInformation.LimitFlags=previous & ~0x2000
        if self.kernel.SetInformationJobObject(self.handle,9,ctypes.byref(self.info),ctypes.sizeof(self.info)):return True
        self.info.BasicLimitInformation.LimitFlags=previous
        return False
    def close(self):
        if self.handle:self.kernel.CloseHandle(self.handle);self.handle=None

def stop_owned(child, child_birth, job):
    if child.poll() is not None:return False
    if not alive(child.pid,child_birth):
        if os.name=='nt' or child_birth is not None:return False
        # No public stop-by-PID fallback. A POSIX Popen owner can still stop
        # its exact unreaped child when a foreign /proc cannot supply birth.
        child.terminate();child.wait(timeout=5);return True
    if os.name=='nt':
        if job.attached:job.terminate()
        else:child.terminate() # Popen retains this exact owned process handle.
    else:
        # Child was created with a new session; only its own process group.
        try:os.killpg(child.pid,signal.SIGTERM)
        except ProcessLookupError:return False
    try:child.wait(timeout=4)
    except subprocess.TimeoutExpired:
        if alive(child.pid,child_birth):
            if os.name=='nt':child.kill()
            else:
                try:os.killpg(child.pid,signal.SIGKILL)
                except ProcessLookupError:pass
        child.wait(timeout=5)
    return True

def run(folder):
    folder=Path(folder);ticket=load(folder/'ticket.json')
    if not isinstance(ticket,dict):return 2
    identity={'processId':ticket['processId'],'ownerNonce':ticket['ownerNonce'],
        'runnerPid':os.getpid(),'runnerBirth':birth(os.getpid()),'startedAt':time.time()}
    atomic(folder/'status.json',{**identity,'stage':'starting'})
    child=None;job=None
    try:
        environment=dict(os.environ,GIT_TERMINAL_PROMPT='0',GCM_INTERACTIVE='never',PYTHONIOENCODING='utf-8')
        with (folder/'stdout.log').open('wb') as stdout,(folder/'stderr.log').open('wb') as stderr:
            child=subprocess.Popen(ticket['argv'],cwd=ticket['cwd'],env=environment,
                stdin=subprocess.DEVNULL,stdout=stdout,stderr=stderr,shell=False,
                creationflags=0x08000000 if os.name=='nt' else 0,start_new_session=os.name!='nt')
            child_birth=birth(child.pid);job=WindowsJob(child)
            identity.update(childPid=child.pid,childBirth=child_birth,
                ownedDescendantStop=job.attached if os.name=='nt' else child_birth is not None)
            atomic(folder/'status.json',{**identity,'stage':'running'})
            deadline=time.monotonic()+ticket['timeoutSeconds'];reason='completed'
            while child.poll() is None:
                if (folder/'stop.flag').exists():reason='stopped';stop_owned(child,child_birth,job);break
                if time.monotonic()>=deadline:reason='timed_out';stop_owned(child,child_birth,job);break
                time.sleep(.15)
            identity['phase']='child_exited';atomic(folder/'status.json',{**identity,'stage':'running'})
            code=child.wait(timeout=5)
            identity['phase']='preserving_descendants';atomic(folder/'status.json',{**identity,'stage':'running'})
            preserved=job.preserve_descendants() if reason=='completed' and code==0 else False
            result={**identity,'stage':reason,'returnCode':code,
                'descendantsPreserved':preserved,'finishedAt':time.time()}
            if reason=='completed' and code==0 and not preserved:
                result.update(stage='failed',error='descendant_preservation_failed')
            atomic(folder/'status.json',result)
        return 0
    except Exception as exc:
        if child is not None and job is not None:stop_owned(child,identity.get('childBirth'),job)
        atomic(folder/'status.json',{**identity,'stage':'failed','error':'runner_execution_failed','errorType':type(exc).__name__,'finishedAt':time.time()})
        return 1
    finally:
        if job is not None:job.close()

if __name__=='__main__':
    if len(sys.argv)!=2:sys.exit(2)
    sys.exit(run(sys.argv[1]))
