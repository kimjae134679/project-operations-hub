package kr.co.kjw.videodownloader;
import org.junit.Test;
import static org.junit.Assert.*;
public class LinkParserTest {
 @Test public void sharedTextAndDuplicates(){assertEquals(2,LinkParser.parse("영상 https://example.org/a.mp4\nhttps://example.org/a.mp4\n다음 (https://example.org/b.m3u8?x=1). ").size());}
 @Test public void rejectsUnsafeSchemesAndCredentials(){assertFalse(LinkParser.valid("javascript:alert(1)"));assertFalse(LinkParser.valid("file:///sdcard/x"));assertFalse(LinkParser.valid("https://user:pass@example.org/x"));assertTrue(LinkParser.valid("https://example.org/a.mpd?token=a%2Bb"));}
 @Test public void preservesSignedQueryAndBoundedBatch(){StringBuilder s=new StringBuilder();for(int i=0;i<600;i++)s.append("https://example.org/"+i+"?token=a%2Bb&x=1\n");assertEquals(500,LinkParser.parse(s.toString()).size());assertEquals("https://example.org/0?token=a%2Bb&x=1",LinkParser.parse(s.toString()).get(0));}
}
