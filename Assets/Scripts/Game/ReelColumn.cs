using UnityEngine;
using UnityEngine.UI;
using Slot.Core;

namespace Slot.Game
{
    /// <summary>
    /// 單一轉輪縱欄（WYSIWYG ＋ 緩衝煞車回彈）：
    /// - 常速向下捲動；icon 落出底部即迴帶到頂並換上「新的隨機符號」（遮罩外換圖）
    /// - 停輪：目標＝「最近的 92px 格點」（四捨五入）；
    ///   動畫＝ Carry（EaseOutQuad 減速、微微滑過頭）→ Settle（EaseOutCubic 回彈落定）
    /// - 若煞車時符號已略過格點（目標在上方），就做玩家期待的「回彈」：慣性再下滑一點後彈回格點
    /// - 落定只對齊位置，絕不改符號；判賞由 controller 讀 GetVisibleSymbols()（畫面事實）
    /// </summary>
    public class ReelColumn : MonoBehaviour
    {
        public const int Rows = 4;
        public const float CellW = 88f;
        public const float CellH = 92f;

        private enum ReelState { Idle, Scrolling, Braking }
        private enum BrakePhase { Carry, Settle }

        private Image[] icons;      // 6 個 icon（順序會因 wrap 改變；位置即真相）
        private SymbolId[] ids;     // 與 icons 平行；每個 icon 目前顯示的符號
        private ReelState state = ReelState.Idle;
        private float speed = 950f;
        private const float WrapY = -276f;
        private const float SpanY = 552f;
        public static readonly float[] SlotY = { 138f, 46f, -46f, -138f };
        private static readonly float[] HomeY = { 230f, 138f, 46f, -46f, -138f, -230f };
        private const float GridTop = 138f;   // 最高可視格 y；格點 = 138 - 92k

        [Header("Brake (緩衝煞車＋回彈)")]
        [SerializeField] private float carryDuration = 0.26f;  // 減速滑行時長（秒）
        [SerializeField] private float settleDuration = 0.20f; // 回彈落定時長（秒）
        [SerializeField] private float overshoot = 30f;        // 滑過格點的幅度（px）→ 回彈量
        [SerializeField] private float minCarry = 12f;         // 距格點低於此值 → 純回彈模式

        private float brakeT;
        private BrakePhase brakePhase;
        private float[] phaseFromY;
        private float[] phaseToY;
        private float[] finalY;

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
            ids = new SymbolId[icons.Length];
        }

        public bool IsConfigured
        {
            get
            {
                if (icons == null || ids == null || icons.Length != 6 || ids.Length != 6) return false;
                foreach (var icon in icons) if (icon == null) return false;
                return true;
            }
        }

        public bool IsScrolling => state != ReelState.Idle;
        public bool IsBraking => state == ReelState.Braking;
        public bool IsIdle => state == ReelState.Idle;

        public void Init(Transform parent)
        {
            icons = new Image[6];
            ids = new SymbolId[6];
            for (int i = 0; i < 6; i++)
            {
                var go = new GameObject($"icon{i}", typeof(Image));
                go.transform.SetParent(parent, false);
                var img = go.GetComponent<Image>();
                img.rectTransform.sizeDelta = new Vector2(CellW - 6, CellH - 6);
                icons[i] = img;
            }
            SnapAll(HomeY);
        }

        /// <summary>
        /// 取得可視 4 格「實際顯示」的符號（row 0=最上）。
        /// 落定後 icon 必在格點上，故以「離 SlotY[row] 最近的 icon」為準（不依賴陣列順序）。
        /// </summary>
        public SymbolId[] GetVisibleSymbols()
        {
            if (!IsConfigured) throw new System.InvalidOperationException("Reel is missing one or more icons.");
            var used = new bool[icons.Length];
            var r = new SymbolId[Rows];
            for (int row = 0; row < Rows; row++)
            {
                int best = -1;
                float bd = float.MaxValue;
                for (int i = 0; i < icons.Length; i++)
                {
                    float d = Mathf.Abs(icons[i].rectTransform.anchoredPosition.y - SlotY[row]);
                    if (d < bd) { bd = d; best = i; }
                }
                if (best < 0 || bd > 0.1f || used[best] || ids[best] < SymbolId.S1 || ids[best] > SymbolId.S10)
                    throw new System.InvalidOperationException("Reel has no unique settled symbol at every visible cell.");
                used[best] = true;
                r[row] = ids[best];
            }
            return r;
        }

        public void SetRandomAll(System.Random rng)
        {
            for (int i = 0; i < icons.Length; i++)
                AssignSymbol(i, (SymbolId)rng.Next(1, 11));
        }

        public void StartScroll()
        {
            state = ReelState.Scrolling;
        }

