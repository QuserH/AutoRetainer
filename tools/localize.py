# -*- coding: utf-8 -*-
"""
AutoRetainer 汉化工具 v2（上下文感知）
- 只翻译 UI 展示上下文中的字符串字面量（Widget/Checkbox/Button/Text/Notify/Path 等）
- 跳过：注释、##ID、含中文、key==value 占位、逻辑比较/Throttle/命令/IPC/日志等黑名单上下文
- 词典 zh-CN.json 为简体中文（已由 zh-TW.json 转换）
"""
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
SRC = os.path.join(ROOT, "AutoRetainer")
DICT = os.path.join(ROOT, "tools", "zh-CN.json")

def normalize_newlines(s):
    return s.replace("\\n", "\n").replace("\\r", "\r").replace("\\t", "\t")

with open(DICT, encoding="utf-8-sig") as f:
    data = json.load(f)

dict_lower = {}
for k, v in data.items():
    if k != v and any("\u4e00" <= c <= "\u9fff" for c in v):
        dict_lower[k.lower()] = normalize_newlines(v)

UI_METHODS = {
    "widget", "checkbox", "button", "radiobutton", "text", "textwrapped", "label",
    "combo", "combostr", "comboenum", "enumcombo", "comboitem", "header", "section",
    "tooltip", "menuitem", "inputtext", "inputint", "inputfloat", "inputdouble",
    "sliderint", "sliderfloat", "sliderintasfloat", "collapsingheader", "selectable",
    "treenode", "bullett", "bullettext", "tab", "tabitem", "drawsection", "drawbutton",
    "drawhelp", "infodialog", "confirmdialog", "smallbutton", "arrowbutton",
    "imagebutton", "closebutton", "notify", "info", "success", "error", "warning",
    "title", "windowtitle", "childlabel", "helptext", "menubutton",
    "tabtitle", "tabname", "listbox", "listboxstr", "beginlistbox", "setnexttooltip",
    "linecentered", "textcentered", "drawtext", "drawtooltip", "hint", "getbutton",
    "input", "search", "searchstr", "searchext", "searchbox",
    "preset", "presetcombo", "margintooltip", "checkboxgroup", "group", "radio",
    "confirm", "popup", "beginpopupmodal", "textdisclaimer", "labeltext",
    "iconbuttonwithtext", "textv", "begincombo", "inputtextwithhint", "textwrapped",
    "textcentered", "texttitled", "drawlabel", "hint", "subheader", "samelinehelp",
    "spinner", "textcopyable", "helpmarker", "hoveredandclicked",
}

BLACK_METHODS = {
    "equals", "equalsany", "equalsignorecase", "equalsignorecaseany", "contains",
    "containsany", "startswith", "startswithany", "endswith", "endswithany",
    "indexof", "compareto", "throttle", "pushid", "popid", "beginpopup", "openpopup",
    "closepopup", "setclipboardtext", "getconfig", "setconfig", "tostring", "parse",
    "getname", "getnames", "getfield", "getproperty", "addtag", "gettag", "hastag",
    "find", "where", "select", "first", "firstordefault", "any", "all", "count",
    "getrow", "gettext", "cleanup", "todalamudstring", "split", "join", "replace",
    "trim", "trimstart", "trimend", "toupper", "tolower", "substring", "format",
    "concat", "trygetvalue", "getoradd", "containskey", "containsvalue", "getsheet",
    "getexcel", "executecommand", "processcommand", "remove", "insert", "append",
    "getaddon", "trygetaddon", "validate", "log", "debug", "print", "write", "warn",
    "info", "trace", "ismatch", "matches", "gettype", "create", "instantiate",
    "getvalue", "getint", "getbool", "getstring", "getuint", "getfloat",
    "getlong", "getulong", "getbyte", "getshort", "getushort", "tryget", "getindex",
    "lastindexof", "getfilename", "getdirectoryname", "combine", "getfullpath",
    "getnamewithout", "getextension", "getcurrentdirectory", "getkey", "getvalueornull",
}

def unescape(s):
    out = []
    i = 0
    n = len(s)
    while i < n:
        c = s[i]
        if c == "\\" and i + 1 < n:
            nxt = s[i + 1]
            if nxt == "n":
                out.append("\n"); i += 2; continue
            if nxt == "r":
                out.append("\r"); i += 2; continue
            if nxt == "t":
                out.append("\t"); i += 2; continue
            if nxt == '"':
                out.append('"'); i += 2; continue
            if nxt == "'":
                out.append("'"); i += 2; continue
            if nxt == "\\":
                out.append("\\"); i += 2; continue
            if nxt == "0":
                out.append("\0"); i += 2; continue
            if nxt == "u" and i + 5 < n:
                try:
                    out.append(chr(int(s[i + 2:i + 6], 16))); i += 6; continue
                except ValueError:
                    pass
        out.append(c)
        i += 1
    return "".join(out)

def escape(s):
    return s.replace("\\", "\\\\").replace('"', '\\"').replace("\n", "\\n").replace("\r", "\\r").replace("\t", "\\t")

