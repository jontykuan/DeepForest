#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import os
import re
import sys
import json

# Paths
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CARDS_DIR = os.path.join(BASE_DIR, "src", "resources", "cards")
EVENTS_DIR = os.path.join(BASE_DIR, "src", "resources", "events")
SCRIPTS_DIR = os.path.join(BASE_DIR, "src", "scripts")
CATALOG_PATH = os.path.join(BASE_DIR, "ResourceCatalog.md")
CURATION_JSON_PATH = os.path.join(BASE_DIR, "ResourceCuration.json")

def parse_tres_file(filepath):
    """
    Parses a Godot .tres resource file.
    Returns: (header_lines, fields_dict)
    """
    header_lines = []
    fields = {}
    
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()
        
    # Split by [resource]
    parts = content.split("[resource]\n")
    if len(parts) < 2:
        parts = content.split("[resource]")
    
    if len(parts) < 2:
        # Not a valid resource file or empty
        return [content], {}
        
    header_lines = parts[0].splitlines()
    header_lines.append("[resource]")
    
    # Parse fields
    field_lines = parts[1].splitlines()
    for line in field_lines:
        line = line.strip()
        if not line or line.startswith(";"):
            continue
        if "=" in line:
            key, val_str = line.split("=", 1)
            key = key.strip()
            val_str = val_str.strip()
            
            # Type parsing
            if val_str.startswith('"') and val_str.endswith('"'):
                # String
                val = val_str[1:-1].replace('\\"', '"')
            elif val_str == "true":
                val = True
            elif val_str == "false":
                val = False
            elif val_str.startswith("ExtResource"):
                val = val_str # Keep raw ref
            elif "." in val_str:
                try:
                    val = float(val_str)
                except ValueError:
                    val = val_str
            else:
                try:
                    val = int(val_str)
                except ValueError:
                    val = val_str
            fields[key] = val
            
    return header_lines, fields

def write_tres_file(filepath, header_lines, fields):
    """
    Writes a Godot .tres resource file with modified fields.
    """
    lines = list(header_lines)
    # Ensure script is always first in fields if present
    ordered_keys = sorted(fields.keys())
    if "script" in fields:
        ordered_keys.remove("script")
        ordered_keys.insert(0, "script")
        
    for k in ordered_keys:
        val = fields[k]
        if isinstance(val, str):
            if val.startswith("ExtResource"):
                lines.append(f"{k} = {val}")
            else:
                escaped = val.replace('"', '\\"')
                lines.append(f'{k} = "{escaped}"')
        elif isinstance(val, bool):
            lines.append(f"{k} = {str(val).lower()}")
        else:
            lines.append(f"{k} = {val}")
            
    # Add trailing empty line
    lines.append("")
    
    with open(filepath, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))

def get_card_type_name(val):
    types = ["力量行動", "敏捷行動", "智慧行動", "消耗品", "裝備", "被動", "重要道具", "詛咒", "傷勢"]
    try:
        idx = int(val)
        if 0 <= idx < len(types):
            return types[idx]
    except Exception:
        pass
    return str(val)

def extract_endings():
    """
    Parses C# files for endings definition.
    """
    endings = []
    
    # 1. EndingManager.cs
    em_path = os.path.join(SCRIPTS_DIR, "narrative", "EndingManager.cs")
    if os.path.exists(em_path):
        with open(em_path, "r", encoding="utf-8") as f:
            content = f.read()
        # Find TriggerEnding calls
        matches = re.findall(r'TriggerEnding\(\s*EndingType\.[A-Za-z]+,\s*"([^"]+)",\s*"([^"]+)"\)', content)
        for m in matches:
            endings.append({"title": m[0], "conditions": "全域/數值消耗", "description": m[1]})
            
    # 2. Handlers directory
    handlers_dir = os.path.join(SCRIPTS_DIR, "narrative", "handlers")
    if os.path.exists(handlers_dir):
        for filename in os.listdir(handlers_dir):
            if filename.endswith(".cs") and filename != "ICharacterStoryHandler.cs" and filename != "StoryHandlerFactory.cs":
                filepath = os.path.join(handlers_dir, filename)
                with open(filepath, "r", encoding="utf-8") as f:
                    content = f.read()
                # Search for ending registrations or CheckEscapeEndings/CheckHiddenStatEndings return statements
                matches = re.findall(r'new\s+EndingResult\(\s*"[^"]+"\s*,\s*"([^"]+)"\s*,\s*"([^"]+)"\)', content)
                for m in matches:
                    endings.append({"title": m[0], "conditions": f"專屬角色 ({filename.replace('StoryHandler.cs','')})", "description": m[1]})
                    
    # Deduplicate by title
    seen = set()
    deduped = []
    for e in endings:
        if e["title"] not in seen:
            seen.add(e["title"])
            deduped.append(e)
    return deduped