        /// <summary>
        /// 緩衝煞車：目標＝最近格點（四捨五入）。若已略過格點則回彈向上修正。
        /// 只決定「停在哪」，不決定「停什麼符號」（WYSIWYG）。
        /// </summary>
        public void BeginBrake()
        {
            if (state != ReelState.Scrolling) return; // Idle 不處理；Braking 中忽略重複

            phaseFromY = new float[icons.Length];
            phaseToY = new float[icons.Length];
            finalY = new float[icons.Length];

            for (int i = 0; i < icons.Length; i++)
            {
                float y = icons[i].rectTransform.anchoredPosition.y;
                // ✅ 最近格點（Round，不再強制多滑 60px）→ 需要時自然產生「向上回彈」
                float k = Mathf.Round((GridTop - y) / CellH);
                float target = GridTop - k * CellH;
                // 已正好踩在格點上（距離≈0）：再多滑一格，避免看起來像瞬停
                if (Mathf.Abs(y - target) < 2f) { k += 1f; target = GridTop - k * CellH; }
                finalY[i] = target;
            }

            StartCarryPhase();
            state = ReelState.Braking;
        }

        /// <summary>Phase A：減速滑行（EaseOutQuad）。目標在下方 → 滑到「格點再下方 overshoot 處」；目標在上方 → 慣性再下滑一點，交給 Settle 回彈。</summary>
        private void StartCarryPhase()
        {
            brakePhase = BrakePhase.Carry;
            brakeT = 0f;
            for (int i = 0; i < icons.Length; i++)
            {
                float y = icons[i].rectTransform.anchoredPosition.y;
                float d = y - finalY[i];   // >0：目標在下方；<0：目標在上方（需回彈）
                phaseFromY[i] = y;
                if (d >= minCarry)
                {
                    float ov = Mathf.Min(overshoot, Mathf.Max(6f, d - 8f));
                    phaseToY[i] = finalY[i] - ov;      // 微微滑過頭
                }
                else
                {
                    phaseToY[i] = y - 18f;             // 慣性再下滑一點
                }
            }
        }

        /// <summary>Phase B：回彈落定（EaseOutCubic）→ 精確吸附最終格點。</summary>
        private void StartSettlePhase()
        {
            brakePhase = BrakePhase.Settle;
            brakeT = 0f;
            for (int i = 0; i < icons.Length; i++)
            {
                phaseFromY[i] = icons[i].rectTransform.anchoredPosition.y;
                phaseToY[i] = finalY[i];
            }
        }

        /// <summary>取得可視列 icon（供取位）。</summary>
        public Image GetVisibleIcon(int row)
        {
            if (row < 0 || row >= Rows) throw new System.ArgumentOutOfRangeException(nameof(row));
            if (!IsConfigured) throw new System.InvalidOperationException("Reel icons are incomplete.");
            Image nearest = null;
            float distance = float.MaxValue;
            foreach (var icon in icons)
            {
                float current = Mathf.Abs(icon.rectTransform.anchoredPosition.y - SlotY[row]);
                if (current < distance) { distance = current; nearest = icon; }
            }
            if (distance > .1f) throw new System.InvalidOperationException("Reel is not settled on its grid.");
            return nearest;
        }

        private void AssignSymbol(int i, SymbolId s)
        {
            ids[i] = s;
            var sp = ReelCell.GetFruitSprite(s);
            if (sp != null && icons[i] != null) icons[i].sprite = sp;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (state == ReelState.Scrolling)
            {
                float dy = speed * dt;
                for (int i = 0; i < icons.Length; i++)
                {
                    var rt = icons[i].rectTransform;
                    float y = rt.anchoredPosition.y - dy;
                    if (y < WrapY)
                    {
                        y += SpanY;
                        // 迴帶（在遮罩外）＝換上新的隨機符號（原始 Lua 行為）
                        AssignSymbol(i, (SymbolId)Random.Range(1, 11));
                    }
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
                }
            }
            else if (state == ReelState.Braking)
            {
                brakeT += dt;
                if (brakePhase == BrakePhase.Carry)
                {
                    float t = Mathf.Clamp01(brakeT / carryDuration);
                    float e = 1f - (1f - t) * (1f - t);   // EaseOutQuad：快→慢減速
                    LerpY(e);
                    if (t >= 1f) StartSettlePhase();
                }
                else
                {
                    float t = Mathf.Clamp01(brakeT / settleDuration);
                    float e = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic：回彈滑入格點
                    LerpY(e);
                    if (t >= 1f)
                    {
                        // 精確吸附最終格點（只動位置、不換符號；不做整列 HomeY 瞬移）
                        for (int i = 0; i < icons.Length; i++)
                        {
                            var rt = icons[i].rectTransform;
                            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, finalY[i]);
                        }
                        state = ReelState.Idle;
                    }
                }
            }
        }

        private void LerpY(float e)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                var rt = icons[i].rectTransform;
                float y = Mathf.Lerp(phaseFromY[i], phaseToY[i], e);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            }
        }

        private void SnapAll(float[] ys)
        {
            for (int i = 0; i < icons.Length && i < ys.Length; i++)
                icons[i].rectTransform.anchoredPosition = new Vector2(0, ys[i]);
        }
    }
}