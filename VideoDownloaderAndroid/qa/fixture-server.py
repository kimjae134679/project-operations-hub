"""Deterministic, range-capable throttled fixture for native cancellation/resume QA."""
import http.server
import os
import sys
import time
from pathlib import Path

class FixtureHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        if self.path.split('?')[0] != '/slow.mp4':
            return super().do_GET()
        source = Path('slow.mp4')
        total = source.stat().st_size
        start = 0
        if self.headers.get('Range', '').startswith('bytes='):
            start = int(self.headers['Range'].split('=')[1].split('-')[0])
        if start >= total:
            self.send_response(416)
            self.end_headers()
            return
        self.send_response(206 if start else 200)
        self.send_header('Content-Type', 'video/mp4')
        self.send_header('Accept-Ranges', 'bytes')
        self.send_header('Content-Length', str(total-start))
        if start:
            self.send_header('Content-Range', f'bytes {start}-{total-1}/{total}')
        self.end_headers()
        try:
            with source.open('rb') as media:
                media.seek(start)
                while chunk := media.read(32768):
                    self.wfile.write(chunk)
                    self.wfile.flush()
                    time.sleep(.12)
        except (BrokenPipeError, ConnectionResetError):
            pass

os.chdir(sys.argv[1])
http.server.ThreadingHTTPServer(('0.0.0.0', 8765), FixtureHandler).serve_forever()
