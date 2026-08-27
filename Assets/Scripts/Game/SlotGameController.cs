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

        private bool spinning;
        private System.Random rng = new System.Random();
        private Coroutine highlightRoutine;
        private Image spinImg;                    // SPIN 按鈕的 Image（換圖用）
        private Sprite spinNormalSprite;          // spin01：藍色 SPIN
        private Sprite spinStopSprite;            // spin02：粉紅色 STOP

        private void Start()
        {
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
            UpdateBetText();
        }

        private void OnSpinClicked()
        {
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
            var final = new SymbolId[21];
            for (int col = 0; col < 5; col++)
            {
                if (reels[col] == null) continue;
                var vis = reels[col].GetVisibleSymbols();
                for (int row = 0; row < 4; row++) final[col * 4 + row + 1] = vis[row];
            }

            // 5) 判賞
            var wins = PaylineEvaluator.EvaluateAll(final, Branch * 6);
            Debug.Log($"[Slot] spin done, {wins.Count} winning lines");

            if (wins.Count > 0)
            {
                var sb = new System.Text.StringBuilder("贏得金幣!");
                foreach (var w in wins) sb.Append($"\nLine{w.LineId}: {w.Count}x {w.Symbol}");
                if (resultText) resultText.text = sb.ToString();

                // 6) 中獎線高亮：輪流框選每條中獎線的格子
                if (lineHighlight != null && reels[0] != null)
                    highlightRoutine = StartCoroutine(HighlightWins(wins));
            }
            else
            {
                if (resultText) resultText.text = "未中獎";
            }

            SetSpinButtonSprite(false);  // → 藍色 SPIN
            spinning = false;
        }

        private IEnumerator HighlightWins(IReadOnlyList<WinResult> wins)
        {
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

        private void UpdateBetText()
        {
            if (betText) betText.text = $"{Money} x {Branch} = {Money * Branch}";
        }

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
