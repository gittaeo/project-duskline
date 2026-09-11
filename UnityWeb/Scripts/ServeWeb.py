"""Local-only preview. Run from anywhere; open http://127.0.0.1:8765."""
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('--port', type=int, default=8765)
parser.add_argument('--smoke', action='store_true', help='Serve the separate Web integration-test build')
args = parser.parse_args()
root = Path(__file__).resolve().parents[1] / 'Builds' / ('Smoke' if args.smoke else 'Web')
if not (root / 'index.html').is_file():
    raise SystemExit('Builds/Web/index.html is missing. Run BuildWeb.ps1 first.')
handler = partial(SimpleHTTPRequestHandler, directory=str(root))
print(f'Duskline Web: http://127.0.0.1:{args.port}', flush=True)
ThreadingHTTPServer(('127.0.0.1', args.port), handler).serve_forever()
