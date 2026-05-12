from datetime import datetime
import os
from pathlib import Path
from flask import Flask, jsonify, send_from_directory, abort

app = Flask(__name__)

CONTENT_DIR = Path(os.getenv("CONTENT_DIR", "content")).resolve()


def format_entry(path: Path, base: Path):
    stat = path.stat()
    return {
        "path": str(path.relative_to(base).as_posix()),
        "is_dir": path.is_dir(),
        "size": stat.st_size,
        "modified_at": datetime.utcfromtimestamp(stat.st_mtime).isoformat() + "Z",
    }


@app.route("/health", methods=["GET"])
def health():
    return jsonify({"status": "ok"})


@app.route("/files", methods=["GET"])
@app.route("/poll", methods=["GET"])
def list_files():
    if not CONTENT_DIR.exists() or not CONTENT_DIR.is_dir():
        return jsonify({"error": "Content folder not found"}), 404

    entries = []
    for path in sorted(CONTENT_DIR.rglob("*")):
        entries.append(format_entry(path, CONTENT_DIR))

    return jsonify({
        "content_dir": str(CONTENT_DIR),
        "count": len(entries),
        "entries": entries,
    })


@app.route("/content/<path:filename>", methods=["GET"])
def serve_content(filename):
    file_path = CONTENT_DIR / filename
    if not file_path.exists() or not file_path.is_file():
        abort(404)
    return send_from_directory(str(CONTENT_DIR), filename)


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8080)
