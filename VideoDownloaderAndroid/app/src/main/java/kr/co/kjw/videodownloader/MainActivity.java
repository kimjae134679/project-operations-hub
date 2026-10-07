package kr.co.kjw.videodownloader;

import android.app.*;
import android.os.*;
import android.content.*;
import android.database.Cursor;
import android.net.Uri;
import android.provider.MediaStore;
import android.view.*;
import android.webkit.*;
import org.json.*;
import java.io.*;
import com.yausername.youtubedl_android.YoutubeDL;

public class MainActivity extends Activity {
    public WebView web;
    private String shared="";
    private android.widget.FrameLayout root;
    private volatile FileDeletion deletion;
    private static final int DELETE_CONSENT=102;
    private static final class DeleteTarget {
        final String id;final JSONObject file;
        boolean consentRequested;
        DeleteTarget(String id,JSONObject file){this.id=id;this.file=file;}
    }
    private static final class FileDeletion {
        final java.util.ArrayList<DeleteTarget> targets=new java.util.ArrayList<>();
        final java.util.LinkedHashSet<String> reserved=new java.util.LinkedHashSet<>();
        int position,deleted,failed;
        volatile boolean waitingConsent,cancelled;
    }
    @Override public void onCreate(Bundle state){
        super.onCreate(state);Store.init(this);if(!DownloadService.running)Store.recover();receive(getIntent());
        web=new WebView(this);web.setBackgroundColor(0xfff7f8fc);web.getSettings().setJavaScriptEnabled(true);web.getSettings().setDomStorageEnabled(true);web.getSettings().setAllowFileAccess(false);web.getSettings().setAllowContentAccess(false);web.getSettings().setMixedContentMode(WebSettings.MIXED_CONTENT_NEVER_ALLOW);
        if(BuildConfig.DEBUG)WebView.setWebContentsDebuggingEnabled(true);
        web.addJavascriptInterface(new Bridge(),"Android");
        web.setWebViewClient(new WebViewClient(){@Override public boolean shouldOverrideUrlLoading(WebView v,WebResourceRequest r){if(r.getUrl().toString().equals("file:///android_asset/index.html"))return false;return true;}});
        root=new android.widget.FrameLayout(this);
        root.setBackgroundColor(0xfff7f8fc);
        root.addView(web,new android.widget.FrameLayout.LayoutParams(-1,-1));
        root.setOnApplyWindowInsetsListener((v,insets)->{if(Build.VERSION.SDK_INT>=30){android.graphics.Insets bars=insets.getInsets(WindowInsets.Type.systemBars()|WindowInsets.Type.displayCutout());android.graphics.Insets ime=insets.getInsets(WindowInsets.Type.ime());v.setPadding(bars.left,bars.top,bars.right,Math.max(bars.bottom,ime.bottom));return WindowInsets.CONSUMED;}v.setPadding(insets.getSystemWindowInsetLeft(),insets.getSystemWindowInsetTop(),insets.getSystemWindowInsetRight(),insets.getSystemWindowInsetBottom());return insets.consumeSystemWindowInsets();});
        setContentView(root);applyNativeTheme(Store.state().optString("theme","system"));root.post(()->applyNativeTheme(Store.state().optString("theme","system")));root.requestApplyInsets();web.loadUrl("file:///android_asset/index.html");
    }
    @Override protected void onNewIntent(Intent intent){super.onNewIntent(intent);setIntent(intent);receive(intent);if(web!=null)web.evaluateJavascript("window.checkShared&&checkShared()",null);}
    private void receive(Intent i){if(i!=null&&(Intent.ACTION_SEND.equals(i.getAction())||Intent.ACTION_SEND_MULTIPLE.equals(i.getAction())))shared=i.getStringExtra(Intent.EXTRA_TEXT)==null?"":i.getStringExtra(Intent.EXTRA_TEXT);}
    private void startDownloads(){if(Build.VERSION.SDK_INT>=33&&checkSelfPermission(android.Manifest.permission.POST_NOTIFICATIONS)!=android.content.pm.PackageManager.PERMISSION_GRANTED)requestPermissions(new String[]{android.Manifest.permission.POST_NOTIFICATIONS},7);startForegroundService(new Intent(this,DownloadService.class));}
    private void message(String s){runOnUiThread(()->{if(web!=null&&!isFinishing()&&!isDestroyed())web.evaluateJavascript("window.toast&&toast("+JSONObject.quote(s)+")",null);});}
    public class Bridge {
        @JavascriptInterface public String state(){return Store.state().toString();}
        @JavascriptInterface public String shared(){String v=shared;shared="";return v;}
        @JavascriptInterface public String command(String json){try{JSONObject c=new JSONObject(json);String a=c.optString("action");
            if("start".equals(a)){runOnUiThread(()->startDownloads());return "{}";}
            if("cancel".equals(a)){DownloadService.cancel(c.optString("id"));return "{}";}
            if("deleteFiles".equals(a)){if(deletion!=null)return Store.obj("error","파일 삭제가 진행 중입니다.").toString();runOnUiThread(()->beginFileDeletion(c));return Store.obj("pending",true).toString();}
            if("external".equals(a)){String url=browserUrl(c.optString("url"));if(!LinkParser.valid(url))return Store.obj("error","올바른 주소를 입력해 주세요.").toString();runOnUiThread(()->{try{startActivity(new Intent(Intent.ACTION_VIEW,Uri.parse(url)).addCategory(Intent.CATEGORY_BROWSABLE));}catch(Exception e){message("주소를 열 수 있는 브라우저가 없습니다.");}});return "{}";}
            if("browser".equals(a)){String url=browserUrl(c.optString("url"));if(!LinkParser.valid(url))return Store.obj("error","올바른 주소를 입력해 주세요.").toString();runOnUiThread(()->startActivity(new Intent(MainActivity.this,BrowserActivity.class).putExtra("url",url).putExtra("taskId",c.optString("id")).putExtra("list",c.optString("list","default"))));return "{}";}
            if("open".equals(a)||"share".equals(a)){JSONObject t=Store.task(c.optString("id"));int index=c.optInt("index",0);JSONArray files=t==null?null:t.optJSONArray("files");if(files==null||index<0||index>=files.length())return Store.obj("error","저장된 파일을 찾을 수 없습니다.").toString();JSONObject file=files.optJSONObject(index);runOnUiThread(()->{try{Uri uri=Uri.parse(file.optString("uri"));try(InputStream stream=getContentResolver().openInputStream(uri)){if(stream==null)throw new IOException();}Intent i;if("share".equals(a)){i=new Intent(Intent.ACTION_SEND).setType(file.optString("mime","video/*")).putExtra(Intent.EXTRA_STREAM,uri);i.setClipData(ClipData.newUri(getContentResolver(),"영상",uri));i.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);startActivity(Intent.createChooser(i,"파일 공유"));}else{i=new Intent(Intent.ACTION_VIEW).setDataAndType(uri,file.optString("mime","video/*")).addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);startActivity(i);}}catch(Exception e){message("파일이 삭제되었거나 열 수 있는 앱이 없습니다.");}});return "{}";}
            if("import".equals(a)){runOnUiThread(()->startActivityForResult(new Intent(Intent.ACTION_OPEN_DOCUMENT).setType("*/*").putExtra(Intent.EXTRA_MIME_TYPES,new String[]{"text/plain","application/json","text/json"}).addCategory(Intent.CATEGORY_OPENABLE),100));return "{}";}
            if("export".equals(a)){runOnUiThread(()->startActivityForResult(new Intent(Intent.ACTION_CREATE_DOCUMENT).setType("application/json").putExtra(Intent.EXTRA_TITLE,"다운로드목록.json"),101));return "{}";}
            if("update".equals(a)){new Thread(()->{if(DownloadService.running){message("다운로드가 끝난 뒤 업데이트해 주세요.");return;}try{DownloadService.initialize(MainActivity.this);YoutubeDL.getInstance().updateYoutubeDL(MainActivity.this,YoutubeDL.UpdateChannel._STABLE);message("다운로드 엔진이 최신 상태입니다.");}catch(Exception e){message("엔진 업데이트 실패. 네트워크를 확인해 주세요.");}},"engine-update").start();return "{}";}
            if("theme".equals(a))runOnUiThread(()->applyNativeTheme(c.optString("value","system")));
            return Store.command(c).toString();
        }catch(Exception e){return Store.obj("error","요청을 처리하지 못했습니다.").toString();}}
    }
    @Override protected void onActivityResult(int request,int result,Intent intent){super.onActivityResult(request,result,intent);if(request==DELETE_CONSENT){if(deletion!=null){if(result==RESULT_OK)continueFileDeletion(false);else continueFileDeletion(true);}return;}if(result!=RESULT_OK||intent==null||intent.getData()==null)return;Uri uri=intent.getData();new Thread(()->{try{
        if(request==101){JSONObject backup=Store.state();JSONArray tasks=backup.optJSONArray("tasks");for(int i=0;i<tasks.length();i++){JSONObject o=tasks.optJSONObject(i);o.remove("cookieFile");o.remove("referer");o.remove("files");o.remove("error");Store.put(o,"status","ready");Store.put(o,"progress",0);}try(OutputStream out=getContentResolver().openOutputStream(uri)){out.write(backup.toString(2).getBytes(java.nio.charset.StandardCharsets.UTF_8));}message("목록을 내보냈습니다.");}
        else {String text;try(InputStream in=getContentResolver().openInputStream(uri)){ByteArrayOutputStream buf=new ByteArrayOutputStream();byte[] b=new byte[8192];int n;while((n=in.read(b))!=-1){if(buf.size()+n>2*1024*1024)throw new IOException();buf.write(b,0,n);}text=buf.toString("UTF-8");}try{JSONObject backup=new JSONObject(text);JSONArray lists=backup.getJSONArray("lists"),tasks=backup.getJSONArray("tasks");java.util.HashMap<String,String> map=new java.util.HashMap<>();for(int i=0;i<lists.length();i++){JSONObject l=lists.getJSONObject(i);String id=Store.command(Store.obj("action","addList","name",l.optString("name","가져온 목록"))).optString("id","default");map.put(l.optString("id"),id);}int added=0;for(int i=0;i<Math.min(tasks.length(),500);i++){JSONObject o=tasks.getJSONObject(i);JSONObject addedResult=Store.command(Store.obj("action","add","text",o.optString("url"),"title",o.optString("title"),"list",map.getOrDefault(o.optString("list"),"default"),"format",o.optString("format","mp4"),"quality",o.optString("quality","best"),"playlist",o.optBoolean("playlist")));added+=addedResult.optInt("added");}message(added+"개 주소를 가져왔습니다.");}catch(JSONException e){JSONObject r=Store.command(Store.obj("action","add","text",text));message(r.optString("error").isEmpty()?r.optInt("added")+"개 주소를 가져왔습니다.":r.optString("error"));}}
    }catch(Exception e){message("파일을 읽거나 저장할 수 없습니다. 2MB 이하 TXT/JSON을 사용해 주세요.");}},"list-file").start();}
    static String browserUrl(String input){String url=input==null?"":input.trim();if(!url.contains("://")&&!url.contains(" ")&&!url.contains("\n"))url="https://"+url;return url;}
    private void applyNativeTheme(String theme){
        boolean dark="dark".equals(theme)||("system".equals(theme)&&(getResources().getConfiguration().uiMode&android.content.res.Configuration.UI_MODE_NIGHT_MASK)==android.content.res.Configuration.UI_MODE_NIGHT_YES);
        int color=dark?0xff10191c:0xfff6f7f9;if(root!=null)root.setBackgroundColor(color);if(web!=null)web.setBackgroundColor(color);
        getWindow().setStatusBarColor(color);getWindow().setNavigationBarColor(color);
        if(Build.VERSION.SDK_INT>=30){WindowInsetsController controller=getWindow().getDecorView().getWindowInsetsController();if(controller!=null)controller.setSystemBarsAppearance(dark?0:WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS|WindowInsetsController.APPEARANCE_LIGHT_NAVIGATION_BARS,WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS|WindowInsetsController.APPEARANCE_LIGHT_NAVIGATION_BARS);}else getWindow().getDecorView().setSystemUiVisibility(dark?0:View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR|View.SYSTEM_UI_FLAG_LIGHT_NAVIGATION_BAR);
    }
    private void beginFileDeletion(JSONObject cmd){
        if(deletion!=null){message("파일 삭제가 진행 중입니다.");return;}
        FileDeletion job=new FileDeletion();JSONArray ids=cmd.optJSONArray("ids");if(ids==null)ids=new JSONArray().put(cmd.optString("id"));
        for(int i=0;i<ids.length();i++){String id=ids.optString(i);if(!Store.reserveFileDeletion(id)){job.failed++;continue;}job.reserved.add(id);JSONObject task=Store.task(id);JSONArray files=task.optJSONArray("files");if(files==null)continue;for(int n=0;n<files.length();n++){if(cmd.has("index")&&n!=cmd.optInt("index",-1))continue;JSONObject file=files.optJSONObject(n);if(file!=null)job.targets.add(new DeleteTarget(id,file));}}
        deletion=job;continueFileDeletion(false);
    }
    private void continueFileDeletion(boolean denied){
        FileDeletion job=deletion;if(job==null)return;
        job.waitingConsent=false;
        if(denied){job.failed++;job.position++;}
        new Thread(()->{
            while(!job.cancelled&&job.position<job.targets.size()){
                DeleteTarget target=job.targets.get(job.position);Uri uri=Uri.parse(target.file.optString("uri"));boolean verified=false;
                try{
                    // Only an explicit saved download from this app's folder may be deleted.
                    if(!"content".equals(uri.getScheme())||!"media".equals(uri.getAuthority())||!uri.getPath().matches("/(external|external_primary)/downloads/[0-9]+"))throw new SecurityException("Unsupported saved file");
                    boolean exists=false;
                    String[] columns=Build.VERSION.SDK_INT>=30?new String[]{MediaStore.MediaColumns.DISPLAY_NAME,MediaStore.MediaColumns.RELATIVE_PATH,MediaStore.MediaColumns.OWNER_PACKAGE_NAME}:new String[]{MediaStore.MediaColumns.DISPLAY_NAME,MediaStore.MediaColumns.RELATIVE_PATH};
                    try(Cursor cursor=getContentResolver().query(uri,columns,null,null,null)){
                        if(cursor==null)throw new IOException("File lookup unavailable");
                        if(cursor.moveToFirst()){exists=true;if(!"Download/VideoDownloader/".equals(cursor.getString(1))||!target.file.optString("name").equals(cursor.getString(0)))throw new SecurityException("Saved file moved or replaced");if(Build.VERSION.SDK_INT>=30&&!cursor.isNull(2)&&!getPackageName().equals(cursor.getString(2)))throw new SecurityException("File belongs to another app");}
                    }
                    verified=exists;
                    if(exists&&getContentResolver().delete(uri,null,null)==0)throw new IOException("File was not deleted");
                    Store.forgetFile(target.id,uri.toString());job.deleted++;job.position++;
                }catch(RecoverableSecurityException e){
                    if(target.consentRequested){job.failed++;job.position++;continue;}target.consentRequested=true;
                    requestDeleteConsent(job,e.getUserAction().getActionIntent());return;
                }catch(SecurityException e){
                    if(Build.VERSION.SDK_INT>=30&&verified&&!target.consentRequested){
                        target.consentRequested=true;
                        try{PendingIntent consent=MediaStore.createDeleteRequest(getContentResolver(),java.util.Collections.singletonList(uri));requestDeleteConsent(job,consent);return;}catch(Exception unsupported){/* Downloads providers may require the system Files app. */}
                    }
                    job.failed++;job.position++;
                }catch(Exception e){job.failed++;job.position++;}
            }
            for(String id:job.reserved)Store.releaseFileDeletion(id);
            runOnUiThread(()->{if(deletion!=job)return;deletion=null;JSONObject result=Store.obj("deleted",job.deleted,"failed",job.failed);if(web!=null&&!isDestroyed())web.evaluateJavascript("window.onFilesDeleted&&onFilesDeleted("+result+")",null);message(job.failed>0?job.deleted+"개 파일 삭제 · "+job.failed+"개는 삭제하지 못했습니다.":job.deleted+"개 저장 파일을 삭제했습니다.");});
        },"delete-saved-media").start();
    }
    private void requestDeleteConsent(FileDeletion job,PendingIntent consent){job.waitingConsent=true;runOnUiThread(()->{if(job.cancelled)return;if(isDestroyed()||isFinishing()){continueFileDeletion(true);return;}try{startIntentSenderForResult(consent.getIntentSender(),DELETE_CONSENT,null,0,0,0);}catch(Exception failure){continueFileDeletion(true);}});}
    @Override public void onBackPressed(){web.evaluateJavascript("window.handleBack&&handleBack()",v->{if(!"true".equals(v))super.onBackPressed();});}
    @Override protected void onDestroy(){FileDeletion job=deletion;if(job!=null&&job.waitingConsent){job.cancelled=true;for(String id:job.reserved)Store.releaseFileDeletion(id);deletion=null;}if(web!=null){web.removeJavascriptInterface("Android");web.destroy();}super.onDestroy();}
}
