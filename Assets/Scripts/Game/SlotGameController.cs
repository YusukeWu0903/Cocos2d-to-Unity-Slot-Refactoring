using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slot.Core;

namespace Slot.Game
{
    /// <summary>
    /// Phase 2b：真實縱向捲動 + 中獎線高亮的水果機主控。
    /// </summary>
    public class SlotGameController : MonoBehaviour
    {
        [Header("Reels (5 columns)")]
        [SerializeField] private ReelColumn[] reels = new ReelColumn[5];

        [Header("UI refs")]
        [SerializeField] private Text betText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button spinButton;
        [SerializeField] private Button moneyUp, moneyDown, branchUp, branchDown;

        [Header("Highlight")]
        [SerializeField] private Image lineHighlight; // 單一高亮框，輪流跳到中獎格

        public int Money = 1;
        public int Branch = 1;

        [SerializeField] private SlotPresentation presentation;
        private bool configurationValid;
        private bool spinning;
        private System.Random rng = new System.Random();
        private Coroutine highlightRoutine;
        private Image spinImg;                    // SPIN 按鈕的 Image（換圖用）
        private Sprite spinNormalSprite;          // spin01：藍色 SPIN
        private Sprite spinStopSprite;            // spin02：粉紅色 STOP

        private void Start()
        {
            if (!ValidateReels(out var error)) { ReportConfigurationError(error); return; }
            for (int symbol = 1; symbol <= 10; symbol++)
                if (ReelCell.GetFruitSprite((SymbolId)symbol) == null)
                { ReportConfigurationError($"Missing image for symbol {symbol}."); return; }
            Money = Mathf.Clamp(Money, 1, 5);
            Branch = Mathf.Clamp(Branch, 1, 5);
            configurationValid = true;
            if (presentation != null) presentation.InspectionRequested += StopHighlight;
            if (spinButton != null)
            {
                spinButton.onClick.AddListener(OnSpinClicked);
                spinImg = spinButton.GetComponent<Image>();
                spinNormalSprite = Resources.Load<Sprite>("Sprites/ui/slot_BTN_unite_spin01");
                spinStopSprite = Resources.Load<Sprite>("Sprites/ui/slot_BTN_unite_spin02");
                if (spinImg != null && spinNormalSprite != null) spinImg.sprite = spinNormalSprite;
            }
            if (moneyUp != null) moneyUp.onClick.AddListener(() => ChangeBet(1, ref Money));
            if (moneyDown != null) moneyDown.onClick.AddListener(() => ChangeBet(-1, ref Money));
            if (branchUp != null) branchUp.onClick.AddListener(() => ChangeBet(1, ref Branch));
            if (branchDown != null) branchDown.onClick.AddListener(() => ChangeBet(-1, ref Branch));
            foreach (var r in reels) if (r != null) r.SetRandomAll(rng);
            UpdateBetText();
        }

        private void ChangeBet(int delta, ref int which)
        {
            if (spinning) return;
            which = Mathf.Clamp(which + delta, 1, 5);
            StopHighlight();
            UpdateBetText();
            if (presentation != null) presentation.SettingsChanged();
        }

        private void OnSpinClicked()
        {
            if (!configurationValid) return;
            if (!ValidateReels(out var error)) { ReportConfigurationError(error); return; }
            if (spinning)
            {
                // 滾動中再按 = STOP：全輪緩衝煞車、一起滑停（非暴力瞬停）
                foreach (var r in reels) if (r != null && !r.IsBraking) r.BeginBrake();
                return;
            }
            spinning = true;
            if (highlightRoutine != null) { StopCoroutine(highlightRoutine); highlightRoutine = null; }
            if (lineHighlight != null) lineHighlight.gameObject.SetActive(false);
            if (resultText) resultText.text = "";
            SetSpinButtonSprite(true);   // → 粉紅 STOP
            if (presentation != null) presentation.BeginSpin();
            UpdateControlState();
            StartCoroutine(SpinRoutine());
        }

        /// <summary>spinning=true 顯示 STOP（spin02），false 顯示 SPIN（spin01）。</summary>
        private void SetSpinButtonSprite(bool toStop)
        {
            if (spinImg == null) return;
            var sp = toStop ? spinStopSprite : spinNormalSprite;
            if (sp != null) spinImg.sprite = sp;
        }

        private IEnumerator SpinRoutine()
        {
            // 1) 全軸開始捲動（無預定結果；符號由捲動過程隨機決定，WYSIWYG）
            foreach (var r in reels) if (r != null) r.StartScroll();
            yield return new WaitForSeconds(0.9f);

            // 2) 由左至右逐軸緩衝煞車（階梯停輪；STOP 急停時全部已在煞車中，此處自動跳過）
            foreach (var r in reels)
            {
                if (r != null && !r.IsBraking) r.BeginBrake();
                yield return new WaitForSeconds(0.22f);
            }

            // 3) 等全部輪完全落定（煞車結束）
            while (true)
            {
                bool allIdle = true;
                foreach (var r in reels) if (r != null && !r.IsIdle) { allIdle = false; break; }
                if (allIdle) break;
                yield return null;
            }

            // 4) 從畫面「實際停著的符號」讀結果（WYSIWYG：看到什麼判什麼）
            if (!TryEvaluateSettledReels(out var wins, out var error))
            {
                ReportConfigurationError(error);
                yield break;
            }

            // 5) 判賞：只接受完整且已對齊的實際盤面。
            Debug.Log($"[Slot] spin done, {wins.Count} winning lines");

            if (wins.Count > 0)
            {
                var sb = new System.Text.StringBuilder($"命中 {wins.Count} 條線");
                foreach (var w in wins) sb.Append($"\nLine{w.LineId}: {w.Count}x {w.Symbol}");
                if (resultText) resultText.text = sb.ToString();

                // 6) 中獎線高亮：輪流框選每條中獎線的格子
                if ((lineHighlight != null || presentation != null) && reels[0] != null)
                    highlightRoutine = StartCoroutine(HighlightWins(wins));
            }
            else
            {
                if (resultText) resultText.text = "未中獎";
            }

            SetSpinButtonSprite(false);  // → 藍色 SPIN
            spinning = false;
            UpdateControlState();
            if (presentation != null) presentation.FinishSpin(wins);
        }

