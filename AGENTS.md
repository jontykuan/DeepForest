# Deep Forest — Agent 開發指引 (Development Guide)

> 本文件是 AI Agent 與開發者在本專案中進行開發時的行為規範與技術指南。
> 所有開發行為應以 [GAME_DESIGN.md](file:///d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot%20project/DeepForest/GAME_DESIGN.md) 為唯一權威設計參考。

---

## 一、專案概要

| 項目 | 值 |
|------|------|
| **引擎** | Godot 4.6 Mono |
| **程式語言** | C# (.NET 8.0) |
| **畫面風格** | ASCII Art（Text-Grid 文字網格渲染，2ch AA 線條風格） |
| **目標平台** | Windows 單機 |
| **專案根目錄** | `d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot project/DeepForest/` |

---

## 二、目錄結構

```
DeepForest/
├── GAME_DESIGN.md                 # 遊戲設計文件（唯一設計權威）
├── AGENTS.md                      # 本文件（Agent 開發指引）
├── 遊戲大綱.txt                    # 原始遊戲大綱
├── project.godot                  # Godot 專案設定
├── DeepForest.csproj              # C# 專案檔
├── DeepForest.sln                 # C# 解決方案
│
├── assets/                        # 靜態資源
│   ├── fonts/                     # 字型檔（等寬字型，如 Sarasa Mono TC）
│   ├── ascii_art/                 # ASCII Art 文字資源
│   │   ├── avatars/               # 人物頭像 ASCII Art
│   │   │   ├── default_male/      # 白板角色頭像
│   │   │   │   ├── base.txt       # 頭像文字模板
│   │   │   │   └── base.tags      # 頭像區域標籤檔
│   │   │   └── .../
│   │   ├── scenes/                # 場景 ASCII Art 模板
│   │   │   ├── riverside/         # 河畔場景
│   │   │   │   ├── base.txt       # 基礎場景模板
│   │   │   │   ├── base.tags      # 區域標籤檔
│   │   │   │   ├── overlay_rain.txt
│   │   │   │   ├── overlay_fog.txt
│   │   │   │   └── sub/           # 可替換子圖
│   │   │   │       ├── campfire.txt
│   │   │   │       └── tent.txt
│   │   │   └── .../
│   │   └── ui/                    # UI 用 ASCII 元素
│   └── shaders/                   # Shader 檔案（僅用於簡單 UI 特效）
│
└── src/                           # 所有原始碼與場景
    ├── resources/                 # Godot Custom Resource (.tres)
    │   ├── cards/                 # 卡牌資源定義
    │   └── events/                # 敘事事件資源
    ├── scenes/                    # Godot 場景 (.tscn)
    │   └── MainScene.tscn         # 主場景（UI Layout）
    └── scripts/                   # C# 腳本 (.cs)
        ├── core/                  # 核心系統
        │   ├── GameState.cs       # 遊戲狀態管理（Autoload Singleton）
        │   ├── TurnManager.cs     # 回合流程控制
        │   └── EnvironmentSystem.cs # 天氣/溫度/濕度系統
        ├── cards/                 # 卡牌相關
        │   ├── Card.cs            # 卡牌基類 Resource
        │   ├── Deck.cs            # 牌組管理（抽牌、洗牌、棄牌）
        │   └── CardEffect.cs      # 卡牌效果定義
        ├── character/             # 角色相關
        │   ├── Player.cs          # 玩家角色（明示量表 + 隱藏數值）
        │   ├── Equipment.cs       # 裝備管理
        │   └── StatusEffect.cs    # 狀態效果（詛咒/傷勢/心靈創傷）
        ├── rendering/             # Text-Grid 渲染系統
        │   ├── CharCell.cs        # 字元格資料結構
        │   ├── TextGrid.cs        # 二維字元陣列管理（拼貼、覆蓋、輸出）
        │   ├── AsciiTemplate.cs   # ASCII Art 模板載入（.txt + .tags）
        │   ├── SceneRenderer.cs   # 場景渲染器（模板 + 子圖拼貼 + 特效）
        │   ├── AvatarRenderer.cs  # 頭像渲染器（區域標籤 + 動態表情）
        │   ├── MapRenderer.cs     # 地圖渲染器（程序化節點圖）
        │   └── EffectRenderer.cs  # 特效渲染器（亂碼、隱藏字、色彩變換）
        ├── scene/                 # 場景互動
        │   ├── SceneData.cs       # 場景資料定義（行動列表、門檻）
        │   ├── SceneAction.cs     # 場景行動定義
        │   └── MapManager.cs      # 地圖管理（節點、路徑、進度）
        ├── combat/                # 戰鬥系統
        │   ├── Enemy.cs           # 敵人定義
        │   └── CombatManager.cs   # 戰鬥流程控制
        ├── narrative/             # 敘事系統
        │   ├── EventManager.cs    # 事件觸發與管理
        │   ├── EndingManager.cs   # 結局判定
        │   └── StoryUnlock.cs     # 背景故事解鎖追蹤
        └── ui/                    # UI 元件
            ├── MainScene.cs       # 主場景腳本（佈局管理）
            ├── HandUI.cs          # 手牌區 UI
            ├── StatsPanel.cs      # 狀態面板 UI
            ├── ActionPanel.cs     # 行動面板 UI
            ├── MapPanel.cs        # 地圖面板 UI
            └── SystemBanner.cs    # 系統資訊橫幅 UI
```

---

## 三、核心設計原則

### 3.1 唯一設計權威

- **所有遊戲邏輯必須以 `GAME_DESIGN.md` 為準**
- 若開發過程中發現設計文件的規格不完整或有矛盾，**先與使用者確認**，不得自行假設
- 修改遊戲設計需取得使用者明確同意後，同步更新 `GAME_DESIGN.md`

### 3.2 不混用其他遊戲概念

- **本專案與 M.U.T.E. 完全無關**。不得引入任何終端機、`[SYSTEM]`/`[SENSOR]`/`[ALERT]`/`[AUDIO]` 標籤系統或血肉龐克監控介面概念
- 本遊戲的系統訊息以正常遊戲 UI 方式呈現（例如 `第 3 天 │ 深度: 42M`），不使用任何終端模擬的敘事風格

### 3.3 ASCII Art 視覺規範

- 所有畫面呈現必須遵循 2ch AA 線條風格的 ASCII Art
- 使用**等寬字型**繪製，半形 1 格、全形 2 格，嚴格對齊
- 渲染核心為 **Text-Grid（文字網格）系統**：C# 二維字元陣列 → BBCode → `RichTextLabel`
- **不使用 3D 場景或 Post-Processing Shader**
- 嚴格遵守色彩系統（見 `GAME_DESIGN.md` 第二節）：黑底、白線、輻射綠主色調
- 暗紅 (`#660000`) 和深藍 (`#000066`) 在黑底上**故意設計為不易讀**，用於穢祟暗示和隱藏線索
- 隱藏字（暗色字）的字元寬度**必須**與該位置原始字元寬度一致，否則整行會錯位

---

## 四、程式碼規範

### 4.1 C# 編碼風格

```csharp
// ✅ 使用 PascalCase 命名類別、方法、屬性
public class CardEffect { }
public void ApplyDamage(int amount) { }
public int MaxHunger { get; set; }

// ✅ 使用 camelCase 命名區域變數與參數
int currentHunger = 50;
void ProcessCard(Card card, Player player) { }

// ✅ 使用 _camelCase 命名私有欄位
private int _currentDay;
private List<Card> _drawPile;

// ✅ 常數使用 PascalCase
public const int DefaultDeckCapacity = 30;

// ✅ 列舉值使用 PascalCase
public enum CardType
{
    ActionStr,
    ActionDex,
    ActionWis,
    Consumable,
    Equipment,
    Passive,
    KeyItem,
    Curse,
    Injury
}
```

### 4.2 Godot 節點與場景規範

- 場景檔案使用 **PascalCase** 命名（如 `MainScene.tscn`、`Forest3D.tscn`）
- C# 腳本檔名與類別名一致
- 節點名稱使用 **PascalCase**
- Signal 名稱使用 **PascalCase**（如 `CardPlayed`、`TurnEnded`）
- 使用 `[Export]` 屬性暴露 Inspector 參數，而非硬編碼數值

### 4.3 資源 (Resource) 使用

- 卡牌、場景行動、敵人等資料定義應使用 Godot 的 **Custom Resource** (`Godot.Resource` 子類別)
- 資源檔案放在 `src/resources/` 對應子目錄下
- 資源命名使用 **snake_case**（如 `card_slash.tres`、`scene_riverside.tres`）

### 4.4 訊號與事件架構

優先使用 Godot 的 Signal 系統進行模組間通訊：

```csharp
// 定義 Signal
[Signal] public delegate void CardPlayedEventHandler(Card card);
[Signal] public delegate void StatChangedEventHandler(string statName, int oldValue, int newValue);
[Signal] public delegate void DayChangedEventHandler(int newDay);

// 發送 Signal
EmitSignal(SignalName.CardPlayed, card);

// 連接 Signal（在 _Ready 中）
player.StatChanged += OnStatChanged;
```

---

## 五、模組開發指南

### 5.1 卡牌系統 (`src/scripts/cards/`)

- `Card.cs` 為 Godot Resource 子類別，包含所有卡牌屬性
- 必要欄位：`Name`, `CardType`, `Weight`, `StrValue`, `DexValue`, `WisValue`, `HungerCost`, `ThirstCost`, `HpCost`, `SanityCost`, `Description`
- 消耗品打出後從牌組移除；裝備遵循裝備/卸下流程
- `Deck.cs` 管理三個牌堆：`DrawPile`, `Hand`, `DiscardPile`，外加 `EquippedCards` 列表

### 5.2 角色系統 (`src/scripts/character/`)

- `Player.cs` 管理 4 個明示量表（HP, Sanity, Hunger, Thirst）和 3 個隱藏數值（Brutality, Corruption, Evil）
- 隱藏數值**永遠不暴露給 UI**，僅由 `EventManager` 和 `EndingManager` 讀取
- 角色參數（Draw, HandLimit, DeckCapacity 等）應定義為 Resource，便於建立不同角色

### 5.3 場景互動 (`src/scripts/scene/`)

- `SceneData.cs` 定義場景的行動列表，每個行動包含：名稱、門檻類型（STR/DEX/WIS）、門檻數值、前置條件（所需裝備/物品）、效果
- 行動狀態（可選/不足/未滿）由 UI 層根據當前累積值與條件即時計算
- 行動達標後玩家可自由選擇何時執行

### 5.4 回合管理 (`src/scripts/core/TurnManager.cs`)

- 回合流程嚴格按照 `GAME_DESIGN.md` 第十一節定義的順序執行
- 使用狀態機 (State Machine) 管理回合階段切換
- 換日判定：當 DrawPile 為空時自動觸發，或玩家主動紮營時觸發

### 5.5 結局系統 (`src/scripts/narrative/EndingManager.cs`)

- 結局判定順序：隱藏數值結局 > 物品組合結局 > 人間蒸發結局
- 背景故事解鎖以持久化存檔記錄，跨遊玩累計

---

## 六、Text-Grid 渲染開發指南

### 6.1 核心架構 (`src/scripts/rendering/`)

- `CharCell.cs`：字元格資料結構，包含 `Character`、`ForegroundColor`、`BackgroundColor`、`Tag`、`IsTransparent`
- `TextGrid.cs`：管理二維 `CharCell[,]` 陣列，提供拼貼（Blit）、區域填充、BBCode 輸出等方法
- `AsciiTemplate.cs`：負責載入 `.txt` + `.tags` 檔案並解析為 `TextGrid`

### 6.2 渲染器分工

| 渲染器 | 職責 |
|-------|------|
| `SceneRenderer.cs` | 載入場景模板、拼貼子圖、應用天氣覆蓋層 |
| `AvatarRenderer.cs` | 載入頭像模板、動態表情替換、傷勢/穢祟效果 |
| `MapRenderer.cs` | 程序化生成節點圖、迷霧效果、路徑繪製 |
| `EffectRenderer.cs` | 全域特效（亂碼、隱藏字、色彩偏移、閃爍） |

### 6.3 模板檔案規範

- `.txt` 檔案：純文字 ASCII Art，使用 UTF-8 編碼
- `.tags` 檔案：與 `.txt` 逐字元對應的區域標籤檔
- 子圖檔案（`sub/*.txt`）：局部 ASCII Art，透明格（空格）不覆蓋下層
- 覆蓋層（`overlay_*.txt`）：天氣/特效用，疊加在場景頂層
- 所有模板中的空白必須使用正確的佔位字元（見 `GAME_DESIGN.md` 2.4.2）

### 6.4 BBCode 輸出

- `TextGrid` 逐行掃描 `CharCell[,]`，依前景色產生 `[color=#RRGGBB]` BBCode 標籤
- 相鄰同色字元合併為同一個 `[color]` 區段，減少 BBCode 標籤數量
- 最終字串送入 Godot `RichTextLabel.BbcodeEnabled = true`
- `RichTextLabel` 使用等寬字型（`Theme` 或 `LabelSettings` 設定）

### 6.5 CJK 寬度處理

- 模板中混用半形/全形字元時，Tag 檔必須正確對齊
- 全形字元佔 2 個 Tag 位置（第二格使用特殊標記如 `_` 表示「延續前格」）
- `TextGrid.Blit()` 方法需處理全形字元的碰撞（覆蓋時需清除被覆蓋的全形字元第二格）

### 6.6 特效系統

- 特效透過 `Tag` 定位目標區域，避免硬編碼座標
- 常用特效：
  - **亂碼替換**：隨機替換指定 Tag 區域的字元為亂碼符號
  - **色彩偏移**：修改指定 Tag 區域的前景色
  - **閃爍**：定時器切換指定 Tag 區域的可見性
  - **隱藏字**：將指定 Tag 區域的前景色設為暗紅/深藍
  - **漸變**：逐格修改字元或顏色（用於迷霧揭開、失明等）

---

## 七、測試與驗證

### 7.1 建置與測試驗證範圍

```bash
# 在專案根目錄執行編譯
dotnet build
```

確認所有 C# 腳本編譯無誤。
在執行 E2E 測試或調整代碼時，應嚴格遵循**最小依賴範圍原則**：
* 每次修正或重構時，**只針對與該編輯過程直接有影響的依賴範圍進行測試修正**，避免對無關的系統或測試代碼進行不必要的修改。
* 優先執行與當前修改模組直接相關的單元/端到端測試，確認局部行為正確且無回歸後再行交付。

### 7.2 手動驗證清單

- [ ] 開啟 Godot 編輯器，能正確載入專案
- [ ] 執行 MainScene.tscn，確認 UI Layout 正確顯示
- [ ] 等寬字型正確載入，半形/全形對齊無誤
- [ ] Text-Grid 渲染系統正確將 ASCII Art 模板顯示在 RichTextLabel 中
- [ ] 場景模板載入並正確拼貼子圖
- [ ] 頭像 ASCII Art 正確顯示，Zone Tag 正確分配
- [ ] 地圖程序化生成正確，迷霧效果正常
- [ ] 色彩系統正確呈現（輻射綠、鮮紅、灰綠等）
- [ ] 隱藏字（暗紅/深藍）不影響周圍字元的對齊
- [ ] 出牌操作正確扣除 Cost（飢餓、口渴等）
- [ ] 行動面板正確顯示三種狀態（可選/不足/未滿）
- [ ] 裝備/卸下流程正確（卡牌進出牌組、使用卡生成/移除）
- [ ] 換日結算正確（飢渴消耗、體力/理智恢復）
- [ ] 隱藏數值不在 UI 中暴露
- [ ] 特效系統正確（亂碼、色彩偏移、隱藏字插入）

### 7.3 迭代流程

1. 先建立 Default 白板角色（成年男性）並完成核心遊戲循環
2. 用白板角色平衡基礎數值
3. 衍生其他年齡/性別角色，調整參數
4. 疊加職業與能力
5. 加入場景內容與敘事事件
6. 最終平衡與結局測試

---

## 八、禁止事項

1. **不得引入 M.U.T.E. 的任何概念**（終端機、標籤系統、血肉龐克）
2. **不得在 UI 中顯示隱藏數值**（暴戾、穢祟、邪惡）的具體數字
3. **不得隨意修改色彩定義**，所有顏色必須使用 `GAME_DESIGN.md` 中定義的 Hex 值
4. **不得假設未定義的遊戲機制**，遇到設計文件未涵蓋的情況應先與使用者確認
5. **不得在卡牌 Cost 中使用扣率制**（如「消耗 10% 飢餓」），所有 Cost 必須是固定整數值
6. **不得忽略牌組重量守恆原則**，裝備離開牌組後，卸下卡必須佔據等重空間
7. **不得使用 3D 場景或 Post-Processing Shader 進行 ASCII 渲染**，所有視覺內容透過 Text-Grid 文字網格系統渲染
8. **不得使用非等寬字元**在 ASCII Art 中，隱藏字的字元寬度必須與原始字元寬度一致（半形對半形、全形對全形）