def cmd_export():
    """
    Dumps all resources to ResourceCuration.json
    """
    data = {"cards": [], "events": []}
    
    # Scan cards
    if os.path.exists(CARDS_DIR):
        for filename in sorted(os.listdir(CARDS_DIR)):
            if filename.endswith(".tres"):
                filepath = os.path.join(CARDS_DIR, filename)
                _, fields = parse_tres_file(filepath)
                fields["_file"] = os.path.relpath(filepath, BASE_DIR).replace("\\", "/")
                data["cards"].append(fields)
                
    # Scan events
    if os.path.exists(EVENTS_DIR):
        for filename in sorted(os.listdir(EVENTS_DIR)):
            if filename.endswith(".tres"):
                filepath = os.path.join(EVENTS_DIR, filename)
                _, fields = parse_tres_file(filepath)
                fields["_file"] = os.path.relpath(filepath, BASE_DIR).replace("\\", "/")
                data["events"].append(fields)
                
    with open(CURATION_JSON_PATH, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print(f"[Resource Curator] 已匯出資源配置至 {CURATION_JSON_PATH}")

def cmd_import():
    """
    Applies edits from ResourceCuration.json back to .tres files
    """
    if not os.path.exists(CURATION_JSON_PATH):
        print(f"[Error] 找不到 {CURATION_JSON_PATH}。請先執行 export！")
        return
        
    with open(CURATION_JSON_PATH, "r", encoding="utf-8") as f:
        data = json.load(f)
        
    count = 0
    for card in data.get("cards", []):
        rel_path = card.get("_file")
        if not rel_path:
            continue
        filepath = os.path.join(BASE_DIR, rel_path)
        if os.path.exists(filepath):
            header, fields = parse_tres_file(filepath)
            # Update fields from JSON (except _file)
            for k, v in card.items():
                if k != "_file":
                    fields[k] = v
            write_tres_file(filepath, header, fields)
            count += 1
            
    for ev in data.get("events", []):
        rel_path = ev.get("_file")
        if not rel_path:
            continue
        filepath = os.path.join(BASE_DIR, rel_path)
        if os.path.exists(filepath):
            header, fields = parse_tres_file(filepath)
            for k, v in ev.items():
                if k != "_file":
                    fields[k] = v
            write_tres_file(filepath, header, fields)
            count += 1
            
    print(f"[Resource Curator] 成功更新了 {count} 個資源檔案！")

def cmd_catalog():
    """
    Generates ResourceCatalog.md
    """
    # 1. Load cards
    cards = []
    if os.path.exists(CARDS_DIR):
        for filename in sorted(os.listdir(CARDS_DIR)):
            if filename.endswith(".tres"):
                filepath = os.path.join(CARDS_DIR, filename)
                _, fields = parse_tres_file(filepath)
                cards.append(fields)
                
    # 2. Load endings
    endings = extract_endings()
    
    # Build MD
    md = []
    md.append("# 遊戲資源目錄 (Resource Catalog)")
    md.append("> 本文件為自動生成的遊戲卡牌、事件與結局清單，用於手動編修與設計參考。")
    md.append("")
    
    # Cards Table
    md.append("## 一、卡牌資源 (Cards)")
    md.append("| ID | 名稱 | 類型 | 重量 | 力量值 | 敏捷值 | 智慧值 | 飢餓消耗 | 口渴消耗 | 體力消耗 | 理智消耗 | 暴戾 | 穢祟 | 邪惡 | 描述 |")
    md.append("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|")
    for c in cards:
        c_id = c.get("CardId", "")
        c_name = c.get("CardName", "")
        c_type_val = c.get("CardClass") if "CardClass" in c else c.get("CardType", "")
        c_type = get_card_type_name(c_type_val)
        weight = c.get("Weight", 0)
        str_val = c.get("StrValue", 0)
        dex_val = c.get("DexValue", 0)
        wis_val = c.get("WisValue", 0)
        h_cost = c.get("HungerCost", 0)
        t_cost = c.get("ThirstCost", 0)
        hp_cost = c.get("HpCost", 0)
        san_cost = c.get("SanityCost", 0)
        b_change = c.get("BrutalityChange", 0)
        cr_change = c.get("CorruptionChange", 0)
        ev_change = c.get("EvilChange", 0)
        desc = c.get("Description", "")
        md.append(f"| {c_id} | {c_name} | {c_type} | {weight} | {str_val} | {dex_val} | {wis_val} | {h_cost} | {t_cost} | {hp_cost} | {san_cost} | {b_change} | {cr_change} | {ev_change} | {desc} |")
    md.append("")
    
    # Endings Table
    md.append("## 二、結局清單 (Endings)")
    md.append("| 結局名稱 | 觸發條件 | 結局描述 |")
    md.append("|---|---|---|")
    for e in endings:
        md.append(f"| {e['title']} | {e['conditions']} | {e['description']} |")
    md.append("")
    
    with open(CATALOG_PATH, "w", encoding="utf-8") as f:
        f.write("\n".join(md))
    print(f"[Resource Curator] 已產生資源目錄至 {CATALOG_PATH}")

def main():
    if len(sys.argv) < 2:
        print("使用說明:")
        print("  python tools/resource_curator.py export  - 匯出所有 .tres 為單一 JSON 配置檔")
        print("  python tools/resource_curator.py import  - 將 JSON 變更套用回各 .tres 檔案")
        print("  python tools/resource_curator.py catalog - 自動產生 ResourceCatalog.md")
        sys.exit(1)
        
    cmd = sys.argv[1].lower()
    if cmd == "export":
        cmd_export()
    elif cmd == "import":
        cmd_import()
    elif cmd == "catalog":
        cmd_catalog()
    else:
        print(f"[Error] 未知的命令: {cmd}")
        sys.exit(1)

if __name__ == "__main__":
    main()
