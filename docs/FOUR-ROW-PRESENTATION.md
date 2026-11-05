# 四排展示版規格與驗收

本輪沿用獨立分支 codex/unity6-upgrade，原專案 D:/Project/SlotUnity 不修改。回復點 bd9c4f6、既有三份瀏覽器比較成品保留。

## 使用者確認

- 鈴鐺 S8 為 Wild；不替代 GOOD LUCK S9 或 BAR S10。一般圖案可被替代、全鈴鐺可連線；GOOD LUCK / BAR 自己連續三個以上仍可中獎。
- 完整顯示四排都參與判賞；固定40條線，含四條橫線與折線。
- 線數五段8/16/24/32/40；每一段包含四條橫線，從最左軸開始，至少連續三個相符圖案。
- 先整理操作、規則與結果提示；未引入金幣帳本、賠率或RTP設計。

## 路徑來源與編號

保留原30條實際路徑。線1/2/3仍是中間／第三／頂排橫線；新增線4為底排橫線。原線4..30依序移為5..31，並補入9條經過第四排的上下鏡像折線為32..40。這是新展示規則，不宣稱原Lua等價。整個四排覆蓋不代表每排的折線數完全相等。

實際盤面疊線與規則面板小圖使用同一份SlotPaylines及PaylineGraphic，避免兩份定義偏離。灰色線未啟用，不參與本輪判賞。

## 安全防護

- 空盤、None、越界符號、短陣列、錯誤支付線或錯誤Wild模式明確拒絕；不把缺失資料當Wild中獎。
- 五軸引用及六個圖案節點必須完整且不重複；資源缺漏停用操作並提示重新載入。
- 判賞只讀取實際停穩且對齊的圖案；不得以附近重複圖案填補空格。
- SPIN期間鎖定設定；STOP可重複請求，完成後可再轉。

## 展示

獨立Assets/Scenes/SlotPresentation.unity；原SlotScene.unity保留。中文標籤、單位與線數分開顯示；完整四排不裁切SPIN。規則面板含40條路徑與鈴鐺／GOOD LUCK／BAR圖片及正反例。中獎顯示線號、圖案、連數並輪流畫線、外框，不遮住水果。未中獎、轉動、缺漏資料均有明確狀態。

以Google Fonts官方Noto Sans TC（OFL1.1）產生146,284的500字重子集，改名SlotSansTC，附OFL及來源。原始字型及工具存D槽任務資料夾；新增中文需重建子集。

WebGL模板依Unity實際回報更新進度，createUnityInstance完成後關閉載入提示；失敗保留重試說明。固定內部1024×720、外部等比縮放，手機橫向較適合閱讀；尚未視為真機驗證。

## 可觀察驗收

頂排／第二／第三／底排橫線在最低8線均能五連；線40底排折線在32線不判、40線可判；Wild正例與GOOD LUCK/BAR反例；全Wild；未中獎；線數最小／中間／最大；連點STOP、下一輪；中文字元與路徑選擇。

機器測試與本機瀏覽器核對不等於使用者美術接受；不宣稱獨立Chrome/Edge及實體手機通過。實際測試、建置、瀏覽器結果於完成時追加。
## 本輪完成結果

- EditMode 55/55、PlayMode 7/7（合計62）通過。PlayMode含實際展示場景的中文、8→40線、規則開關、選線40、STOP連點及下一輪；30輪轉動另外核對可視圖片與判賞符號一致。文字最後只縮短換行並清除轉動中的舊線號，未再改判賞。
- 最終成品為unity6-fourrow-v3，Unity 6000.5.10f1非開發版gzip，建置Succeeded、0 errors；1項既有空Slot.Data assembly警告。核心4檔14,001,338 bytes，全部檔案14,004,552 bytes。HEAD四檔200、gzip及Wasm MIME符合預期。v1/v2與舊三版保留。
- 最終字型146,284 bytes，介面字串缺字檢查0。瀏覽器確認四排完整、SPIN完整、中文清楚，8線預設、24線中間設定、40線上限、第四排橫線預覽、轉動鎖定、提早STOP、未中獎與實際中獎連線均已觀察。單位3在轉動中點加號仍為3。
- 最後一輪瀏覽器驗證自2026-11-05T04:58:04.416Z起，沒有新增warn/error。此tab舊頁切換時曾在04:45出現1筆MutationObserver錯誤，來源未確認；本次成品HTML/loader/framework無該名稱，沒有把整個tab歷史描述成完全無錯誤。完整紀錄fourrow-browser-review.json保留。
- 最終實際截圖fourrow-v3-preview.jpg、fourrow-v3-rules.jpg、fourrow-v3-win.jpg位於D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/。fourrow-final-verification.json、fourrow-path-audit.json、兩類XML與fourrow-build-v3.log可供追溯。
- 比較頁http://127.0.0.1:8121/預設新版，單獨入口http://127.0.0.1:8121/unity6-fourrow-v3/。伺服器維持本機127.0.0.1。未發布作品集、未push；獨立Chrome/Edge與實體手機尚待驗證，使用者外觀接受亦待回覆。
