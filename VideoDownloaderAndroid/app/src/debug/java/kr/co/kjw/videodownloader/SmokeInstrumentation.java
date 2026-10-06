package kr.co.kjw.videodownloader;

import android.app.*;
import android.os.Bundle;
import android.content.*;
import org.json.*;
import java.util.*;
import java.util.concurrent.*;

public class SmokeInstrumentation extends Instrumentation {
 @Override public void onCreate(Bundle b){super.onCreate(b);start();}
 private void require(boolean condition,String message){if(!condition)throw new AssertionError(message);}
 private String js(MainActivity a,String script)throws Exception{CountDownLatch latch=new CountDownLatch(1);String[] result={""};runOnMainSync(()->a.web.evaluateJavascript(script,v->{result[0]=v;latch.countDown();}));require(latch.await(20,TimeUnit.SECONDS),"WebView callback timeout");return result[0];}
 @Override public void onStart(){Bundle result=new Bundle();try{
  Context c=getTargetContext();Store.init(c);c.getSharedPreferences("engine",0).edit().putLong("updated",System.currentTimeMillis()).commit();
  MainActivity a=(MainActivity)startActivitySync(new Intent(c,MainActivity.class).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));waitForIdleSync();
  for(int n=0;n<40;n++){if("true".equals(js(a,"!!document.querySelector('#addTop')")))break;Thread.sleep(500);}
  require("true".equals(js(a,"!!document.querySelector('#addTop')")),"App UI loaded");
  String list=Store.command(Store.obj("action","addList","name","네이티브 QA 목록")).getString("id");
  String base="http://10.0.2.2:8765/";JSONObject add=Store.command(Store.obj("action","add","text",base+"sample.mp4\n"+base+"hls/index.m3u8\n"+base+"dash/index.mpd","list",list,"format","mp4","quality","best"));require(add.getInt("added")==3,"Three media tasks added");
  require(Store.command(Store.obj("action","add","text",base+"sample.mp4","list",list)).getInt("duplicate")==1,"Deduplication");
  ArrayList<String> ids=new ArrayList<>();JSONArray tasks=Store.state().getJSONArray("tasks");for(int i=0;i<tasks.length();i++)if(list.equals(tasks.getJSONObject(i).optString("list")))ids.add(tasks.getJSONObject(i).getString("id"));
  Store.command(Store.obj("action","enqueue","ids",new JSONArray(ids)));runOnMainSync(()->c.startForegroundService(new Intent(c,DownloadService.class)));
  long deadline=System.currentTimeMillis()+360000;while(System.currentTimeMillis()<deadline){boolean done=true;for(String id:ids){String s=Store.task(id).optString("status");if(!Arrays.asList("completed","failed").contains(s))done=false;}if(done)break;Thread.sleep(1000);}
  for(String id:ids){JSONObject t=Store.task(id);require("completed".equals(t.optString("status")),"Download "+t.optString("url")+": "+t.toString());require(t.getJSONArray("files").length()>0,"MediaStore file present");require(t.optDouble("duration",0)<600,"Short media downloaded");try(java.io.InputStream in=c.getContentResolver().openInputStream(android.net.Uri.parse(t.getJSONArray("files").getJSONObject(0).getString("uri")))){require(in.read()!=-1,"Saved file readable");}}
  // Download same fixture in a second list; MediaStore must reserve a fresh _001 filename.
  String other=Store.command(Store.obj("action","addList","name","동명 파일 QA")).getString("id");Store.command(Store.obj("action","add","text",base+"sample.mp4","list",other));String duplicate="";tasks=Store.state().getJSONArray("tasks");for(int i=0;i<tasks.length();i++)if(other.equals(tasks.getJSONObject(i).optString("list")))duplicate=tasks.getJSONObject(i).getString("id");Store.command(Store.obj("action","enqueue","id",duplicate));runOnMainSync(()->c.startForegroundService(new Intent(c,DownloadService.class)));deadline=System.currentTimeMillis()+120000;while(System.currentTimeMillis()<deadline&&!Arrays.asList("completed","failed").contains(Store.task(duplicate).optString("status")))Thread.sleep(500);require("completed".equals(Store.task(duplicate).optString("status")),"Duplicate file download");require(Store.task(duplicate).getJSONArray("files").getJSONObject(0).getString("name").contains("_001"),"Filename collision suffix");
  require("true".equals(js(a,"refresh(true);state.tasks.filter(t=>t.status==='completed').length>=4")),"Native results rendered in WebView");
  Store.patch(ids.get(0),Store.obj("status","downloading"));Store.recover();require("paused".equals(Store.task(ids.get(0)).optString("status")),"Interrupted task recovery");
  result.putString("stream","PASS: UI bridge, lists, duplicate URLs, MP4, HLS, DASH, short duration, MediaStore bytes, filename _001, process recovery.");finish(Activity.RESULT_OK,result);
 }catch(Throwable e){result.putString("stream","FAIL: "+android.util.Log.getStackTraceString(e));finish(Activity.RESULT_CANCELED,result);}}
}
