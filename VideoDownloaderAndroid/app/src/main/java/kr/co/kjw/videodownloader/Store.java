package kr.co.kjw.videodownloader;

import android.content.Context;
import org.json.*;
import java.util.*;

public final class Store {
    private static JSONObject data;
    private static Context context;
    private static final Set<String> deletingFiles = new HashSet<>();
    private static long lastPruneMs=0;
    public static synchronized void init(Context c) {
        context = c.getApplicationContext();
        if (data != null) return;
        try { data = new JSONObject(context.getSharedPreferences("library", 0).getString("data", "")); }
        catch (Exception e) { data = new JSONObject(); put(data,"lists",new JSONArray().put(obj("id","default","name","내 다운로드"))); put(data,"tasks",new JSONArray()); put(data,"theme","system"); }
        if(!data.has("retentionDays"))put(data,"retentionDays",7);
        pruneCompleted();
        save();
    }
    public static JSONObject obj(Object... values) { JSONObject o = new JSONObject(); for (int i=0;i<values.length;i+=2) put(o,values[i].toString(),values[i+1]); return o; }
    public static void put(JSONObject o,String k,Object v) { try { o.put(k,v); } catch (Exception ignored) {} }
    public static synchronized JSONObject state() { pruneCompleted(); try { return new JSONObject(data.toString()); } catch(Exception e){return new JSONObject();} }
    public static synchronized void save() { if(context!=null) context.getSharedPreferences("library",0).edit().putString("data",data.toString()).commit(); }
    // Clean up completed HISTORY only. Never delete the downloaded MediaStore files.
    private static synchronized void pruneCompleted() {
        if(data==null)return;
        long now=System.currentTimeMillis();
        if(now-lastPruneMs<3600000L)return;
        lastPruneMs=now;
        int days=data.optInt("retentionDays",7);
        if(days<=0)return;
        JSONArray tasks=data.optJSONArray("tasks");
        if(tasks==null)return;
        boolean changed=false;
        for(int i=tasks.length()-1;i>=0;i--){
            JSONObject t=tasks.optJSONObject(i);
            if(t==null||!"completed".equals(t.optString("status")))continue;
            long finished=t.optLong("completedAt",t.optLong("updated",t.optLong("created",now)));
            if(finished>0&&now-finished>=days*86400000L){
                String cookie=t.optString("cookieFile");
                if(!cookie.isEmpty())new java.io.File(cookie).delete();
                tasks.remove(i);changed=true;
            }
        }
        if(changed)save();
    }
    public static synchronized JSONObject task(String id) {
        JSONArray a=data.optJSONArray("tasks"); for(int i=0;i<a.length();i++) { JSONObject o=a.optJSONObject(i); if(id.equals(o.optString("id"))) { try{return new JSONObject(o.toString());}catch(Exception ignored){} } } return null;
    }
    public static synchronized void patch(String id,JSONObject patch) {
        JSONArray a=data.optJSONArray("tasks"); for(int i=0;i<a.length();i++) { JSONObject o=a.optJSONObject(i); if(id.equals(o.optString("id"))) { Iterator<String> keys=patch.keys(); while(keys.hasNext()){String k=keys.next(); put(o,k,patch.opt(k));} put(o,"updated",System.currentTimeMillis()); save(); return; } }
    }
    public static synchronized String next() { JSONArray a=data.optJSONArray("tasks"); for(int i=0;i<a.length();i++){JSONObject task=a.optJSONObject(i);if("queued".equals(task.optString("status"))&&!deletingFiles.contains(task.optString("id")))return task.optString("id");}return null; }
    // Atomic claim prevents multiple concurrent workers from downloading the same task.
    public static synchronized String takeNext(){
        JSONArray a=data.optJSONArray("tasks");
        for(int i=0;i<a.length();i++){
            JSONObject t=a.optJSONObject(i);String id=t.optString("id");
            if("queued".equals(t.optString("status"))&&!deletingFiles.contains(id)){
                put(t,"status","analyzing");put(t,"message","영상 정보 확인 중");put(t,"updated",System.currentTimeMillis());save();return id;
            }
        }
        return null;
    }
    public static synchronized int countQueued(){
        int count=0;JSONArray a=data.optJSONArray("tasks");
        for(int i=0;i<a.length();i++)if("queued".equals(a.optJSONObject(i).optString("status")))count++;
        return count;
    }
    public static synchronized boolean reserveFileDeletion(String id) {
        JSONObject task=task(id);
        if(task==null||deletingFiles.contains(id)||Arrays.asList("queued","analyzing","downloading","exporting","pausing").contains(task.optString("status")))return false;
        deletingFiles.add(id);return true;
    }
    public static synchronized void releaseFileDeletion(String id) { deletingFiles.remove(id); }
    public static synchronized void forgetFile(String id,String uri) {
        JSONObject task=task(id);if(task==null)return;JSONArray files=task.optJSONArray("files");if(files==null)return;
        for(int i=files.length()-1;i>=0;i--)if(uri.equals(files.optJSONObject(i).optString("uri")))files.remove(i);
        JSONObject change=obj("files",files);
        if(files.length()==0&&Arrays.asList("completed","partial").contains(task.optString("status"))){put(change,"status","ready");put(change,"progress",0);put(change,"message","저장 파일을 삭제했습니다. 다시 다운로드할 수 있습니다.");put(change,"error","");}
        patch(id,change);
    }
    public static synchronized void recover() { JSONArray a=data.optJSONArray("tasks"); for(int i=0;i<a.length();i++){JSONObject o=a.optJSONObject(i); if(Arrays.asList("queued","analyzing","downloading","exporting","pausing").contains(o.optString("status"))){put(o,"status","paused");put(o,"message","다운로드가 중단되었습니다. 이어받기를 눌러 주세요.");}}save(); }
    public static synchronized JSONObject command(JSONObject cmd) {
        String action=cmd.optString("action");JSONArray tasks=data.optJSONArray("tasks"),lists=data.optJSONArray("lists");
        if("addList".equals(action)){String name=cmd.optString("name").trim();if(name.isEmpty()||name.length()>60)return obj("error","목록 이름은 1~60자로 입력해 주세요.");String id=UUID.randomUUID().toString();lists.put(obj("id",id,"name",name));save();return obj("id",id);}
        if("renameList".equals(action)){String name=cmd.optString("name").trim();if(name.isEmpty()||name.length()>60)return obj("error","목록 이름은 1~60자로 입력해 주세요.");for(int i=0;i<lists.length();i++)if(cmd.optString("id").equals(lists.optJSONObject(i).optString("id")))put(lists.optJSONObject(i),"name",name);}
        if("deleteList".equals(action)){String id=cmd.optString("id");if("default".equals(id))return obj("error","기본 목록은 삭제할 수 없습니다.");for(int i=lists.length()-1;i>=0;i--)if(id.equals(lists.optJSONObject(i).optString("id")))lists.remove(i);for(int i=0;i<tasks.length();i++)if(id.equals(tasks.optJSONObject(i).optString("list")))put(tasks.optJSONObject(i),"list","default");}
        if("theme".equals(action))put(data,"theme",cmd.optString("value","system"));
        if("retention".equals(action)){
            int days=cmd.optInt("days",7);
            if(days!=0&&days!=3&&days!=7&&days!=30)return obj("error","기록 정리 기간이 올바르지 않습니다.");
            put(data,"retentionDays",days);lastPruneMs=0;pruneCompleted();save();return obj("ok",true);
        }
        if("enqueuePending".equals(action)){
            int queued=0;String list=cmd.optString("list","");
            for(int i=0;i<tasks.length();i++){
                JSONObject t=tasks.optJSONObject(i);
                if(!list.isEmpty()&&!list.equals(t.optString("list")))continue;
                if(Arrays.asList("ready","paused","failed","partial").contains(t.optString("status"))&&!deletingFiles.contains(t.optString("id"))){
                    DownloadService.clearCancelled(t.optString("id"));put(t,"status","queued");put(t,"message","다운로드 대기 중");put(t,"error","");queued++;
                }
            }
            save();return obj("queued",queued);
        }
        if("add".equals(action)){
            List<String> links=LinkParser.parse(cmd.optString("text"));String list=cmd.optString("list","default");boolean found=false;for(int i=0;i<lists.length();i++)if(list.equals(lists.optJSONObject(i).optString("id")))found=true;if(!found)list="default";
            int added=0,duplicate=0;JSONArray ids=new JSONArray(),existingIds=new JSONArray();
            for(String url:links){
                String existing="";
                for(int i=0;i<tasks.length();i++){JSONObject o=tasks.optJSONObject(i);if(url.equals(o.optString("url"))&&list.equals(o.optString("list"))){existing=o.optString("id");break;}}
                if(!existing.isEmpty()){duplicate++;existingIds.put(existing);continue;}
                String id=UUID.randomUUID().toString();
                tasks.put(obj("id",id,"url",url,"title",cmd.optString("title",LinkParser.host(url)),"thumbnail",cmd.optString("thumbnail",""),"list",list,"status","ready","progress",0,"format",cmd.optString("format","mp4"),"quality",cmd.optString("quality","best"),"playlist",cmd.optBoolean("playlist",false),"referer",cmd.optString("referer"),"cookieFile",cmd.optString("cookieFile"),"userAgent",cmd.optString("userAgent","Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 Chrome/131.0.0.0 Mobile Safari/537.36"),"created",System.currentTimeMillis(),"message","다운로드 준비"));
                ids.put(id);added++;
            }
            save();return obj("added",added,"duplicate",duplicate,"ids",ids,"existingIds",existingIds,"error",links.isEmpty()?"http:// 또는 https:// 주소를 입력해 주세요.":"");
        }
        JSONArray ids=cmd.optJSONArray("ids");if(ids==null)ids=new JSONArray().put(cmd.optString("id"));
        for(int n=0;n<ids.length();n++){String id=ids.optString(n);for(int i=tasks.length()-1;i>=0;i--){JSONObject o=tasks.optJSONObject(i);if(!id.equals(o.optString("id")))continue;String status=o.optString("status");boolean active=Arrays.asList("analyzing","downloading","exporting","pausing").contains(status);
            if(deletingFiles.contains(id))continue;
            if("enqueue".equals(action)&&!active&&!"completed".equals(status)){DownloadService.clearCancelled(id);put(o,"status","queued");put(o,"message","순서를 기다리는 중");put(o,"error","");}
            if("pause".equals(action)&&!active&&!"completed".equals(status)){put(o,"status","paused");put(o,"message","일시정지");}
            if("remove".equals(action)&&!active&&!"queued".equals(status)){String cf=o.optString("cookieFile");if(!cf.isEmpty())new java.io.File(cf).delete();tasks.remove(i);}
            if("move".equals(action))put(o,"list",cmd.optString("list","default"));
            if("edit".equals(action)&&!active){put(o,"title",cmd.optString("title",o.optString("title")));put(o,"format",cmd.optString("format",o.optString("format")));put(o,"quality",cmd.optString("quality",o.optString("quality")));put(o,"playlist",cmd.optBoolean("playlist",o.optBoolean("playlist")));String url=cmd.optString("url",o.optString("url"));if(LinkParser.valid(url))put(o,"url",url);}
        }}save();return obj("ok",true);
    }
}
