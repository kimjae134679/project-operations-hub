package kr.co.kjw.videodownloader;

import android.app.*;
import android.content.*;
import android.database.Cursor;
import android.net.Uri;
import android.os.*;
import android.provider.MediaStore;
import android.webkit.MimeTypeMap;
import com.yausername.youtubedl_android.*;
import com.yausername.ffmpeg.FFmpeg;
import org.json.*;
import org.jsoup.Jsoup;
import org.jsoup.nodes.*;
import java.io.*;
import java.util.*;
import java.util.concurrent.*;
import java.util.regex.*;
import kotlin.Unit;

public class DownloadService extends Service {
    public static volatile boolean running=false;
    private static final Set<String> cancelled=ConcurrentHashMap.newKeySet();
    private static volatile String activeId="";
    private Thread worker;
    private PowerManager.WakeLock wake;
    private int startId;
    private long lastSave=0;
    public static synchronized void initialize(Context c) throws Exception { YoutubeDL.getInstance().init(c.getApplicationContext());FFmpeg.getInstance().init(c.getApplicationContext()); }
    public static void cancel(String id){JSONObject t=Store.task(id);if(t==null||"completed".equals(t.optString("status")))return;cancelled.add(id);Store.patch(id,Store.obj("status",id.equals(activeId)?"pausing":"paused","message",id.equals(activeId)?"안전하게 중지하는 중":"일시정지 · 이어받기 가능"));new Thread(()->{try{YoutubeDL.getInstance().destroyProcessById(id);}catch(Exception ignored){}},"cancel-download").start();}
    @Override public void onCreate(){super.onCreate();Store.init(this);NotificationManager m=getSystemService(NotificationManager.class);m.createNotificationChannel(new NotificationChannel("downloads","다운로드 진행",NotificationManager.IMPORTANCE_LOW));wake=((PowerManager)getSystemService(POWER_SERVICE)).newWakeLock(PowerManager.PARTIAL_WAKE_LOCK,"VideoDownloader:transfer");}
    private Notification notification(String text,int progress){Intent i=new Intent(this,MainActivity.class);PendingIntent p=PendingIntent.getActivity(this,0,i,PendingIntent.FLAG_UPDATE_CURRENT|PendingIntent.FLAG_IMMUTABLE);Notification.Builder b=new Notification.Builder(this,"downloads").setSmallIcon(android.R.drawable.stat_sys_download).setContentTitle("영상다운로더").setContentText(text).setContentIntent(p).setOngoing(true).setProgress(100,Math.max(0,progress),progress<0);if(!activeId.isEmpty()){Intent pause=new Intent(this,DownloadService.class).setAction("pause").putExtra("id",activeId);PendingIntent q=PendingIntent.getService(this,1,pause,PendingIntent.FLAG_UPDATE_CURRENT|PendingIntent.FLAG_IMMUTABLE);b.addAction(new Notification.Action.Builder(null,"일시정지",q).build());}return b.build();}
    @Override public synchronized int onStartCommand(Intent intent,int flags,int id){startId=id;if(intent!=null&&"pause".equals(intent.getAction()))cancel(intent.getStringExtra("id"));startForeground(1,notification("다운로드 준비 중",-1));if(worker==null||!worker.isAlive()){running=true;worker=new Thread(this::loop,"download-queue");worker.start();}return START_NOT_STICKY;}
    private void loop(){
        wake.acquire(6*60*60*1000L);
        try{initialize(this);String id;while((id=Store.next())!=null){activeId=id;cancelled.remove(id);try{download(id);}catch(Exception e){if(cancelled.contains(id))Store.patch(id,Store.obj("status","paused","message","일시정지 · 이어받기 가능"));else Store.patch(id,Store.obj("status","failed","message",friendly(e),"error",redact(e.toString())));}activeId="";}}
        catch(Exception e){String id;while((id=Store.next())!=null)Store.patch(id,Store.obj("status","failed","message","다운로드 엔진 준비 실패. 설정에서 엔진을 업데이트해 주세요.","error",redact(e.toString())));}
        finally{if(wake.isHeld())wake.release();synchronized(this){worker=null;if(Store.next()!=null){running=true;worker=new Thread(this::loop,"download-queue");worker.start();}else{running=false;stopForeground(STOP_FOREGROUND_REMOVE);stopSelfResult(startId);}}}
    }
    private void check(String id)throws InterruptedException{if(cancelled.contains(id)||Thread.currentThread().isInterrupted())throw new InterruptedException("cancelled");}
    private YoutubeDLRequest request(JSONObject task,String url){
        YoutubeDLRequest r=new YoutubeDLRequest(url);r.addOption("--no-mtime");r.addOption("--no-overwrites");r.addOption("--continue");r.addOption("--newline");r.addOption("--socket-timeout",20);r.addOption("--retries",5);r.addOption("--fragment-retries",5);r.addOption("--extractor-retries",3);r.addOption("--concurrent-fragments",3);r.addOption("--remote-components","ejs:github");
        r.addOption("--user-agent",task.optString("userAgent","Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 Chrome/131.0.0.0 Mobile Safari/537.36"));
        if(!task.optBoolean("playlist"))r.addOption("--no-playlist");
        String ref=task.optString("referer");if(LinkParser.valid(ref))r.addOption("--referer",ref);
        String cookies=task.optString("cookieFile");if(!cookies.isEmpty()&&new File(cookies).isFile())r.addOption("--cookies",cookies);
        return r;
    }
    private void progress(String id,float pct,long eta,String line){if(cancelled.contains(id))return;long now=System.currentTimeMillis();if(now-lastSave<600)return;lastSave=now;String text="다운로드 중";Matcher m=Pattern.compile("at\\s+(\\S+/s)").matcher(line==null?"":line);if(m.find())text+=" · "+m.group(1);if(eta>0)text+=" · 남은 시간 "+(eta/60)+"분 "+(eta%60)+"초";Store.patch(id,Store.obj("status","downloading","progress",Math.min(99,Math.max(0,pct)),"message",text));getSystemService(NotificationManager.class).notify(1,notification(text,(int)pct));}
    private String quality(JSONObject task){String h=task.optString("quality","best");return Arrays.asList("1080","720","480").contains(h)?"[height<="+h+"]":"";}
    private void download(String id)throws Exception{
        JSONObject task=Store.task(id);if(task==null)return;check(id);Store.patch(id,Store.obj("status","analyzing","message","영상 정보 확인 중","progress",0));
        // Update once per day; bundled engine remains available if update server is offline.
        android.content.SharedPreferences prefs=getSharedPreferences("engine",0);if(System.currentTimeMillis()-prefs.getLong("updated",0)>86400000L){Store.patch(id,Store.obj("message","다운로드 엔진 확인 중"));try{YoutubeDL.getInstance().updateYoutubeDL(this,YoutubeDL.UpdateChannel._STABLE);prefs.edit().putLong("updated",System.currentTimeMillis()).apply();}catch(Exception ignored){}check(id);}
        String original=task.optString("url");File stage=new File(getExternalFilesDir(null),"pending/"+id);if(!stage.exists()&&!stage.mkdirs())throw new IOException("저장 공간을 준비할 수 없습니다.");
        if(!task.optBoolean("playlist")){
            try{YoutubeDLRequest info=request(task,original);info.addOption("--skip-download");info.addOption("--dump-single-json");YoutubeDLResponse out=YoutubeDL.getInstance().execute(info,id,(p,e,l)->Unit.INSTANCE);JSONObject metadata=jsonLine(out.getOut());if(metadata!=null){String title=metadata.optString("title",task.optString("title"));Store.patch(id,Store.obj("title",title,"duration",metadata.optDouble("duration",0),"message","다운로드 준비"));}}catch(Exception e){check(id);}
        }
        ArrayList<String> candidates=new ArrayList<>();candidates.add(original);Exception error=null,partialError=null;List<File> outputs=null;
        for(int candidate=0;candidate<candidates.size()&&candidate<12;candidate++){
            String url=candidates.get(candidate);
            for(int attempt=0;attempt<2;attempt++){
                check(id);String mode=task.optString("format","mp4");String filter=quality(task);
                Store.patch(id,Store.obj("status","downloading","message",attempt==0?(candidate==0?"다운로드 시작":"발견한 미디어로 재시도"):"원본 포맷으로 다시 시도","attempt",attempt+1));
                YoutubeDLRequest r=request(task,url);r.addOption("--no-simulate");r.addOption("--progress");r.addOption("-o",new File(stage,"%(title).120B [%(id)s].%(ext)s").getAbsolutePath());r.addOption("--print","after_move:__FILE__%(filepath)j");r.addCommands(Arrays.asList("--print-to-file","after_move:%(filepath)j",new File(stage,"completed.jsonl").getAbsolutePath()));
                if("audio".equals(mode)){r.addOption("-f","bestaudio/best");if(attempt==0){r.addOption("-x");r.addOption("--audio-format","mp3");r.addOption("--audio-quality","0");}}
                else if(attempt==0&&"mp4".equals(mode)){r.addOption("-f","bv*[ext=mp4]"+filter+"+ba[ext=m4a]/b[ext=mp4]"+filter+"/bv*"+filter+"+ba/b"+filter+"/b");r.addOption("--merge-output-format","mp4/mkv");r.addOption("--remux-video","mp4/mkv");}
                else{r.addOption("-f","bv*"+filter+"+ba/b"+filter+"/b");r.addOption("--merge-output-format","mkv");}
                if(candidate>0&&!LinkParser.valid(task.optString("referer")))r.addOption("--referer",original);
                try{YoutubeDLResponse response=YoutubeDL.getInstance().execute(r,id,(p,e,l)->{progress(id,p,e,l);return Unit.INSTANCE;});check(id);outputs=reportedFiles(response.getOut(),stage);if(outputs.isEmpty())throw new IOException("저장된 미디어 파일이 없습니다.");break;}
                catch(Exception e){check(id);error=e;List<File> partial=manifestFiles(stage);if(!partial.isEmpty()){outputs=partial;partialError=e;break;}}
            }
            if(outputs!=null&&!outputs.isEmpty())break;
            if(candidate==0){Store.patch(id,Store.obj("status","analyzing","message","페이지와 외부 플레이어에서 미디어 찾는 중"));try{for(String u:discover(original,0,new HashSet<>()))if(!candidates.contains(u))candidates.add(u);}catch(Exception ignored){}check(id);}
        }
        if(outputs==null||outputs.isEmpty())throw error==null?new IOException("미디어를 찾지 못했습니다."):error;
        check(id);Store.patch(id,Store.obj("status","exporting","message","휴대폰 다운로드 폴더에 저장 중","progress",99));JSONArray files=new JSONArray();JSONArray prior=Store.task(id).optJSONArray("files");if(prior!=null)for(int i=0;i<prior.length();i++)files.put(prior.opt(i));
        // Each successfully exported file is checkpointed before continuing a playlist.
        for(File output:outputs){check(id);boolean already=false;for(int i=0;i<files.length();i++)if(output.getName().equals(files.optJSONObject(i).optString("source")))already=true;if(already)continue;JSONObject file=exportFile(output,id);files.put(file);Store.patch(id,Store.obj("files",files));}
        check(id);Store.patch(id,Store.obj("status",partialError==null?"completed":"partial","progress",partialError==null?100:0,"message",files.length()+"개 파일 저장"+(partialError==null?" 완료":" · 나머지는 재시도가 필요합니다."),"files",files,"error",partialError==null?"":redact(partialError.toString())));if(partialError==null){deleteStage(stage);String cookie=task.optString("cookieFile");if(!cookie.isEmpty())new File(cookie).delete();Store.patch(id,Store.obj("cookieFile",""));}
    }
    private JSONObject jsonLine(String out){String[] lines=out.split("\\r?\\n");for(int i=lines.length-1;i>=0;i--)try{if(lines[i].startsWith("{"))return new JSONObject(lines[i]);}catch(Exception ignored){}return null;}
    private List<File> reportedFiles(String out,File stage)throws Exception{ArrayList<File> result=new ArrayList<>();String root=stage.getCanonicalPath()+File.separator;for(String line:out.split("\\r?\\n")){if(line.startsWith("__FILE__")){String path=(String)new JSONTokener(line.substring(8)).nextValue();File f=new File(path);if(f.getCanonicalPath().startsWith(root)&&f.isFile()&&f.length()>0&&!result.contains(f))result.add(f);}}return result;}
    private List<File> manifestFiles(File stage){ArrayList<File> result=new ArrayList<>();try(BufferedReader r=new BufferedReader(new InputStreamReader(new FileInputStream(new File(stage,"completed.jsonl")),java.nio.charset.StandardCharsets.UTF_8))){String line;String root=stage.getCanonicalPath()+File.separator;while((line=r.readLine())!=null){try{File f=new File((String)new JSONTokener(line).nextValue());if(f.getCanonicalPath().startsWith(root)&&f.isFile()&&f.length()>0&&!result.contains(f))result.add(f);}catch(Exception ignored){}}}catch(Exception ignored){}return result;}
    private List<String> discover(String url,int depth,Set<String> visited)throws Exception{
        ArrayList<String> result=new ArrayList<>();if(depth>1||visited.size()>6||!visited.add(url))return result;
        Document doc=Jsoup.connect(url).userAgent("Mozilla/5.0").timeout(12000).maxBodySize(2*1024*1024).followRedirects(true).get();
        for(Element e:doc.select("video[src],audio[src],source[src],video[data-src],meta[property^=og:video],meta[name=twitter:player:stream]")){String u=e.hasAttr("content")?e.absUrl("content"):e.hasAttr("src")?e.absUrl("src"):e.absUrl("data-src");if(LinkParser.valid(u)&&!result.contains(u))result.add(u);}
        Matcher m=Pattern.compile("https?://[^\\s<>\"']+?\\.(?:m3u8|mpd|mp4|webm|m4a|mp3)(?:\\?[^\\s<>\"']*)?",Pattern.CASE_INSENSITIVE).matcher(doc.html().replace("\\/","/"));while(m.find()&&result.size()<12)if(LinkParser.valid(m.group())&&!result.contains(m.group()))result.add(m.group());
        for(Element e:doc.select("iframe[src]")){if(result.size()>=12)break;String u=e.absUrl("src");if(LinkParser.valid(u))try{for(String media:discover(u,depth+1,visited))if(!result.contains(media))result.add(media);}catch(Exception ignored){}}
        return result;
    }
    private JSONObject exportFile(File file,String id)throws Exception{
        String ext=file.getName().substring(file.getName().lastIndexOf('.')+1).toLowerCase(Locale.ROOT);String mime=MimeTypeMap.getSingleton().getMimeTypeFromExtension(ext);if(mime==null)mime="mkv".equals(ext)?"video/x-matroska":"application/octet-stream";
        String original=file.getName().replaceAll("[\\x00-\\x1f/\\\\]","_");String name=uniqueName(original);ContentValues v=new ContentValues();v.put(MediaStore.Downloads.DISPLAY_NAME,name);v.put(MediaStore.Downloads.MIME_TYPE,mime);v.put(MediaStore.Downloads.RELATIVE_PATH,"Download/VideoDownloader/");v.put(MediaStore.Downloads.IS_PENDING,1);Uri uri=getContentResolver().insert(MediaStore.Downloads.EXTERNAL_CONTENT_URI,v);if(uri==null)throw new IOException("파일 저장 공간 부족");
        long written=0;try(InputStream in=new FileInputStream(file);OutputStream out=getContentResolver().openOutputStream(uri)){if(out==null)throw new IOException("파일 저장 실패");byte[] b=new byte[256*1024];int n;while((n=in.read(b))!=-1){check(id);out.write(b,0,n);written+=n;}if(written!=file.length())throw new IOException("파일 저장이 중단되었습니다.");}catch(Exception e){getContentResolver().delete(uri,null,null);throw e;}
        ContentValues done=new ContentValues();done.put(MediaStore.Downloads.IS_PENDING,0);getContentResolver().update(uri,done,null,null);return Store.obj("name",name,"source",file.getName(),"uri",uri.toString(),"mime",mime,"bytes",written);
    }
    private String uniqueName(String name){int dot=name.lastIndexOf('.');String base=dot>0?name.substring(0,dot):name,ext=dot>0?name.substring(dot):"";String candidate=name;for(int i=1;i<10000;i++){boolean exists;try(Cursor c=getContentResolver().query(MediaStore.Downloads.EXTERNAL_CONTENT_URI,new String[]{MediaStore.Downloads._ID},MediaStore.Downloads.DISPLAY_NAME+"=? AND "+MediaStore.Downloads.RELATIVE_PATH+"=?",new String[]{candidate,"Download/VideoDownloader/"},null)){exists=c!=null&&c.moveToFirst();}if(!exists)return candidate;candidate=base+String.format(Locale.ROOT,"_%03d",i)+ext;}return base+"_"+System.currentTimeMillis()+ext;}
    private void deleteStage(File file){if(file.isDirectory()){File[] all=file.listFiles();if(all!=null)for(File f:all)deleteStage(f);}file.delete();}
    public static String redact(String detail){String s=detail==null?"":detail;s=s.replaceAll("(?i)(cookie|authorization|token|signature|password)([^\\s]*)([=:])[^\\s]+","$1=[숨김]").replaceAll("https?://[^\\s]+","[미디어 주소]");return s.length()>3000?s.substring(s.length()-3000):s;}
    private String friendly(Exception e){String s=e.toString().toLowerCase(Locale.ROOT);if(s.contains("drm"))return "이 영상은 DRM으로 보호되어 저장할 수 없습니다.";if(s.contains("sign in")||s.contains("login")||s.contains("403")||s.contains("cookies"))return "로그인 또는 사이트 확인이 필요합니다. 브라우저에서 재생 후 미디어를 선택해 주세요.";if(s.contains("space")||s.contains("enospc"))return "휴대폰 저장 공간이 부족합니다.";if(s.contains("timed out")||s.contains("network")||s.contains("resolve"))return "네트워크 연결을 확인한 뒤 재시도해 주세요.";return "다운로드 실패 · 재시도하거나 브라우저에서 미디어를 찾아 주세요.";}
    @Override public void onTimeout(int id,int type){if(!activeId.isEmpty())cancel(activeId);if(worker!=null)worker.interrupt();stopSelf();}
    @Override public void onDestroy(){running=false;if(worker!=null)worker.interrupt();if(!activeId.isEmpty())cancel(activeId);if(wake!=null&&wake.isHeld())wake.release();super.onDestroy();}
    @Override public IBinder onBind(Intent intent){return null;}
}
