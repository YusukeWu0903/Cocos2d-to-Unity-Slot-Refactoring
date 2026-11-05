# 停輪疊格／缺格修正（2026-11-05）

## 現場與重現

使用者在復古機台v2連續操作後發現停輪不再返回START，第一軸第二排疊了兩張圖、第三排空缺。實際瀏覽器亦看到同一盤面、STATUS為ERROR，而按鈕仍停留STOP。不是單純缺少水果資產；相鄰icon停在同一格，完整盤面驗證拒絕判賞。

舊版每個icon各自Mathf.Round計算最近格點。半格位置的midpoint-to-even取整會讓相鄰圖案向不同方向取整，造成兩個icon重疊，另一格空缺；每圖獨立累積浮點位移及各自判斷2px邊界也可能產生分歧。新增實際ReelColumn動畫回歸，在offset=46的半格案例穩定重現GetVisibleSymbols的「no unique settled symbol」例外。修正前測試1/1失敗，XML及日誌保留。

## 修正

- ReelColumn：每幀以同一軸相位計算全部icon位置；共用一個最近格點決定整軸停輪位移、慣性滑行及回彈。所有相鄰圖案維持92px間距。
- 停穩後只將遮罩外的緩衝圖案環繞回六格範圍，保留sprite與symbol，不用隨機補圖掩蓋缺格。
- SlotPresentation：發生錯誤時清除舊路徑，按鈕顯示「暫停 ERROR」，不再偽裝成仍在轉動的STOP。原盤面驗證保留，不接受不完整盤面判賞。
- 規則、40條支付線、現有水果、機框與操作位置均沿用。

## 驗證

PlayMode12/12通過，其中包括：

- 半格正負方向、半格兩側、0／正負2px附近、接近整格與環繞，共16種停輪位置；每次檢查四格各只有一張圖、六張圖等距、sprite與判賞符號一致、煞車不換圖。
- 實際SlotCabinet場景100輪混合自動停輪、不同時間STOP、重複STOP。每輪檢查返回START、WIN或NO WIN、五軸均Idle、四格圖片與符號一致。此為加速時鐘的Unity回歸，不冒稱100輪瀏覽器手動操作或效能基準。
- 原有30輪、無效資料防護、中文、藍線重設、機台控制及新增錯誤提示驗證。

修正前3f43c52來源及v2成品保留；新成品為unity6-retro-v3。非開發gzip建置Succeeded、0 errors、1既有Slot.Data空assembly警告；核心四檔14,017,305 bytes、全部14,020,521 bytes。四檔HTTP200、gzip及Wasm MIME均已核對。比較頁預設與README入口更新至v3，v2歷史成品未覆寫。

## 證據位置

D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/：brake-fix-before/、brake-fault-before.jpg、brake-before-tests.xml／log、brake-after-tests.xml／log、brake-build-v3.log。未push或發布作品集；使用者外觀接受與實體手機驗收不由測試取代。

## WebGL實際操作

修正後在內嵌瀏覽器連續三輪：連點STOP、自動階梯停輪、40線手動STOP，皆返回START、五軸四排完整；觀察未中獎與金色中獎連線。當次頁面無新增warn/error。實際截圖brake-fixed-v3.jpg、瀏覽器紀錄brake-browser-review.json及完整brake-verification.json保存在上述D槽資料夾。這三輪網頁操作與100輪Unity場景測試分別記錄，不宣稱100輪WebGL手動驗收。
