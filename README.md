# Cocos2d-to-Unity Slot Refactoring

Cocos2d-x Quick Lua 水果機的 Unity 重構原型。此獨立分支使用 Unity 6000.5.10f1，新增可試玩的四排展示場景；原 Unity 2022 專案與既有成品保留。

## 復古機台版

新增獨立 `Assets/Scenes/SlotCabinet.unity`：紫金機框、固定彩色燈泡、白色四排捲軸、兩側線號、真實回合資訊窗與大型 START／STOP 按鈕。現有水果與40線規則沿用，原功能版場景保留。

[外觀、操作與驗收記錄](docs/RETRO-CABINET.md)。[停輪疊格／缺格修正](docs/BRAKE-GRID-FIX.md)：整軸共用格點與位移，12項PlayMode通過，含100輪實際機台場景回歸。本機試玩 http://127.0.0.1:8121/unity6-retro-v4/ ，比較頁 http://127.0.0.1:8121/ 可切回四排功能版及早期成品。尚未發布作品集。

重建機台場景：**Tools → Slot → Build Retro Fruit Cabinet**。下方建置命令的 `-slotScene presentation` 改為 `-slotScene cabinet` 即輸出機台版；`-slotReuseScene true` 可沿用已儲存場景。機台獨立模板為 `Assets/WebGLTemplates/RetroCabinet/`，內部畫布1024×800。

[支付線可讀性調整與後續建議](docs/LINE-VISIBILITY.md)：盤面線5px、命中框4px，預覽與中獎線加深色細描邊。

## 四排規則與展示

- 五軸、完整四排、40 條固定支付線；8／16／24／32／40 五檔，每檔都包含四排橫線。
- 從最左軸沿啟用線連續三個以上相符圖案即中獎。
- 鈴鐺是 Wild，可替代一般圖案，但不替代 GOOD LUCK 或 BAR。兩者自己連續三個仍可中獎；全鈴鐺也可連線。
- 中文控制、規則與40條線縮圖；可選線在盤面上查看，中獎逐線畫路徑與外框。
- SPIN／STOP、緩衝停輪；設定於轉動期間鎖定。無效盤面、缺漏節點及錯誤圖案不會冒充中獎。
- 單位控制是原型介面，沒有點數帳本、賠率表或金幣結算。

規格、路徑編號變更與驗收：[四排展示說明](docs/FOUR-ROW-PRESENTATION.md)。初次升級比較的歷史紀錄：[Unity 6 試升級](docs/UNITY6-UPGRADE.md)。新40線規則不宣稱與原Lua完全等價。

## 開啟與建置

1. Unity Hub 開啟本副本，使用 Unity 6000.5.10f1（含 WebGL Build Support）。
2. 展示場景為 `Assets/Scenes/SlotPresentation.unity`。原 `SlotScene.unity` 保留。
3. 重建展示場景：選單 **Tools → Slot → Build Four Row Presentation**。會重新產生展示場景，不保留其手動排版修改。
4. 獨立建置命令：

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.5.10f1/Editor/Unity.exe' -batchmode -nographics -quit `
  -projectPath 'D:/Project/SlotUnity-unity6' -buildTarget WebGL `
  -executeMethod Slot.EditorTools.UpgradeBuild.Build -slotProfile release -slotScene presentation `
  -slotOutput 'D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/<新的資料夾名稱>' `
  -logFile 'D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/<新的日誌名稱>.log'
```

在 PowerShell 自動化中請使用 `Start-Process -WindowStyle Hidden -PassThru` 並等待 ExitCode；GUI exe 的呼叫返回不等於建置完成。輸出必須是不存在的新資料夾。建置器保留既有輸出及快取；非開發版使用 gzip，伺服器需正確傳送 Content-Encoding 及 Wasm MIME。

若只修改執行期程式而沒有改排版，可加 `-slotReuseScene true` 沿用已儲存的展示場景，避免重建場景；未提供此參數時維持原有重建流程。

本機比較伺服器：`python D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/review_server.py`，開啟 http://127.0.0.1:8121/ 。保留舊版切換，一次只載入一個遊戲。

## 程式位置

- `Assets/Scripts/Core/Payline.cs`：40條線，原30條路徑全部保留。
- `Assets/Scripts/Core/PaylineEvaluator.cs`：純C#判賞與輸入驗證。
- `Assets/Scripts/Game/ReelColumn.cs`：六個節點循環、緩停、實際可視圖案讀取。
- `Assets/Scripts/Game/SlotGameController.cs`：回合、設定鎖定、轉輪驗證與判賞。
- `Assets/Scripts/Game/SlotPresentation.cs`：說明、規則、選線與結果狀態。
- `Assets/Scripts/Game/PaylineGraphic.cs`：實際盤面和規則縮圖共用路徑繪製。
- `Assets/Scripts/Game/Editor/PresentationSceneBuilder.cs`：可重建的獨立展示場景。
- `Assets/WebGLTemplates/Portfolio/`：等比畫布及實際載入進度；啟動完成關閉提示。
- `Assets/Tests/EditMode/`、`Assets/Tests/PlayMode/`：判賞、資料防護、轉輪與實際場景操作驗證。

## 中文字型

`Assets/Resources/Fonts/SlotSansTC.ttf` 是 [Google Fonts Noto Sans TC](https://github.com/google/fonts/tree/main/ofl/notosanstc) 的500字重精簡衍生字型，依 OFL 1.1 重新命名，隨附完整 OFL 及來源。新增中文後，先下載官方原始可變TTF，再用安裝有 fonttools 的 Python 執行：

```powershell
python Tools/subset_ui_font.py '<原始NotoSansTC字型.ttf>' --report '<D槽輸出報告.json>'
```

工具從程式碼收集字元並檢查介面字串缺字；新增文字後需重新產生場景與WebGL成品。原始字型與本輪工具依賴保存在專案外的D槽比較資料夾。

## 驗證範圍

本輪測試與實際瀏覽器結果記錄於 `docs/RETRO-CABINET.md`、`docs/FOUR-ROW-PRESENTATION.md` 及D槽比較資料夾。內嵌瀏覽器實測、Editor測試與手機尺寸預覽，均不能代替獨立Chrome／Edge及實體手機驗證。手機建議橫向觀看，正式採用前仍需真機操作確認。
