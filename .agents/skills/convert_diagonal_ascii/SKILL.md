---
name: convert-diagonal-ascii
description: Convert source images into diagonally sliced, feathered 136x48 Monospace ASCII art textures for the DeepForest engine.
---

# convert-diagonal-ascii 技能操作手冊

本技能引導 AI Agent 如何使用專屬 Python 工具將任意圖片轉化為適用於本遊戲 Text-Grid 渲染系統的對角線切分元件。

## 1. 使用情境 (When to Use)
* 當玩家提供新的實景森林、小徑、背景參考圖，並要求將其製作為場景地形或地面貼圖時。
* 當需要新增或更新 `woodland`、`swamp`、`ruins`、`stone_wall` 等地形或 `grass`、`dirt` 等地面時。

## 2. 幾何裁切與遮罩規範 (Geometry & Masking Rules)
畫布大小固定為 136×48 網格，消失點中心定義為 `(68, 22)`：
1. **左地形 (Left Triangle)**：
   * 範圍限制在左側三角形內：`x < 68` 且 `y >= 22 * x / 68` 且 `y <= 22 + 25 * (1 - x / 68)`。
   * 其他區域強制輸出透明空格。
2. **右地形 (Right Triangle)**：
   * 範圍限制在右側三角形內：`x >= 68` 且 `y >= 22 * (1 - (x - 68) / 67)` 且 `y <= 22 + 25 * ((x - 68) / 67)`。
   * 其他區域強制輸出透明空格。
3. **地面 (Bottom Triangle)**：
   * 範圍限制在下側三角形內：`y >= 22` 且不屬於左右三角形的區域。
4. **天空 (Top Triangle)**：
   * 範圍限制在上側三角形內：`y < 22` 且不屬於左右三角形的區域。

## 3. 邊緣羽化衰減 (Feathering & Fading)
* 為了消除疊圖時的方形外框硬邊界，在對角線邊緣的 3 像素高度內、以及靠近中心消失點的 12 像素寬度內，必須套用線性衰減 `fade_factor`（0.0 ~ 1.0）。
* 亮度值乘以 `fade_factor` 降為 0 後會對應到空格 `" "`（100% 透明），實現邊界無縫平滑融合。

## 4. 工具化命令執行 (Execution)
在專案根目錄下執行以下命令：
```bash
python .agents/skills/convert_diagonal_ascii/scripts/convert_diagonal_ascii.py --source <圖片路徑> --type <all|left|right|base|ground> --name <地形/地面名稱>
```

### 參數說明：
* `--source`：來源圖片的絕對路徑。
* `--type`：欲產出的元件類型。可選：
  * `all`：輸出完整的 left, right, base 及 ground。
  * `left`：僅輸出左地形元件 (`terrain_<name>_left.png`)。
  * `right`：僅輸出右地形元件 (`terrain_<name>_right.png`)。
  * `base`：僅輸出透視背景小徑 (`perspective_template.png`)。
  * `ground`：僅輸出下三角地面元件 (`ground_<name>.png`)。
* `--name`：地形/地面在遊戲中的識別名稱（例如：`woodland`、`swamp`、`grass` 等）。
