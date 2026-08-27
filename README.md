# Cocos2d-to-Unity Slot Refactoring

將舊版 Cocos2d-x (Quick Lua) 老虎機專案移植重構到 **Unity 2022.3 LTS (WebGL 目標)** 的完整專案。

---

## 🎯 專案簡介

- **原始專案**：Cocos2d-x Quick Lua 老虎機
- **目標引擎**：Unity 2022.3.24f1 LTS
- **目標平台**：WebGL（瀏覽器直接執行）
- **核心玩法**：5 軸 × 4 行、30 條支付線、Wild(ID8)、9/10 高賠特例、真實縱向滾動動畫、中獎線高亮

---

## 🏗️ 開發階段總覽

| Phase | 內容 | 關鍵產出 |
|-------|------|----------|
| **Phase 0** | 專案骨架、Assembly Definitions、Git 初始化 | `Core` / `Data` / `Game` / `Tests` asmdef |
| **Phase 1** | 判賞引擎核心邏輯 | `PaylineEvaluator.cs` (30 條支付線、Wild、特例) + 28 測試全綠 |
| **Phase 2a** | 切圖工具、UI 場景程式化建構、互動下注/SPIN | `SlotUiSceneBuilder.cs`、`split_plist.py`、Sprite Import Fixer |
| **Phase 2b** | 真實縱向滾動動畫、中獎線金框高亮、SPIN 按鈕圖 | `ReelColumn.cs`、`SlotGameController.cs` |
| **Phase 2b+** | **關鍵修復**：NRE 根因、WebGL Builder 自動化設定、Development Build 除錯 | `ReelColumn.Awake()` 重建引用、`SlotWebGLBuilder.cs` 全自動設定 |

---

## 📁 專案結構

```
Assets/
├── Resources/
│   └── Sprites/              # 切好圖資（水果 1~10 + UI 18 張）
├── Scenes/
│   └── SlotScene.unity       # 空場景，完全由 SlotUiSceneBuilder 建構
├── Scripts/
│   ├── Core/                 # 核心領域模型
│   │   ├── SymbolId.cs
│   │   ├── Payline.cs
│   │   └── WinResult.cs
│   ├── Data/                 # 靜態資料
│   │   └── SlotPaylines.cs   # 30 條支付線定義
│   ├── Game/                 # 遊戲邏輯
│   │   ├── ReelCell.cs       # 單格 Sprite 取得
│   │   ├── ReelColumn.cs     # 單軸滾動（6 icon 循環、Awake 重建引用）
│   │   ├── SlotGameController.cs  # 主控：Spin、停輪、判賞、高亮
│   │   ├── PaylineEvaluator.cs  # 判賞核心（Pure C#、可單測）
│   │   └── Editor/
│   │       ├── SlotUiSceneBuilder.cs      # 一鍵建構完整 UI 場景
│   │       ├── SlotWebGLBuilder.cs        # WebGL 建置自動化（關鍵設定）
│   │       ├── SlotTextureImporterFix.cs  # 批次設定 Sprite Import
│   │       ├── SpriteImportFixer.cs
│   │       └── WebGLSettingsApplier.cs
│   └── Tests/
│       └── PaylineEvaluatorTests.cs  # 28 測試全綠
Tools/
├── split_plist.py            # .plist 切圖工具（Pillow）
├── webgl_server.py           # 本機 HTTP Server（正確 gzip + no-store header）
└── run_webgl_server.bat
```

---

## ⚙️ 關鍵技術細節

### 1. 判賞引擎 (`PaylineEvaluator`)
- 30 條支付線定義於 `SlotPaylines.cs`
- Wild = `SymbolId.Wild (ID=8)`，可代替任意符號
- 9/10 號符號：3 連線即算中獎（高賠特例）
- 純 C#、無 Unity 依賴 → **可單元測試**（28 tests pass）

### 2. 真實縱向滾動 (`ReelColumn`)
- 6 個 icon 在遮罩內向下循環（y: 230 → -276 → wrap 到 276）
- 每格 92px，可視 4 格（row 1~4）
- 階梯停輪：左至右逐軸停、間隔 0.22s
- **關鍵修復**：`Awake()` 於 runtime 由 child `icon0..icon5` 重建 `private Image[] icons` 引用（解決 scene 序列化遺失 NRE）

