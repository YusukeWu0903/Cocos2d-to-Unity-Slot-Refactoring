#!/usr/bin/env python3
"""Minimal HTTP server that serves Unity WebGL .gz builds with correct headers.
Usage: python webgl_server.py <port> <relative-build-dir>
"""
import http.server
import os
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))


def make_handler(root_dir):
    class Handler(http.server.SimpleHTTPRequestHandler):
        def __init__(self, *a, **kw):
            super().__init__(*a, directory=root_dir, **kw)

        def guess_type(self, path):
            base = super().guess_type(path)
            if path.endswith(".gz"):
                inner = path[:-3]
                if inner.endswith(".js"):
                    return "application/javascript"
                if inner.endswith(".wasm"):
                    return "application/wasm"
                if inner.endswith(".data"):
                    return "application/octet-stream"
            return base

        def end_headers(self):
            if self.path.endswith(".gz"):
                self.send_header("Content-Encoding", "gzip")
                self.send_header("Cache-Control", "no-store")
            else:
                self.send_header("Cache-Control", "no-cache")
            super().end_headers()

    return Handler


if __name__ == "__main__":
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8080
    # 第二參數為「網頁根目錄」（絕對或相對皆可，絕對路徑直接用）
    rel = sys.argv[2] if len(sys.argv) > 2 else "../Build/WebGL2"
    root = rel if os.path.isabs(rel) else os.path.normpath(os.path.join(SCRIPT_DIR, rel))
    http.server.ThreadingHTTPServer(("127.0.0.1", port), make_handler(root)).serve_forever()