        private IEnumerator HighlightWins(IReadOnlyList<WinResult> wins)
        {
            if (presentation != null)
            {
                while (true)
                {
                    for (int i = 0; i < wins.Count; i++)
                    {
                        presentation.ShowWin(wins[i], i, wins.Count);
                        yield return new WaitForSeconds(1.8f);
                    }
                }
            }
            var rt = lineHighlight.rectTransform;
            var parent = rt.parent as RectTransform;
            while (true)
            {
                foreach (var w in wins)
                {
                    var line = SlotPaylines.All[w.LineId - 1];
                    for (int i = 0; i < w.Count; i++)
                    {
                        int cell = line.Cells[i];              // 1..20
                        int col = (cell - 1) / 4, row = (cell - 1) % 4;
                        // ✅ 高亮改用「確定格位座標」對位：欄 x + SlotY[row]，
                        //    不依賴停輪後 icon 陣列順序（icons 索引 ≠ 可視列）。
                        var colRt = (RectTransform)reels[col].transform;
                        Vector2 colPos = colRt.anchoredPosition;
                        rt.anchoredPosition = new Vector2(colPos.x, colPos.y + ReelColumn.SlotY[row]);
                        rt.sizeDelta = new Vector2(ReelColumn.CellW - 4f, ReelColumn.CellH - 4f);
                        lineHighlight.gameObject.SetActive(true);
                        yield return new WaitForSeconds(0.35f);
                    }
                }
            }
        }

        private bool ValidateReels(out string error)
        {
            error = null;
            if (reels == null || reels.Length != 5)
            { error = "Exactly five reels are required."; return false; }
            var seen = new HashSet<ReelColumn>();
            for (int col = 0; col < reels.Length; col++)
                if (reels[col] == null || !reels[col].IsConfigured || !seen.Add(reels[col]))
                { error = $"Reel {col + 1} is missing, incomplete or duplicated."; return false; }
            return true;
        }

        private bool TryEvaluateSettledReels(out IReadOnlyList<WinResult> wins, out string error)
        {
            wins = null;
            if (!ValidateReels(out error)) return false;
            try
            {
                var final = new SymbolId[21];
                for (int col = 0; col < 5; col++)
                {
                    var visible = reels[col].GetVisibleSymbols();
                    for (int row = 0; row < ReelColumn.Rows; row++) final[col * 4 + row + 1] = visible[row];
                }
                wins = PaylineEvaluator.EvaluateAll(final, Mathf.Clamp(Branch, 1, 5) * SlotPaylines.LinesPerLevel);
                return true;
            }
            catch (System.ArgumentException ex) { error = ex.Message; return false; }
            catch (System.InvalidOperationException ex) { error = ex.Message; return false; }
        }

        private void ReportConfigurationError(string error)
        {
            configurationValid = false;
            spinning = false;
            if (highlightRoutine != null) { StopCoroutine(highlightRoutine); highlightRoutine = null; }
            if (lineHighlight != null) lineHighlight.gameObject.SetActive(false);
            SetSpinButtonSprite(false);
            foreach (var button in new[] { spinButton, moneyUp, moneyDown, branchUp, branchDown })
                if (button != null) button.interactable = false;
            if (resultText != null) resultText.text = "Game unavailable. Please reload.";
            if (presentation != null) presentation.ShowError();
            Debug.LogError("[Slot] Configuration error: " + error);
        }

        private void UpdateBetText()
        {
            if (betText) betText.text = $"單位 {Money} · 啟用 {Branch * SlotPaylines.LinesPerLevel} 線";
            UpdateControlState();
            if (presentation != null) presentation.UpdateSettings(Money, Branch * SlotPaylines.LinesPerLevel);
        }

        private void StopHighlight()
        {
            if (highlightRoutine != null) { StopCoroutine(highlightRoutine); highlightRoutine = null; }
            if (lineHighlight != null) lineHighlight.gameObject.SetActive(false);
        }

        private void UpdateControlState()
        {
            if (moneyUp != null) moneyUp.interactable = configurationValid && !spinning && Money < 5;
            if (moneyDown != null) moneyDown.interactable = configurationValid && !spinning && Money > 1;
            if (branchUp != null) branchUp.interactable = configurationValid && !spinning && Branch < 5;
            if (branchDown != null) branchDown.interactable = configurationValid && !spinning && Branch > 1;
        }

        public void BindPresentation(SlotPresentation view) { presentation = view; }

        /// <summary>由 SlotSceneBuilder 注入。</summary>
        public void BindRefs(Text bet, Text result, Button spin,
            Button mUp, Button mDn, Button bUp, Button bDn,
            ReelColumn[] reelCols, Image highlight)
        {
            betText = bet; resultText = result; spinButton = spin;
            moneyUp = mUp; moneyDown = mDn; branchUp = bUp; branchDown = bDn;
            reels = reelCols; lineHighlight = highlight;
        }
    }
}
