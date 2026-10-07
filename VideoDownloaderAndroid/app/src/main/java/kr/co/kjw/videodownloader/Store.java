package kr.co.kjw.videodownloader;

import android.content.Context;
import org.json.*;
import java.util.*;

public final class Store {
    private static JSONObject data;
    private static Context context;
    private static final Set<String> deletingFiles = new HashSet<>();
    public static synchronized void init(Context c) {
        context = c.getApplicationContext();
        if (data != null) return;
        try { data = new JSONObject(context.getSharedPreferences("library", 0).getString("data", "")); }
        catch (Exception e) { data = new JSONObject(); put(data,"lists",new JSONArray().put(obj("id","default","name","내 다운로드"))); put(data,"tasks",new JSONArray()); put(data,"theme","system"); }
        save();
    }
    public static JSONObject obj(Object... values) { JSONObject o = new JSONObject(); for (int i=0;i<values.length;i+=2) put(o,values[i].toString(),values[i+1]); return o; }
    public static void put(JSONObject o,String k,Object v) { try { o.put(k,v); } catch (Exception ignored) {} }
    public static synchronized JSONObject state() { try { return new JSONObject(data.toString()); } catch(Exception e){return new JSONObject();} }
    public static synchronized void save() { if(context!=null) context.getSharedPreferences("library",0).edit().putString("data",data.toString()).commit(); }
    public static synchronized JSONObject task(String id) {
        JSONArray a=data.optJSONArray("tasks"); for(int i=0;i<a.length();i++) { JSONObject o=a.optJSONObject(i); if(id.equals(o.optString("id"))) { try{return new JSONObject(o.toString());}catch(Exception ignored){} } } return null;
    }
    public static synchronized void patch(String id,JSONObject patch) {
        JSONArray a=data.optJSONArray("tasks"); for(int i=0;i<a.length();i++) { JSONObject o=a.optJSONObject(i); if(id.equals(o.optString("id"))) { Iterator<String> keys=patch.keys(); while(keys.hasNext()){String k=keys.next(); put(o,k,patch.opt(k));} put(o,"updated",System.currentTimeMillis()); save(); return; } }
    }
    public static synchronized String next() { JSONArray a=data.optJSONArray("tasks"); for(int i=0;i<a.length();i++){JSONObject task=a.optJSONObject(i);if("queued".equals(task.optString("status"))&&!deletingFiles.contains(task.optString("id")))return task.optString("id");}return null; }
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
        if("add".equals(action)){
            List<String> links=LinkParser.parse(cmd.optString("text"));String list=cmd.optString("list","default");boolean found=false;for(int i=0;i<lists.length();i++)if(list.equals(lists.optJSONObject(i).optString("id")))found=true;if(!found)list="default";
            int added=0,duplicate=0;
            for(String url:links){boolean exists=false;for(int i=0;i<tasks.length();i++){JSONObject o=tasks.optJSONObject(i);if(url.equals(o.optString("url"))&&list.equals(o.optString("list"))){exists=true;break;}}if(exists){duplicate++;continue;}String id=UUID.randomUUID().toString();tasks.put(obj("id",id,"url",url,"title",cmd.optString("title",LinkParser.host(url)),"list",list,"status","ready","progress",0,"format",cmd.optString("format","mp4"),"quality",cmd.optString("quality","best"),"playlist",cmd.optBoolean("playlist",false),"referer",cmd.optString("referer"),"cookieFile",cmd.optString("cookieFile"),"userAgent",cmd.optString("userAgent","Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 Chrome/131.0.0.0 Mobile Safari/537.36"),"created",System.currentTimeMillis(),"message","다운로드 준비"));added++;}
            save();return obj("added",added,"duplicate",duplicate,"error",links.isEmpty()?"http:// 또는 https:// 주소를 입력해 주세요.":"");
        }
        JSONArray ids=cmd.optJSONArray("ids");if(ids==null)ids=new JSONArray().put(cmd.optString("id"));
        for(int n=0;n<ids.length();n++){String id=ids.optString(n);for(int i=tasks.length()-1;i>=0;i--){JSONObject o=tasks.optJSONObject(i);if(!id.equals(o.optString("id")))continue;String status=o.optString("status");boolean active=Arrays.asList("analyzing","downloading","exporting","pausing").contains(status);
            if(deletingFiles.contains(id))continue;
            if("enqueue".equals(action)&&!active&&!"completed".equals(status)){put(o,"status","queued");put(o,"message","순서를 기다리는 중");put(o,"error","");}
            if("pause".equals(action)&&!active&&!"completed".equals(status)){put(o,"status","paused");put(o,"message","일시정지");}
            if("remove".equals(action)&&!active){String cf=o.optString("cookieFile");if(!cf.isEmpty())new java.io.File(cf).delete();tasks.remove(i);}
            if("move".equals(action))put(o,"list",cmd.optString("list","default"));
            if("edit".equals(action)&&!active){put(o,"title",cmd.optString("title",o.optString("title")));put(o,"format",cmd.optString("format",o.optString("format")));put(o,"quality",cmd.optString("quality",o.optString("quality")));put(o,"playlist",cmd.optBoolean("playlist",o.optBoolean("playlist")));String url=cmd.optString("url",o.optString("url"));if(LinkParser.valid(url))put(o,"url",url);}
        }}save();return obj("ok",true);
    }
}