### 3. WebGL 建置自動化 (`SlotWebGLBuilder.cs`)
每次建置自動執行：
```csharp
// 1. 清理舊輸出目錄
Directory.Delete(outDir, true);

// 2. 清理 IL2CPP 編譯快取
CleanIl2cppCache(); // Library/Bee, Library/BuildCache

// 3. 強制關鍵 PlayerSettings
PlayerSettings.WebGL.dataCaching = false;
PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Minimal);
PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;

// 4. Development Build（暴露完整 C# 堆疊）
options = BuildOptions.Development;
```
→ **一鍵產出可在 Chrome/Edge 正常執行的 WebGL build**，避免 `null function` / `INVALID_ENUM` / NRE 等常見崩潰。

### 4. 本機測試 Server (`webgl_server.py`)
```bash
python Tools/webgl_server.py 8120 D:/Project/SlotUnity/Build/WebGL2
```
- 正確送出 `Content-Encoding: gzip` + `Cache-Control: no-store`
- 避免瀏覽器快取舊 `.data/.wasm` 造成版本不一致

---

## 🚀 快速開始

### 前置需求
- Unity 2022.3.24f1 LTS（含 WebGL Build Support 模組）
- Python 3.8+（本機 server 用）

### 1. 開啟專案
```bash
# 用 Unity Hub 開啟 D:/Project/SlotUnity
```

### 2. 一鍵建構場景（Editor 內）
- 選單 `Tools → Slot → Build → Build Slot UI Scene`
- 或 `Tools → Slot → Build → Build WebGL`（直接輸出 WebGL）

### 3. 本機測試
```bash
cd D:/Project/SlotUnity
python Tools/webgl_server.py 8120 Build/WebGL2
```
用 **Chrome/Edge 無痕視窗** 開啟：`http://localhost:8120/`

> ⚠️ **請勿使用 Hermes/內嵌預覽視窗**（WebKit WebGL 不完整會崩潰），務必用真 Chrome/Edge。

---

## 🧪 測試

```bash
# Unity Editor 內
Window → General → Test Runner → Run All
```
- `PaylineEvaluatorTests`：28 測試全綠
- 涵蓋：Wild 替換、9/10 特例、多條線同時中、不中獎、邊界條件

---

## 📦 WebGL 部署

建置產物在 `Build/WebGL2/`：
```
Build/WebGL2/
├── index.html
└── Build/
    ├── WebGL2.data       # 未壓縮（dataCaching=false）
    ├── WebGL2.framework.js
    ├── WebGL2.loader.js
    └── WebGL2.wasm
```

部署時只需靜態托管（Nginx、GitHub Pages、Vercel、Netlify 等），確保：
- `.gz` 檔送 `Content-Encoding: gzip`
- 所有檔送 `Cache-Control: no-store`（或用 hash filename）

---

## 🐛 已解決的關鍵坑

| 現象 | 根因 | 解法 |
|------|------|------|
| `RuntimeError: null function` | Stripping Level = High 誤刪函式 | `Minimal` + `FullWithStacktrace` |
| `INVALID_ENUM: getInternalformatParameter` | 在 WebKit 內嵌視窗（Hermes）測試 | **改用真 Chrome/Edge** |
| `NullReferenceException: icons[]` | `private Image[]` 非序列化、scene 載入後遺失 | `Awake()` 依 child 名稱重建引用 |
| 舊 build 殘留導致版本不一致 | `loader.js` 時間戳 ≠ `wasm/framework` | 每次建置**清空輸出目錄**、全新產出 |

---

## 📝 開發規範

- **Assembly Definition** 嚴格分層：Core → Data → Game → Tests
- **Pure C# 核心邏輯**（PaylineEvaluator）零 Unity 依賴，易測試、易移植
- **Editor 工具** 隔離在 `Editor/` 資料夾，不打包進 Build
- **WebGL 設定** 集中在 `SlotWebGLBuilder.cs`，一致性由腳本保證

---

## 🔮 待辦 / 路線圖

- [ ] 錢包系統（餘額、扣注、贏分結算、持久化）
- [ ] Paytable 彈窗（使用現有 `PayTable1/2` 圖資）
- [ ] 規則說明彈窗 (`RuleLayer`)
- [ ] 音效整合（現有 `slot_BTN_voice01` 等）
- [ ] GitHub Actions CI：自動 Build WebGL → Deploy to GitHub Pages
- [ ] Production Build：關閉 Development、啟用 Brotli/Gzip、Code Stripping=High（經驗證後）

---

## 📄 授權

內部專案，專供教學/移植參考使用。

---

## 👤 作者

Yusuke Wu (@YusukeWu0903)