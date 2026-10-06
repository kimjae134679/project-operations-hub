package kr.co.kjw.videodownloader;

import android.app.*;
import android.os.*;
import android.content.*;
import android.database.Cursor;
import android.net.Uri;
import android.view.*;
import android.webkit.*;
import org.json.*;
import java.io.*;
import com.yausername.youtubedl_android.YoutubeDL;

public class MainActivity extends Activity {
    public WebView web;
    private String shared="";
    @Override public void onCreate(Bundle state){
        super.onCreate(state);Store.init(this);if(!DownloadService.running)Store.recover();receive(getIntent());
        web=new WebView(this);web.setBackgroundColor(0xfff7f8fc);web.getSettings().setJavaScriptEnabled(true);web.getSettings().setDomStorageEnabled(true);web.getSettings().setAllowFileAccess(false);web.getSettings().setAllowContentAccess(false);web.getSettings().setMixedContentMode(WebSettings.MIXED_CONTENT_NEVER_ALLOW);
        if(BuildConfig.DEBUG)WebView.setWebContentsDebuggingEnabled(true);
        web.addJavascriptInterface(new Bridge(),"Android");
        web.setWebViewClient(new WebViewClient(){@Override public boolean shouldOverrideUrlLoading(WebView v,WebResourceRequest r){if(r.getUrl().toString().equals("file:///android_asset/index.html"))return false;return true;}});
        android.widget.FrameLayout root=new android.widget.FrameLayout(this);
        root.setBackgroundColor(0xfff7f8fc);
        root.addView(web,new android.widget.FrameLayout.LayoutParams(-1,-1));
        root.setOnApplyWindowInsetsListener((v,insets)->{if(Build.VERSION.SDK_INT>=30){android.graphics.Insets bars=insets.getInsets(WindowInsets.Type.systemBars()|WindowInsets.Type.displayCutout());android.graphics.Insets ime=insets.getInsets(WindowInsets.Type.ime());v.setPadding(bars.left,bars.top,bars.right,Math.max(bars.bottom,ime.bottom));return WindowInsets.CONSUMED;}v.setPadding(insets.getSystemWindowInsetLeft(),insets.getSystemWindowInsetTop(),insets.getSystemWindowInsetRight(),insets.getSystemWindowInsetBottom());return insets.consumeSystemWindowInsets();});
        setContentView(root);root.requestApplyInsets();web.loadUrl("file:///android_asset/index.html");
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
            if("browser".equals(a)){String url=c.optString("url");if(!LinkParser.valid(url))return Store.obj("error","올바른 주소를 입력해 주세요.").toString();runOnUiThread(()->startActivity(new Intent(MainActivity.this,BrowserActivity.class).putExtra("url",url).putExtra("taskId",c.optString("id")).putExtra("list",c.optString("list","default"))));return "{}";}
            if("open".equals(a)||"share".equals(a)){JSONObject t=Store.task(c.optString("id"));int index=c.optInt("index",0);JSONArray files=t==null?null:t.optJSONArray("files");if(files==null||index<0||index>=files.length())return Store.obj("error","저장된 파일을 찾을 수 없습니다.").toString();JSONObject file=files.optJSONObject(index);runOnUiThread(()->{try{Uri uri=Uri.parse(file.optString("uri"));try(InputStream stream=getContentResolver().openInputStream(uri)){if(stream==null)throw new IOException();}Intent i;if("share".equals(a)){i=new Intent(Intent.ACTION_SEND).setType(file.optString("mime","video/*")).putExtra(Intent.EXTRA_STREAM,uri);i.setClipData(ClipData.newUri(getContentResolver(),"영상",uri));i.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);startActivity(Intent.createChooser(i,"파일 공유"));}else{i=new Intent(Intent.ACTION_VIEW).setDataAndType(uri,file.optString("mime","video/*")).addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);startActivity(i);}}catch(Exception e){message("파일이 삭제되었거나 열 수 있는 앱이 없습니다.");}});return "{}";}
            if("import".equals(a)){runOnUiThread(()->startActivityForResult(new Intent(Intent.ACTION_OPEN_DOCUMENT).setType("*/*").putExtra(Intent.EXTRA_MIME_TYPES,new String[]{"text/plain","application/json","text/json"}).addCategory(Intent.CATEGORY_OPENABLE),100));return "{}";}
            if("export".equals(a)){runOnUiThread(()->startActivityForResult(new Intent(Intent.ACTION_CREATE_DOCUMENT).setType("application/json").putExtra(Intent.EXTRA_TITLE,"다운로드목록.json"),101));return "{}";}
            if("update".equals(a)){new Thread(()->{if(DownloadService.running){message("다운로드가 끝난 뒤 업데이트해 주세요.");return;}try{DownloadService.initialize(MainActivity.this);YoutubeDL.getInstance().updateYoutubeDL(MainActivity.this,YoutubeDL.UpdateChannel._STABLE);message("다운로드 엔진이 최신 상태입니다.");}catch(Exception e){message("엔진 업데이트 실패. 네트워크를 확인해 주세요.");}},"engine-update").start();return "{}";}
            if("theme".equals(a)){runOnUiThread(()->{boolean dark="dark".equals(c.optString("value"));if(Build.VERSION.SDK_INT>=30){WindowInsetsController ctl=getWindow().getDecorView().getWindowInsetsController();if(ctl!=null)ctl.setSystemBarsAppearance(dark?0:WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS|WindowInsetsController.APPEARANCE_LIGHT_NAVIGATION_BARS,WindowInsetsController.APPEARANCE_LIGHT_STATUS_BARS|WindowInsetsController.APPEARANCE_LIGHT_NAVIGATION_BARS);}else getWindow().getDecorView().setSystemUiVisibility(dark?0:View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR|View.SYSTEM_UI_FLAG_LIGHT_NAVIGATION_BAR);});}
            return Store.command(c).toString();
        }catch(Exception e){return Store.obj("error","요청을 처리하지 못했습니다.").toString();}}
    }
    @Override protected void onActivityResult(int request,int result,Intent intent){super.onActivityResult(request,result,intent);if(result!=RESULT_OK||intent==null||intent.getData()==null)return;Uri uri=intent.getData();new Thread(()->{try{
        if(request==101){JSONObject backup=Store.state();JSONArray tasks=backup.optJSONArray("tasks");for(int i=0;i<tasks.length();i++){JSONObject o=tasks.optJSONObject(i);o.remove("cookieFile");o.remove("referer");o.remove("files");o.remove("error");Store.put(o,"status","ready");Store.put(o,"progress",0);}try(OutputStream out=getContentResolver().openOutputStream(uri)){out.write(backup.toString(2).getBytes(java.nio.charset.StandardCharsets.UTF_8));}message("목록을 내보냈습니다.");}
        else {String text;try(InputStream in=getContentResolver().openInputStream(uri)){ByteArrayOutputStream buf=new ByteArrayOutputStream();byte[] b=new byte[8192];int n;while((n=in.read(b))!=-1){if(buf.size()+n>2*1024*1024)throw new IOException();buf.write(b,0,n);}text=buf.toString("UTF-8");}try{JSONObject backup=new JSONObject(text);JSONArray lists=backup.getJSONArray("lists"),tasks=backup.getJSONArray("tasks");java.util.HashMap<String,String> map=new java.util.HashMap<>();for(int i=0;i<lists.length();i++){JSONObject l=lists.getJSONObject(i);String id=Store.command(Store.obj("action","addList","name",l.optString("name","가져온 목록"))).optString("id","default");map.put(l.optString("id"),id);}int added=0;for(int i=0;i<Math.min(tasks.length(),500);i++){JSONObject o=tasks.getJSONObject(i);JSONObject addedResult=Store.command(Store.obj("action","add","text",o.optString("url"),"title",o.optString("title"),"list",map.getOrDefault(o.optString("list"),"default"),"format",o.optString("format","mp4"),"quality",o.optString("quality","best"),"playlist",o.optBoolean("playlist")));added+=addedResult.optInt("added");}message(added+"개 주소를 가져왔습니다.");}catch(JSONException e){JSONObject r=Store.command(Store.obj("action","add","text",text));message(r.optString("error").isEmpty()?r.optInt("added")+"개 주소를 가져왔습니다.":r.optString("error"));}}
    }catch(Exception e){message("파일을 읽거나 저장할 수 없습니다. 2MB 이하 TXT/JSON을 사용해 주세요.");}},"list-file").start();}
    @Override public void onBackPressed(){web.evaluateJavascript("window.handleBack&&handleBack()",v->{if(!"true".equals(v))super.onBackPressed();});}
    @Override protected void onDestroy(){if(web!=null){web.removeJavascriptInterface("Android");web.destroy();}super.onDestroy();}
}
