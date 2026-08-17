"""Serve a Unity WebGL build with the required Brotli and WebAssembly headers."""

from __future__ import annotations

import argparse
import functools
import http.server
import pathlib


class UnityWebGlHandler(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path: str) -> str:
        uncompressed_path = path[:-3] if path.endswith(".br") else path
        if uncompressed_path.endswith(".wasm"):
            return "application/wasm"
        if uncompressed_path.endswith(".js"):
            return "text/javascript"
        if uncompressed_path.endswith(".data"):
            return "application/octet-stream"
        return super().guess_type(uncompressed_path)

    def end_headers(self) -> None:
        if self.path.split("?", 1)[0].endswith(".br"):
            self.send_header("Content-Encoding", "br")
        self.send_header("Cache-Control", "no-store")
        super().end_headers()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--directory", default="Build/WebGL")
    parser.add_argument("--port", type=int, default=8008)
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    directory = pathlib.Path(args.directory).resolve()
    if not (directory / "index.html").is_file():
        raise SystemExit(f"No Unity WebGL build found at {directory}")

    handler = functools.partial(UnityWebGlHandler, directory=str(directory))
    server = http.server.ThreadingHTTPServer(("127.0.0.1", args.port), handler)
    print(f"Serving {directory} at http://127.0.0.1:{args.port}/", flush=True)
    server.serve_forever()


if __name__ == "__main__":
    main()