def tokenize(text, in_block=0):
    """对整个文件文本 tokenize。返回 tokens: list of (kind, text)，kind ∈ {code, string, comment}"""
    tokens = []
    i = 0
    n = len(text)
    buf = []
    def flush():
        if buf:
            tokens.append(("code", "".join(buf)))
            buf.clear()
    while i < n:
        c = text[i]
        if in_block:
            j = text.find("*/", i)
            if j == -1:
                tokens.append(("comment", text[i:]))
                i = n
                continue
            tokens.append(("comment", text[i:j + 2]))
            in_block = 0
            i = j + 2
            continue
        if c == "/" and i + 1 < n and text[i + 1] == "/":
            flush()
            j = text.find("\n", i)
            if j == -1:
                j = n
            tokens.append(("comment", text[i:j]))
            i = j
            continue
        if c == "/" and i + 1 < n and text[i + 1] == "*":
            flush()
            j = text.find("*/", i + 2)
            if j == -1:
                tokens.append(("comment", text[i:]))
                return tokens, 1
            tokens.append(("comment", text[i:j + 2]))
            i = j + 2
            continue
        if text.startswith('"""', i):
            flush()
            j = text.find('"""', i + 3)
            if j == -1:
                tokens.append(("raw_string", text[i:]))
                i = n
                continue
            j += 3
            tokens.append(("raw_string", text[i:j]))
            i = j
            continue
        if c == '"':
            flush()
            j = i + 1
            while j < n:
                if text[j] == "\\":
                    j += 2
                    continue
                if text[j] == '"':
                    j += 1
                    break
                j += 1
            tokens.append(("string", text[i:j]))
            i = j
            continue
        buf.append(c)
        i += 1
    flush()
    return tokens, in_block

def get_method_name(line, start):
    """字符串 token 起始于 start。倒序括号配对扫描（含 {} lambda 块），找到包含它的最外层调用方法名。"""
    prefix = line[:start]
    depth = 0
    curly = 0
    i = len(prefix) - 1
    while i >= 0:
        c = prefix[i]
        if c == ")":
            depth += 1
        elif c == "}":
            curly += 1
        elif c == "{":
            if curly > 0:
                curly -= 1
        elif c == "(":
            if depth > 0:
                depth -= 1
            else:
                m = re.search(r"([A-Za-z_][\w]*(?:\.[A-Za-z_][\w]*)*)\s*$", prefix[:i])
                if m:
                    return m.group(1).split(".")[-1].lower()
                return None
        i -= 1
    return None

def get_expr_context(line, start):
    prefix = line[:start]
    if re.search(r"=>\s*(?:\$|@\$|\$@)?$", prefix):
        return "arrow"
    return None

def normalize_raw_content(raw):
    raw = raw.replace("\r\n", "\n")
    if raw.startswith("\n"):
        raw = raw[1:]
    lines = raw.split("\n")
    if lines and not lines[-1].strip():
        lines.pop()
    indents = [len(line) - len(line.lstrip(" \t")) for line in lines if line.strip()]
    if indents:
        indent = min(indents)
        lines = [line[indent:] if len(line) >= indent else "" for line in lines]
    return "\n".join(lines).strip()

def escape_raw_string(value, original):
    value = normalize_newlines(value).replace("\r\n", "\n")
    indents = re.findall(r"(?:\n|\\n)([ \t]+)\S", original)
    indent = min(indents, key=len) if indents else ""
    body = "\n".join(f"{indent}{line}" if line else "" for line in value.split("\n"))
    return f'"""\n{body}\n{indent}"""'

