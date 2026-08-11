# -*- coding: utf-8 -*-
"""生成 UI 上下文中未被词典覆盖的字符串清单（按出现次数排序）"""
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from localize import tokenize, get_method_name, get_expr_context, get_name_context, \
    translate_segments, UI_METHODS, BLACK_METHODS, unescape, normalize_newlines, \
    normalize_raw_content

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
SRC = os.path.join(ROOT, "AutoRetainer")
DICT = os.path.join(ROOT, "tools", "zh-CN.json")

with open(DICT, encoding="utf-8-sig") as f:
    data = json.load(f)

dict_lower = {}
for k, v in data.items():
    if k != v and any("\u4e00" <= c <= "\u9fff" for c in v):
        dict_lower[k.lower()] = normalize_newlines(v)

def walk():
    for root, dirs, fns in os.walk(SRC):
        dirs[:] = [d for d in dirs if d not in (".git", "bin", "obj")]
        for fn in sorted(fns):
            if fn.endswith(".cs") and fn != "Lang.cs":
                yield os.path.join(root, fn)

miss = {}
for fp in walk():
    rel = os.path.relpath(fp, ROOT)
    rel_slash = rel.replace("\\", "/")
    if "/UI/NeoUI/AdvancedEntries/DebugSection/" in f"/{rel_slash}":
        continue
    with open(fp, encoding="utf-8", errors="replace") as f:
        text = f.read()
    tokens, _ = tokenize(text, 0)
    pos = 0
    for kind, tok in tokens:
        if kind in ("string", "raw_string"):
            is_raw = kind == "raw_string"
            content = normalize_raw_content(tok[3:-3]) if is_raw else unescape(tok[1:-1])
            interp = not is_raw and ((pos > 0 and text[pos - 1] == "$") or \
                    (pos > 1 and text[pos - 1] == "@" and text[pos - 2] == "$")
            )
            if interp:
                content = re.sub(r"\{[^{}]*\}", "{}", content)
            if not (content.startswith("##") or content == "" or
                    content.startswith("loading ") or content == "LoginCenter" or
                    content in ("Debug", "Discord") or content.startswith(("About", "planbuttons")) or
                    all("\ue000" <= char <= "\uf8ff" for char in content) or
                    any(marker in content for marker in ("{", "}", "##", "\\u", "%")) or
                    re.fullmatch(r"[-~/:\\.\s]+", content) or
                    any("\u4e00" <= c <= "\u9fff" for c in content)):
                method = get_method_name(text, pos)
                arrow = get_expr_context(text, pos)
                name_ctx = get_name_context(text, pos)
                is_ui = method in UI_METHODS or \
                    ((arrow == "arrow" or name_ctx == "name") and "/UI/" in rel_slash)
                if is_ui:
                    t = translate_segments(content, dict_lower)
                    if not t:
                        miss.setdefault(content, [0, set()])
                        miss[content][0] += 1
                        miss[content][1].add(rel.split("AutoRetainer\\")[-1].split("AutoRetainer/")[-1])
        pos += len(tok)

items = sorted(miss.items(), key=lambda x: -x[1][0])
print(f"未覆盖 UI 字符串总数（去重）: {len(items)}")
with open(os.path.join(ROOT, "tools", "_ui_missing.txt"), "w", encoding="utf-8") as f:
    for content, (cnt, files) in items:
        f.write(f"[{cnt}] {content!r}  # {', '.join(sorted(files))}\n")
print("清单已写入 tools/_ui_missing.txt")
