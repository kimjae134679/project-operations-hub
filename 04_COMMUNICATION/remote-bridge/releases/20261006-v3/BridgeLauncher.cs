using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography;
using System.Text;

class BridgeLauncher : Form {
    static EventWaitHandle showSignal;
    string home,state,logon; Process worker; bool stopped,ending,launchHidden;
    DateTime nextStart=DateTime.MinValue; int crashes;
    Label status,meta; ListView jobs; NotifyIcon tray; System.Windows.Forms.Timer timer;
    JavaScriptSerializer json=new JavaScriptSerializer();
    [DllImport("advapi32.dll",SetLastError=true)] static extern bool OpenProcessToken(IntPtr p,uint a,out IntPtr t);
    [DllImport("advapi32.dll",SetLastError=true)] static extern bool GetTokenInformation(IntPtr t,int c,IntPtr b,int l,out int n);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
    static string LogonId() {
        IntPtr token=IntPtr.Zero,buf=IntPtr.Zero;
        try {
            if(!OpenProcessToken(Process.GetCurrentProcess().Handle,8,out token)) throw new Exception();
            int n;GetTokenInformation(token,10,IntPtr.Zero,0,out n);buf=Marshal.AllocHGlobal(n);
            if(!GetTokenInformation(token,10,buf,n,out n)) throw new Exception();
            return Marshal.ReadInt32(buf,12).ToString("x8")+Marshal.ReadInt32(buf,8).ToString("x8");
        } catch {return Environment.UserName+"_"+Process.GetCurrentProcess().SessionId;}
        finally {if(buf!=IntPtr.Zero)Marshal.FreeHGlobal(buf);if(token!=IntPtr.Zero)CloseHandle(token);}
    }
    static string InstanceKey() {
        string path=Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar).ToUpperInvariant();
        using(var hash=SHA256.Create())return LogonId()+"_"+BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(path))).Replace("-","").Substring(0,16);
    }
    public BridgeLauncher(bool background) {
        launchHidden=background;
        home=AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);state=Path.Combine(home,"state");
        Directory.CreateDirectory(state);logon=LogonId();File.WriteAllText(Path.Combine(state,"current_logon.txt"),logon);
        stopped=Stopped();if(!stopped){Delete("stop.flag");Delete("disconnected.flag");}
        Text="공용 PC 연결 · ProjectBridge 3.0";Width=850;Height=620;MinimumSize=new Size(720,480);
        Font=new Font("Malgun Gothic",10);BackColor=Color.FromArgb(245,247,250);Icon=SystemIcons.Application;
        status=new Label {Dock=DockStyle.Top,Height=68,Padding=new Padding(20,20,0,0),Font=new Font(Font.FontFamily,16,FontStyle.Bold),Text="연결 준비 중"};
        meta=new Label {Dock=DockStyle.Top,Height=112,Padding=new Padding(20,12,0,0),Text="로컬 도구와 원격 GPT 연결을 각각 확인합니다."};
        jobs=new ListView {Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,GridLines=false,BorderStyle=BorderStyle.None};
        jobs.Columns.Add("작업",210);jobs.Columns.Add("프로젝트",150);jobs.Columns.Add("도구",110);jobs.Columns.Add("상태",120);jobs.Columns.Add("작업 ID",200);
        var buttons=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=70,Padding=new Padding(12)};
        AddButton(buttons,"연결 켜기",delegate {Resume();});
        AddButton(buttons,"새 작업 일시정지",delegate {File.WriteAllText(Path.Combine(state,"disconnected.flag"),"paused locally");});
        AddButton(buttons,"연결 끄기",delegate {Stop();});
        AddButton(buttons,"통합 관리 열기",delegate {string p=@"D:\A_KJ\AI\Applications\AIControlTower\AIControlTower.exe";if(File.Exists(p))Process.Start(new ProcessStartInfo {FileName=p,UseShellExecute=true});});
        Controls.Add(jobs);Controls.Add(meta);Controls.Add(status);Controls.Add(buttons);
        tray=new NotifyIcon {Icon=SystemIcons.Application,Text="공용 PC 연결",Visible=true};
        var menu=new ContextMenuStrip();menu.Items.Add("연결 상태 보기",null,delegate {Show();WindowState=FormWindowState.Normal;Activate();});
        menu.Items.Add("연결 끄고 닫기",null,delegate {Stop();ending=true;Close();});tray.ContextMenuStrip=menu;
        tray.DoubleClick+=delegate {Show();WindowState=FormWindowState.Normal;Activate();};
        timer=new System.Windows.Forms.Timer {Interval=1000};timer.Tick+=delegate {Tick();};timer.Start();
        // Startup does not show a frame even briefly. Start the owned worker
        // directly; the message-loop timer and tray remain active while hidden.
        if(!stopped)StartWorker();Tick();
        FormClosing+=delegate(object sender,FormClosingEventArgs e) {
            if(e.CloseReason==CloseReason.WindowsShutDown||e.CloseReason==CloseReason.TaskManagerClosing)ending=true;
            if(!ending){e.Cancel=true;Hide();return;}
            timer.Stop();File.WriteAllText(Path.Combine(state,"stop.flag"),"application shutdown");tray.Dispose();
        };
    }
    protected override void SetVisibleCore(bool value) {
        if(launchHidden&&value){launchHidden=false;if(!IsHandleCreated)CreateHandle();base.SetVisibleCore(false);return;}
        base.SetVisibleCore(value);
    }
    protected override void WndProc(ref Message m) {
        if(m.Msg==0x0011){m.Result=new IntPtr(1);return;}
        if(m.Msg==0x0016&&m.WParam!=IntPtr.Zero){ending=true;timer.Stop();File.WriteAllText(Path.Combine(state,"stop.flag"),"Windows shutdown");}
        base.WndProc(ref m);
    }
    Dictionary<string,object> Config(){return json.Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(home,"config.json")));}
    string WorkerPath(){string p=Path.Combine(home,"Runtime","bridge_worker.py");return File.Exists(p)?p:Path.Combine(home,"bridge_worker.py");}
    void AddButton(FlowLayoutPanel p,string text,Action action){var b=new Button {Text=text,AutoSize=true,Height=38,FlatStyle=FlatStyle.Flat,BackColor=Color.White,Margin=new Padding(4)};b.Click+=delegate {try{action();}catch{status.Text="연결 작업 확인 필요";status.ForeColor=Color.Firebrick;}};p.Controls.Add(b);}
    void Delete(string name){string p=Path.Combine(state,name);if(File.Exists(p))File.Delete(p);}
    bool Stopped(){string p=Path.Combine(state,"stopped_logon.txt");return File.Exists(p)&&File.ReadAllText(p)==logon;}
    void Stop(){stopped=true;File.WriteAllText(Path.Combine(state,"stopped_logon.txt"),logon);File.WriteAllText(Path.Combine(state,"stop.flag"),"stopped by local user");Tick();}
    void Resume(){stopped=false;crashes=0;Delete("stop.flag");Delete("disconnected.flag");Delete("stopped_logon.txt");nextStart=DateTime.MinValue;StartWorker();}
    void StartWorker(){
        if(stopped||ending||DateTime.Now<nextStart)return;
        if(worker!=null&&!worker.HasExited)return;
        try {
            var old=json.Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(state,"launcher_child.json")));
            var child=Process.GetProcessById(Convert.ToInt32(old["pid"]));
            var cfg=Config();string python=(string)cfg["python"],pyw=Path.Combine(Path.GetDirectoryName(python),"pythonw.exe");
            string executable=child.MainModule.FileName;
            if(!child.HasExited&&child.StartTime.ToUniversalTime().ToString("o")==Convert.ToString(old["startedAt"])&&Convert.ToString(old["logonId"])==logon&&(String.Equals(executable,python,StringComparison.OrdinalIgnoreCase)||String.Equals(executable,pyw,StringComparison.OrdinalIgnoreCase))){worker=child;return;}
        }catch{}
        var config=Config();string exe=(string)config["python"],windowless=Path.Combine(Path.GetDirectoryName(exe),"pythonw.exe");if(File.Exists(windowless))exe=windowless;
        worker=Process.Start(new ProcessStartInfo {FileName=exe,Arguments="\""+WorkerPath()+"\" --home \""+home+"\"",WorkingDirectory=home,UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden});
        File.WriteAllText(Path.Combine(state,"launcher_child.json"),json.Serialize(new {pid=worker.Id,startedAt=worker.StartTime.ToUniversalTime().ToString("o"),logonId=logon}));
    }
    static object Value(Dictionary<string,object> row,string key,object fallback){object v;return row.TryGetValue(key,out v)?v:fallback;}
    void Tick(){
        try {
            bool external=Stopped();if(external!=stopped){stopped=external;crashes=0;nextStart=DateTime.MinValue;}
            if(showSignal!=null&&showSignal.WaitOne(0)){Show();WindowState=FormWindowState.Normal;Activate();}
            if(!stopped&&!ending&&worker!=null&&worker.HasExited){worker=null;crashes++;nextStart=DateTime.Now.AddSeconds(Math.Min(30,2*crashes));}
            if(!stopped&&!ending&&worker==null)StartWorker();
            var row=json.Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(state,"universal_status.json")));
            DateTime beat;bool fresh=DateTime.TryParse(Convert.ToString(Value(row,"updatedAt","")),out beat)&&DateTime.UtcNow-beat.ToUniversalTime()<TimeSpan.FromSeconds(12);
            string stage=stopped?"stopped":!fresh?"stale":Convert.ToString(Value(row,"stage","unknown"));
            var labels=new Dictionary<string,string>{{"connected","● 로컬 도구 연결됨"},{"running","● PC 작업 실행 중"},{"disconnected","● 새 작업 일시정지"},{"stopped","● 연결 꺼짐 · 다음 로그인에 다시 연결"},{"stale","● PC 응답 없음 · 자동 복구 중"}};
            status.Text=labels.ContainsKey(stage)?labels[stage]:"● 연결 확인 필요";
            status.ForeColor=stage=="connected"?Color.FromArgb(25,109,75):stage=="running"?Color.FromArgb(33,91,174):stage=="disconnected"||stage=="stopped"?Color.FromArgb(121,88,26):Color.FromArgb(177,49,49);
            status.BackColor=stage=="connected"?Color.FromArgb(229,245,237):stage=="running"?Color.FromArgb(229,238,255):Color.FromArgb(255,240,224);
            bool relay=Convert.ToBoolean(Value(row,"privateChannelAvailable",false));
            meta.Text="이 PC: "+Value(row,"device","확인 중")+"   ·   실행 "+Value(row,"runningJobs",0)+" / 병렬 "+Value(row,"maxWorkers",4)+"   ·   대기 "+Value(row,"pendingJobs",0)+"\r\n"+
                "원격 GPT 중계: "+(relay?"연결됨":"확인 필요 · 로컬 도구 연결과 별도")+"   "+Value(row,"relayCode","")+"\r\n"+
                "마지막 응답: "+Value(row,"updatedAt","")+"\r\n창의 X는 트레이로 숨깁니다. 연결 끄기로 이번 로그인 동안 중지합니다.";
            jobs.BeginUpdate();jobs.Items.Clear();object raw=Value(row,"jobs",null);var list=raw as System.Collections.IEnumerable;
            if(list!=null)foreach(object element in list){var j=element as Dictionary<string,object>;if(j==null)continue;string s=Convert.ToString(Value(j,"state",""));string outcome=Convert.ToString(Value(j,"outcome",""));
                var item=new ListViewItem(Convert.ToString(Value(j,"title",Value(j,"action","PC 작업"))));item.SubItems.Add(Convert.ToString(Value(j,"projectId","")));item.SubItems.Add(Convert.ToString(Value(j,"toolId","")));
                item.SubItems.Add(s=="started"?"실행 중":s=="queued"?"대기":outcome=="completed"?"완료":outcome=="interrupted"?"중단 · 확인 필요":"실패 · 확인 필요");item.SubItems.Add(Convert.ToString(Value(j,"id","")));
                item.ForeColor=s=="started"?Color.RoyalBlue:s=="queued"?Color.DarkGoldenrod:outcome=="completed"?Color.SeaGreen:Color.Firebrick;jobs.Items.Add(item);
            }jobs.EndUpdate();
        }catch{status.Text=stopped?"● 연결 꺼짐":"● 연결 준비 중 · 자동 확인";}
    }
    [STAThread]static void Main(string[] args){
        string stateDir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"state");Directory.CreateDirectory(stateDir);
        bool resume=Array.IndexOf(args,"--resume")>=0;
        if(Array.IndexOf(args,"--pause")>=0){File.WriteAllText(Path.Combine(stateDir,"disconnected.flag"),"paused by management program");return;}
        if(Array.IndexOf(args,"--stop")>=0){File.WriteAllText(Path.Combine(stateDir,"stopped_logon.txt"),LogonId());File.WriteAllText(Path.Combine(stateDir,"stop.flag"),"stopped by management program");return;}
        if(resume)foreach(string n in new[]{"stopped_logon.txt","stop.flag","disconnected.flag"}){string p=Path.Combine(stateDir,n);if(File.Exists(p))File.Delete(p);}
        bool created;using(var mutex=new Mutex(true,"Local\\ProjectBridge_"+InstanceKey(),out created)){
            string eventName="Local\\ProjectBridge_Show_"+InstanceKey();
            if(!created){if(!resume)try{using(var existing=EventWaitHandle.OpenExisting(eventName))existing.Set();}catch{}return;}
            using(showSignal=new EventWaitHandle(false,EventResetMode.AutoReset,eventName)){
                Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new BridgeLauncher(resume||Array.IndexOf(args,"--background")>=0));
            }
        }
    }
}
