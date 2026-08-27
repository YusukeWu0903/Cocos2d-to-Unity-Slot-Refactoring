using UnityEngine;
using UnityEngine.UI;
using Slot.Core;

namespace Slot.Game
{
    /// <summary>
    /// 單一轉輪縱欄：6 個 icon 在遮罩內向下捲動，循環換符號；停止時對齊 4 個可視格位。
    /// 對應原始 Lua 的縱向滾輪（LengthwaysSpeed 階梯停輪）。
    /// </summary>
    public class ReelColumn : MonoBehaviour
    {
        public const int Rows = 4;
        public const float CellW = 88f;
        public const float CellH = 92f;

        private Image[] icons;   // 6 個：y = 230,138,46,-46,-138,-230（1..4 可視）
        private bool scrolling;
        private float speed = 950f;
        private const float WrapY = -276f;
        private const float SpanY = 552f;
        private static readonly float[] SlotY = { 138f, 46f, -46f, -138f };

        /// <summary>
        /// Runtime 重建 icons 引用。editor 建構時由 Init() 填入，但 Image[] 非序列化欄位
        /// 在 scene 存檔後於 runtime 會還原為 null；child GameObject 已序列化保存，故在此依名稱重建。
        /// </summary>
        private void Awake()
        {
            if (icons != null && icons.Length == 6) return;
            var list = new System.Collections.Generic.List<Image>();
            for (int i = 0; i < 6; i++)
            {
                var t = transform.Find($"icon{i}");
                if (t != null) list.Add(t.GetComponent<Image>());
            }
            icons = list.ToArray();
        }

        public bool IsScrolling => scrolling;

        public void Init(Transform parent)
        {
            icons = new Image[6];
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"icon{i}", typeof(Image));
                go.transform.SetParent(parent, false);
                var img = go.GetComponent<Image>();
                img.rectTransform.sizeDelta = new Vector2(CellW - 6, CellH - 6);
                icons[i] = img;
            }
            SnapAll(new float[] { 230f, 138f, 46f, -46f, -138f, -230f });
        }

        public void SetSymbol(int row, SymbolId s)
        {
            // row 0..3 → 可視 icons[1..4]
            if (icons == null || icons.Length < row + 2) return;
            var sp = ReelCell.GetFruitSprite(s);
            if (sp != null && icons[row + 1] != null) icons[row + 1].sprite = sp;
        }

        public void SetRandomAll(System.Random rng)
        {
            for (int row = 0; row < 4; row++)
                SetSymbol(row, (SymbolId)rng.Next(1, 11));
        }

        public void StartScroll()
        {
            scrolling = true;
        }

        /// <summary>停止並對齊：visible 4 格貼上最終符號並吸附格位。</summary>
        public void StopAndSnap(SymbolId[] finalCol)
        {
            scrolling = false;
            // 依目前 y 排序，取在可視帶內的 4 個 icon
            var list = new System.Collections.Generic.List<Image>(icons);
            list.Sort((a, b) => b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));
            int placed = 0;
            foreach (var img in list)
            {
                float y = img.rectTransform.anchoredPosition.y;
                if (y > -184f && y < 184f && placed < 4)
                {
                    var sp = ReelCell.GetFruitSprite(finalCol[placed]);
                    if (sp != null) img.sprite = sp;
                    var rt = img.rectTransform;
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, SlotY[placed]);
                    placed++;
                }
            }
            // 其餘藏到上下外側
            foreach (var img in list)
            {
                float y = img.rectTransform.anchoredPosition.y;
                if (y > 184f) img.rectTransform.anchoredPosition = new Vector2(img.rectTransform.anchoredPosition.x, 276f);
                else if (y < -184f) img.rectTransform.anchoredPosition = new Vector2(img.rectTransform.anchoredPosition.x, -276f);
            }
            // 保險：若可視帶不足 4（浮點邊界），直接強制重排
            if (placed < 4) ForceSnap(finalCol);
        }

        private void ForceSnap(SymbolId[] finalCol)
        {
            SnapAll(new float[] { 276f, 138f, 46f, -46f, -138f, -276f });
            for (int row = 0; row < 4; row++) SetSymbol(row, finalCol[row]);
        }

        private void SnapAll(float[] ys)
        {
            for (int i = 0; i < icons.Length && i < ys.Length; i++)
                icons[i].rectTransform.anchoredPosition = new Vector2(0, ys[i]);
        }

        /// <summary>取得可視列 icon（供高亮/取位）。</summary>
        public Image GetVisibleIcon(int row) => icons[row + 1];

        private void Update()
        {
            if (!scrolling) return;
            float dy = speed * Time.deltaTime;
            foreach (var img in icons)
            {
                var rt = img.rectTransform;
                float y = rt.anchoredPosition.y - dy;
                if (y < WrapY)
                {
                    y += SpanY;
                    img.sprite = ReelCell.GetFruitSprite((SymbolId)Random.Range(1, 11));
                }
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            }
        }
    }
}
