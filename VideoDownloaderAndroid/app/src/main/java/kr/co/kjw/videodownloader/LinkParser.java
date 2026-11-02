package kr.co.kjw.videodownloader;

import java.net.URI;
import java.util.*;
import java.util.regex.*;

public final class LinkParser {
    private static final Pattern URL = Pattern.compile("https?://[^\\s<>\"']+", Pattern.CASE_INSENSITIVE);
    public static List<String> parse(String text) {
        LinkedHashSet<String> result = new LinkedHashSet<>();
        Matcher m = URL.matcher(text == null ? "" : text);
        while (m.find() && result.size() < 500) {
            String u = m.group().replaceAll("[.,;!\\)\\]\\}]+$", "");
            if (valid(u)) result.add(u);
        }
        return new ArrayList<>(result);
    }
    public static boolean valid(String url) {
        try { URI u = new URI(url); return ("http".equalsIgnoreCase(u.getScheme()) || "https".equalsIgnoreCase(u.getScheme())) && u.getHost() != null && u.getUserInfo() == null; }
        catch (Exception e) { return false; }
    }
    public static String host(String url) { try { return new URI(url).getHost(); } catch (Exception e) { return "영상"; } }
}