def process_text(text, file_rel, stats, collected):
    """对整个文件文本处理。返回替换后的文本。"""
    tokens, _ = tokenize(text, 0)
    pos = 0
    out = []
    for kind, text_tok in tokens:
        if kind not in ("string", "raw_string"):
            out.append(text_tok)
            pos += len(text_tok)
            continue
        is_raw = kind == "raw_string"
        raw = text_tok[3:-3] if is_raw else text_tok[1:-1]
        content = normalize_raw_content(raw) if is_raw else unescape(raw)
        if content.startswith("##") or content == "":
            out.append(text_tok)
            pos += len(text_tok)
            continue
        if any("\u4e00" <= c <= "\u9fff" for c in content):
            content = normalize_newlines(content)
            out.append(escape_raw_string(content, raw) if is_raw else '"' + escape(content) + '"')
            pos += len(text_tok)
            continue

        interp = not is_raw and ((pos > 0 and text[pos - 1] == "$") or \
                  (pos > 1 and text[pos - 1] == "@" and text[pos - 2] == "$"))
        exprs = []
        template = content
        if interp:
            def grab(m2):
                exprs.append(m2.group(0))
                return "{}"
            template = re.sub(r"\{[^{}]*\}", grab, content)

        translated = None
        method = get_method_name(text, pos)
        arrow = get_expr_context(text, pos)
        name_ctx = get_name_context(text, pos)
        tuple_ctx = get_tuple_context(text, pos)
        rel_slash = file_rel.replace("\\", "/")
        in_ui = "/UI/" in rel_slash

        if method in UI_METHODS:
            translated = try_translate(template, dict_lower)
            stats["ui"] += 1
            if not translated:
                stats["ui_miss"].setdefault(template, 0)
                stats["ui_miss"][template] += 1
        elif method in BLACK_METHODS:
            stats["black"] += 1
        elif (arrow == "arrow" or name_ctx == "name") and in_ui:
            translated = translate_segments(template, dict_lower)
            if arrow == "arrow":
                stats["arrow"] += 1
            else:
                stats["name"] += 1
            if not translated:
                stats["arrow_miss"].setdefault(template, 0)
                stats["arrow_miss"][template] += 1
        elif tuple_ctx and in_ui:
            translated = try_translate(template, dict_lower)
            stats["tuple"] += 1
            if not translated:
                stats["arrow_miss"].setdefault(template, 0)
                stats["arrow_miss"][template] += 1
        else:
            stats["other"] += 1
            if collected is not None:
                collected.append((file_rel, content))

        if translated:
            stats["translated"] += 1
            if interp:
                n = len(exprs)
                def restore(m2):
                    if m2.group(1) is not None:
                        idx = int(m2.group(1))
                        return exprs[idx] if idx < n else m2.group(0)
                    restore.i += 1
                    idx = restore.i - 1
                    return exprs[idx] if idx < n else m2.group(0)
                restore.i = 0
                result = re.sub(r"\{(\d+)\}|\{\}", restore, translated)
            else:
                result = translated
            result = normalize_newlines(result)
            out.append(escape_raw_string(result, raw) if is_raw else '"' + escape(result) + '"')
        else:
            if is_raw and "\\n" in raw:
                out.append(text_tok.replace("\\n", "\n").replace("\\r", "\r").replace("\\t", "\t"))
            else:
                out.append(text_tok)
        pos += len(text_tok)
    return "".join(out)

def try_translate(content, table):
    variants = {content.lower(), content.lower().replace("\\n", "\n")}
    for v in variants:
        hit = table.get(v)
        if hit:
            return hit
    return None

def translate_segments(s, table):
    """整段查词典；失败则按 / 拆段逐段查并拼接。有翻译返回翻译，否则 None"""
    hit = try_translate(s, table)
    if hit:
        return hit
    if "/" in s:
        parts = s.split("/")
        out = []
        changed = False
        for p in parts:
            t = try_translate(p, table)
            if t:
                out.append(t)
                changed = True
            else:
                out.append(p)
        if changed:
            return "/".join(out)
    return None

def get_name_context(line, start):
    """检测 Name 属性赋值上下文：Name { get; } = "..." """
    prefix = line[:start]
    if re.search(r"Name\s*\{\s*get;\s*\}\s*=\s*$", prefix):
        return "name"
    return None

def get_tuple_context(line, start):
    """检测元组/数组元素上下文：( "...", 或 , "..." 或 { "..." """
    prefix = line[:start]
    if re.search(r"\(\s*$", prefix):
        return "tuple"
    if re.search(r"\{\s*$", prefix):
        return "tuple"
    if re.search(r",\s*$", prefix):
        return "tuple"
    return None

def main():
    only_report = "--report" in sys.argv
    stats = {"ui": 0, "black": 0, "arrow": 0, "name": 0, "tuple": 0, "other": 0, "translated": 0,
             "ui_miss": {}, "arrow_miss": {}}
    collected = []
    changed_files = []

    for root, dirs, fns in os.walk(SRC):
        dirs[:] = [d for d in dirs if d not in (".git", "bin", "obj")]
        for fn in sorted(fns):
            if not fn.endswith(".cs"):
                continue
            fp = os.path.join(root, fn)
            rel = os.path.relpath(fp, ROOT)
            if fn == "Lang.cs":
                continue
            with open(fp, encoding="utf-8", errors="replace") as f:
                text = f.read()
            new_text = process_text(text, rel, stats, collected)
            if new_text != text:
                changed_files.append(rel)
                if not only_report:
                    with open(fp, "w", encoding="utf-8", newline="") as f:
                        f.write(new_text)

    print(f"UI 上下文命中: {stats['ui']}, 已翻译: {stats['translated']}, 黑名单跳过: {stats['black']}, arrow(Path): {stats['arrow']}, Name属性: {stats['name']}, 元组/数组: {stats['tuple']}, 其他上下文: {stats['other']}")
    print(f"改动文件数: {len(changed_files)}")
    miss = sorted(stats["ui_miss"].items(), key=lambda x: -x[1])
    print(f"\nUI 上下文但词典未覆盖（{len(miss)} 条）:")
    for k, c in miss[:60]:
        print(f"  [{c}] {k!r}")
    amiss = sorted(stats["arrow_miss"].items(), key=lambda x: -x[1])
    print(f"\nPath 但词典未覆盖（{len(amiss)} 条）:")
    for k, c in amiss[:10]:
        print(f"  [{c}] {k!r}")

if __name__ == "__main__":
    main()
