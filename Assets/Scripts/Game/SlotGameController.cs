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

        private void Start()
        {
            if (spinButton != null) spinButton.onClick.AddListener(OnSpinClicked);
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
            if (spinning) return;
            spinning = true;
            if (highlightRoutine != null) { StopCoroutine(highlightRoutine); highlightRoutine = null; }
            if (lineHighlight != null) lineHighlight.gameObject.SetActive(false);
            if (resultText) resultText.text = "";
            StartCoroutine(SpinRoutine());
        }

        private IEnumerator SpinRoutine()
        {
            // 1) 決定最終 20 格結果（cells[1..20]，col-major：col*4+row+1）
            var final = new SymbolId[21];
            for (int i = 1; i <= 20; i++) final[i] = (SymbolId)rng.Next(1, 11);

            // 2) 全軸開始捲動
            foreach (var r in reels) if (r != null) r.StartScroll();
            yield return new WaitForSeconds(0.9f);

            // 3) 由左至右逐軸停（階梯停輪，對應原始 LengthwaysSpeed）
            for (int col = 0; col < 5; col++)
            {
                var colSymbols = new SymbolId[4];
                for (int row = 0; row < 4; row++) colSymbols[row] = final[col * 4 + row + 1];
                if (reels[col] != null) reels[col].StopAndSnap(colSymbols);
                yield return new WaitForSeconds(0.22f);
            }

            // 4) 判賞
            var wins = PaylineEvaluator.EvaluateAll(final, Branch * 6);
            Debug.Log($"[Slot] spin done, {wins.Count} winning lines");

            if (wins.Count > 0)
            {
                var sb = new System.Text.StringBuilder("贏得金幣!");
                foreach (var w in wins) sb.Append($"\nLine{w.LineId}: {w.Count}x {w.Symbol}");
                if (resultText) resultText.text = sb.ToString();

                // 5) 中獎線高亮：輪流框選每條中獎線的格子
                if (lineHighlight != null && reels[0] != null)
                    highlightRoutine = StartCoroutine(HighlightWins(wins));
            }
            else
            {
                if (resultText) resultText.text = "未中獎";
            }

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
                        var icon = reels[col].GetVisibleIcon(row);
                        rt.sizeDelta = icon.rectTransform.sizeDelta + new Vector2(10, 10);
                        // icon 在欄 local，換算到高亮 parent（同一 reel area）座標
                        var colRt = icon.transform.parent as RectTransform;
                        Vector2 localPos = icon.rectTransform.anchoredPosition;
                        Vector2 colPos = colRt != null ? colRt.anchoredPosition : Vector2.zero;
                        rt.anchoredPosition = colPos + localPos;
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
