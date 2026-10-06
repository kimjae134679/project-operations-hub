using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

class BridgeLauncher : Form {
    static EventWaitHandle showSignal;
    string home, state, logon; Process worker;
    bool stopped=false, ending=false; int crashes=0; DateTime nextStart=DateTime.MinValue;
    TextBox details; Label status; NotifyIcon tray; System.Windows.Forms.Timer timer;
    JavaScriptSerializer json = new JavaScriptSerializer();
    [DllImport("advapi32.dll", SetLastError=true)] static extern bool OpenProcessToken(IntPtr p, uint a, out IntPtr t);
    [DllImport("advapi32.dll", SetLastError=true)] static extern bool GetTokenInformation(IntPtr t,int c,IntPtr b,int l,out int n);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
    static string LogonId() {
        IntPtr token=IntPtr.Zero, buf=IntPtr.Zero;
        try {
            if(!OpenProcessToken(Process.GetCurrentProcess().Handle,8,out token)) throw new Exception();
            int n; GetTokenInformation(token,10,IntPtr.Zero,0,out n); buf=Marshal.AllocHGlobal(n);
            if(!GetTokenInformation(token,10,buf,n,out n)) throw new Exception();
            // TOKEN_STATISTICS: TokenId LUID followed by AuthenticationId LUID.
            return Marshal.ReadInt32(buf,12).ToString("x8")+Marshal.ReadInt32(buf,8).ToString("x8");
        } catch { return Environment.UserName+"_"+Process.GetCurrentProcess().SessionId; }
        finally { if(buf!=IntPtr.Zero) Marshal.FreeHGlobal(buf); if(token!=IntPtr.Zero) CloseHandle(token); }
    }
    public BridgeLauncher(bool background) {
        home=AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar); state=Path.Combine(home,"state");
        Directory.CreateDirectory(state); logon=LogonId();
        string prior=Path.Combine(state,"stopped_logon.txt");
        stopped=File.Exists(prior)&&File.ReadAllText(prior)==logon;
        if(!stopped) { Delete("stop.flag"); Delete("disconnected.flag"); }
        Text="프로젝트 연결"; Width=780; Height=580; MinimumSize=new Size(650,460);
        Font=new Font("Malgun Gothic",10); BackColor=Color.FromArgb(246,248,250);
        status=new Label { Dock=DockStyle.Top,Height=52,Padding=new Padding(18,16,0,0),Text="연결 준비 중" };
        details=new TextBox { Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BorderStyle=BorderStyle.None,BackColor=Color.White };
        var buttons=new FlowLayoutPanel { Dock=DockStyle.Bottom,Height=66,Padding=new Padding(12) };
        AddButton(buttons,"연결 / 재개",delegate { stopped=false; crashes=0; Delete("stop.flag"); Delete("disconnected.flag"); Delete("stopped_logon.txt"); nextStart=DateTime.MinValue; StartWorker(); });
        AddButton(buttons,"연결 일시정지",delegate { File.WriteAllText(Path.Combine(state,"disconnected.flag"),"paused locally"); });
        AddButton(buttons,"연결 종료",delegate { Stop(); });
        AddButton(buttons,"듣기 폴더",delegate { var cfg=Config(); string root=(string)cfg["projectRoot"]; string shelf=Path.Combine(root,"00_듣기"); Process.Start(new ProcessStartInfo { FileName=Directory.Exists(shelf)?shelf:root,UseShellExecute=true }); });
        AddButton(buttons,"기록 폴더",delegate { Process.Start(new ProcessStartInfo { FileName=state,UseShellExecute=true }); });
        AddButton(buttons,"통합소통",delegate {
            string tower=@"D:\A_KJ\AI\Applications\AIControlTower\AIControlTower.exe";
            Process.Start(new ProcessStartInfo { FileName=File.Exists(tower)?tower:"https://github.com/kimjae134679/project-operations-hub/tree/main/04_COMMUNICATION/remote-bridge",UseShellExecute=true });
        });
        Controls.Add(details); Controls.Add(status); Controls.Add(buttons);
        tray=new NotifyIcon { Icon=SystemIcons.Application,Text="프로젝트 연결",Visible=true };
        var menu=new ContextMenuStrip(); menu.Items.Add("창 열기",null,delegate { Show(); WindowState=FormWindowState.Normal; Activate(); });
        menu.Items.Add("연결 종료 후 닫기",null,delegate { Stop(); ending=true; Close(); }); tray.ContextMenuStrip=menu;
        tray.DoubleClick+=delegate { Show(); WindowState=FormWindowState.Normal; Activate(); };
        timer=new System.Windows.Forms.Timer { Interval=2000 }; timer.Tick+=delegate { Tick(); }; timer.Start();
        Shown+=delegate { if(background) Hide(); if(!stopped) StartWorker(); Tick(); };
        FormClosing+=delegate(object sender,FormClosingEventArgs e) {
            if(e.CloseReason==CloseReason.WindowsShutDown||e.CloseReason==CloseReason.TaskManagerClosing) ending=true;
            if(!ending) { e.Cancel=true; Hide(); return; }
            timer.Stop(); File.WriteAllText(Path.Combine(state,"stop.flag"),"application shutdown"); tray.Dispose();
        };
    }
    protected override void WndProc(ref Message m) {
        if(m.Msg==0x0011) { ending=true; File.WriteAllText(Path.Combine(state,"stop.flag"),"Windows shutdown"); m.Result=new IntPtr(1); return; }
        if(m.Msg==0x0016&&m.WParam!=IntPtr.Zero) { ending=true; timer.Stop(); }
        base.WndProc(ref m);
    }
    Dictionary<string,object> Config() { return json.Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(home,"config.json"))); }
    void AddButton(FlowLayoutPanel p,string text,Action action) { var b=new Button { Text=text,AutoSize=true,Height=36,Margin=new Padding(4) }; b.Click+=delegate { try { action(); } catch { status.Text="로컬 작업 실패 · 기록 폴더를 확인하세요"; } }; p.Controls.Add(b); }
    void Delete(string name) { string p=Path.Combine(state,name); if(File.Exists(p)) File.Delete(p); }
    void Stop() { stopped=true; File.WriteAllText(Path.Combine(state,"stopped_logon.txt"),logon); File.WriteAllText(Path.Combine(state,"stop.flag"),"stopped by local user"); status.Text="연결 종료 요청 · 현재 작업이 끝나면 종료됩니다"; }
    void StartWorker() {
        if(stopped||ending||DateTime.Now<nextStart) return;
        if(worker!=null&&!worker.HasExited) return;
        // Adopt only this launcher's recorded child with the exact birth time.
        // Never terminate or adopt a process based on a name or PID alone.
        try {
            string ticket=Path.Combine(state,"launcher_child.json");
            if(File.Exists(ticket)) {
                var old=json.Deserialize<Dictionary<string,object>>(File.ReadAllText(ticket));
                var child=Process.GetProcessById(Convert.ToInt32(old["pid"]));
                if(!child.HasExited&&child.StartTime.ToUniversalTime().ToString("o")==Convert.ToString(old["startedAt"])&&Convert.ToString(old["logonId"])==logon) { worker=child; return; }
            }
        } catch {}
        var cfg=Config(); string python=(string)cfg["python"], pyw=Path.Combine(Path.GetDirectoryName(python),"pythonw.exe");
        if(File.Exists(pyw)) python=pyw;
        var start=new ProcessStartInfo { FileName=python,Arguments="\""+Path.Combine(home,"bridge_worker.py")+"\" --home \""+home+"\"",WorkingDirectory=home,UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden };
        worker=Process.Start(start); File.WriteAllText(Path.Combine(state,"launcher_child.json"),json.Serialize(new { pid=worker.Id,startedAt=worker.StartTime.ToUniversalTime().ToString("o"),logonId=logon }));
    }
    void Tick() {
        try {
            if(showSignal!=null&&showSignal.WaitOne(0)) { Show(); WindowState=FormWindowState.Normal; Activate(); }
            if(!stopped&&!ending&&worker!=null&&worker.HasExited) {
                if(crashes<5) { crashes++; worker=null; nextStart=DateTime.Now.AddSeconds(30); }
                else { stopped=true; status.Text="연속 오류로 중지 · 연결 / 재개를 눌러 다시 시도하세요"; }
            }
            if(!stopped&&!ending&&worker==null) StartWorker();
            string general=Path.Combine(state,"universal_status.json");
            string path=File.Exists(general)?general:Path.Combine(state,"ui_status.json");
            if(File.Exists(path)) {
                var row=json.Deserialize<Dictionary<string,object>>(File.ReadAllText(path));
                string stage=row.ContainsKey("stage")?(string)row["stage"]:"unknown";
                var names=new Dictionary<string,string> { {"connected","공용 PC 연결됨"},{"running","PC 작업 실행 중"},{"disconnected","연결 일시정지"},{"stopped","연결 종료됨"},{"error","PC 연결 확인 필요"} };
                status.Text=stopped?"연결 종료 상태 · 다음 로그인 때 다시 시작":(names.ContainsKey(stage)?names[stage]:stage);
                details.Text=Pretty(row);
            }
        } catch { status.Text="상태 갱신 중"; }
    }
    string Pretty(Dictionary<string,object> row) {
        string result="여러 프로젝트의 PC 작업을 통합소통으로 연결합니다.\r\n상세 명령·로그·화면은 비공개 작업 통로에 보관합니다.\r\n\r\n";
        var labels=new Dictionary<string,string> { {"device","연결 PC"},{"updatedAt","상태 확인 시각"},{"pendingJobs","대기 작업"},{"currentJob","진행 중 작업"},{"action","작업 종류"} };
        foreach(var label in labels) if(row.ContainsKey(label.Key)) result+=label.Value+": "+Convert.ToString(row[label.Key])+"\r\n";
        if(row.ContainsKey("code")) {
            string code=Convert.ToString(row["code"]);
            result+="\r\n"+(code=="github_login_required"?"PC의 GitHub 로그인이 필요합니다.":code=="private_channel_required"?"비공개 작업 통로 설정을 확인하세요.":"연결 문제를 확인하고 있습니다.")+"\r\n확인 코드: "+code+"\r\n";
        }
        result+="\r\n프로그램: "+home+"\r\n로그인 후 자동 시작 · 창의 X는 트레이로 접기\r\n완전히 끄려면 연결 종료를 사용하세요.";
        return result;
    }
    [STAThread] static void Main(string[] args) {
        bool created; using(var mutex=new Mutex(true,"Local\\ProjectBridge_"+LogonId(),out created)) {
            string eventName="Local\\ProjectBridge_Show_"+LogonId();
            if(!created) { try { using(var existing=EventWaitHandle.OpenExisting(eventName)) existing.Set(); } catch {} return; }
            using(showSignal=new EventWaitHandle(false,EventResetMode.AutoReset,eventName)) {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BridgeLauncher(Array.IndexOf(args,"--background")>=0));
            }
        }
    }
}
