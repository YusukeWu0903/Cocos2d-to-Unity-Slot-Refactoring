# 獨立 WebGL 展示部署

WebGLShowcase/包含已驗收retro-v4的純靜態成品及vercel.json，遊戲程式來源為31b74d7。逐檔SHA256見webgl-release.json。此資料夾為Vercel專案根目錄，Framework Other／無建置與安裝步驟，Output Directory為`.`。

部署來源分支codex/unity6-upgrade；原main維持Unity2022比較版。Vercel使用既有GitHub連線取此分支的明確提交；不在雲端執行Unity或要求Unity授權金鑰。

未來更新先在本機Unity建置並驗收新WebGL成品，再同步此資料夾、重新設定hash檔案對應gzip／Wasm MIME與快取標頭、更新manifest並提交，最後部署該提交。保留舊Vercel部署可供回復。不要把Library、Logs、Unity快取或Vercel登入狀態放進此資料夾。

公開網址驗證需包含：gzip／Wasm MIME、載入提示關閉、START／STOP及結果、作品集入口。正式網址：https://unity-fruit-slot.vercel.app/ 。Vercel專案unity-fruit-slot；首個正式部署dpl_8djD8czp7rWbeWQ4Cg2B8ps9Mn1d，Git來源10e540f429378f43bfc1c2513725360e8ba53faf（遊戲來源31b74d7）。

2026-11-05已驗證五個公開檔案雜湊與v4一致、gzip／Wasm MIME正確、載入提示關閉、自動停輪及連點STOP返回START。作品集正式版aea1393已上架至第一街區原倒水遊戲地塊，倒水移至第二街區。公開入口完整鏈：作品集卡片→/open/fruit-slot→此公開網址。實體手機尚未驗收；作品集已做390px寬度操作檢查。
