#!/usr/bin/env python3
"""Conservative static audit for JT Digital World website/app preview files."""
from __future__ import annotations
import html.parser
import os
import sys
from pathlib import Path
from urllib.parse import urlparse, unquote

ROOT = Path.cwd().resolve()
IGNORE_DIRS = {".git", "node_modules", ".wrangler", "dist", "build"}
HTML_FILES, JS_FILES, CSS_FILES = [], [], []
ERRORS, WARNINGS = [], []

class RefParser(html.parser.HTMLParser):
    def __init__(self):
        super().__init__(convert_charrefs=True)
        self.refs = []
    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        for key in ("href", "src", "poster", "action"):
            value = attrs.get(key)
            if value:
                self.refs.append((key, value.strip()))

def iter_files():
    for current, dirs, files in os.walk(ROOT):
        dirs[:] = [d for d in dirs if d not in IGNORE_DIRS and not d.startswith(".")]
        for name in files:
            rel = (Path(current) / name).relative_to(ROOT)
            if rel.suffix.lower() == ".html": HTML_FILES.append(rel)
            elif rel.suffix.lower() == ".js": JS_FILES.append(rel)
            elif rel.suffix.lower() == ".css": CSS_FILES.append(rel)

def external(value):
    p = urlparse(value)
    return bool(p.scheme or value.startswith("//") or value.startswith("#") or value.startswith(("data:", "mailto:", "tel:", "javascript:")))

def check_html(rel):
    try:
        parser = RefParser()
        parser.feed((ROOT / rel).read_text(encoding="utf-8"))
        parser.close()
    except Exception as exc:
        ERRORS.append(f"{rel}: cannot parse/read HTML: {exc}")
        return
    for attr, value in parser.refs:
        if external(value): continue
        local = unquote(value.split("#", 1)[0].split("?", 1)[0])
        if not local: continue
        # Leading slash is a site-root URL, not a filesystem path outside the repo.
        target = (ROOT / local.lstrip("/")) if local.startswith("/") else ((ROOT / rel).parent / local)
        target = target.resolve()
        try: target.relative_to(ROOT)
        except ValueError:
            ERRORS.append(f"{rel}: {attr} escapes repository: {value}")
            continue
        # Directory routes such as /jt-apps/ resolve to index.html.
        if target.is_dir():
            target = target / "index.html"
        if not target.exists():
            ERRORS.append(f"{rel}: missing local {attr} target '{value}'")

def main():
    iter_files()
    for rel in HTML_FILES: check_html(rel)
    for rel in JS_FILES + CSS_FILES:
        try:
            if not (ROOT / rel).read_text(encoding="utf-8").strip():
                WARNINGS.append(f"Empty source file: {rel}")
        except Exception as exc:
            ERRORS.append(f"{rel}: cannot read source: {exc}")
    lines = [
        "# JT Command Agent — Static Audit", "",
        f"- HTML files scanned: {len(HTML_FILES)}",
        f"- JavaScript files scanned: {len(JS_FILES)}",
        f"- CSS files scanned: {len(CSS_FILES)}",
        f"- Errors: {len(ERRORS)}", f"- Warnings: {len(WARNINGS)}", "",
        "## Errors",
    ]
    lines.extend([f"- {x}" for x in ERRORS] if ERRORS else ["- None"])
    lines.extend(["", "## Warnings"])
    lines.extend([f"- {x}" for x in WARNINGS] if WARNINGS else ["- None"])
    lines.extend(["", "## Scope limits",
        "- Static checks only; remote endpoints are not assumed.",
        "- Does not verify runtime, backend health, desktop/mobile binaries, or external feedback channels.",
        "- No automatic code changes or production deployments are performed.", ""])
    report = "\n".join(lines)
    Path("jt-command-agent-report.md").write_text(report, encoding="utf-8")
    print(report)
    if ERRORS: sys.exit(1)

if __name__ == "__main__":
    main()
