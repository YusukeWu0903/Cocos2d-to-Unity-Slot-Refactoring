using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Slot.Game;

namespace Slot.EditorTools
{
    /// <summary>
    /// Phase 2b：程式化建立水果機 UI 場景（真實縱向捲動 + 高亮）。
    /// Tools/Slot/Phase2/Build Slot UI Scene
    /// </summary>
    public static class SlotUiSceneBuilder
    {
        private static Sprite Ui(string name) => Resources.Load<Sprite>($"Sprites/ui/slot_{name}");

        [MenuItem("Tools/Slot/Phase2/Build Slot UI Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- Camera ----
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.10f);
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 0, -10);

            // ---- Canvas ----
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 640);
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // ---- EventSystem ----
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            // ---- Root ----
            var rootGo = new GameObject("SlotRoot");
            var ctrl = rootGo.AddComponent<SlotGameController>();

            // ---- 背景 ----
            var bg = NewImage(canvasGo.transform, "BG", new Color(0.16f, 0.20f, 0.26f));
            Stretch(bg.rectTransform);

            // ---- 轉輪區（含遮罩）----
            var reelArea = NewPanel(canvasGo.transform, "ReelArea",
                new Vector2(0, 60), new Vector2(560, 420));
            reelArea.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.16f, 1f);

            var maskGo = new GameObject("Mask", typeof(RectMask2D));
            maskGo.transform.SetParent(reelArea.transform, false);
            var maskRt = (RectTransform)maskGo.transform;
            StretchRt(maskRt, 6f);

            var reels = new ReelColumn[5];
            for (int col = 0; col < 5; col++)
            {
                var colGo = new GameObject($"Reel_{col}", typeof(Image));
                colGo.transform.SetParent(maskGo.transform, false);
                var cimg = colGo.GetComponent<Image>();
                cimg.color = new Color(1, 1, 1, 0.02f);
                var crt = (RectTransform)colGo.transform;
                crt.sizeDelta = new Vector2(104, 400);
                crt.anchoredPosition = new Vector2(-208 + col * 104, 0);

                var rc = colGo.AddComponent<ReelColumn>();
                rc.Init(colGo.transform);
                reels[col] = rc;
            }

            // ---- 高亮框（放在 reelArea 上層）----
            var hlGo = new GameObject("LineHighlight", typeof(Image));
            hlGo.transform.SetParent(reelArea.transform, false);
            var hl = hlGo.GetComponent<Image>();
            hl.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            hl.raycastTarget = false;
            var hlRt = (RectTransform)hlGo.transform;
            hlRt.sizeDelta = new Vector2(96, 92);
            hlGo.SetActive(false);

            // ---- 底部控制列 ----
            var bar = NewPanel(canvasGo.transform, "ControlBar",
                new Vector2(0, -250), new Vector2(940, 120));
            bar.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.20f, 1f);

            var betTxt = NewText(bar.transform, "BetText", "1 x 1 = 1",
                new Vector2(-330, 0), new Vector2(220, 60), 34);

            var mUpB = NewBtn(bar.transform, "MoneyUp", "+", new Vector2(-170, 30));
            var mDnB = NewBtn(bar.transform, "MoneyDown", "-", new Vector2(-170, -30));
            NewLabel(bar.transform, "MoneyLabel", "押注", new Vector2(-240, 0));
            var bUpB = NewBtn(bar.transform, "BranchUp", "+", new Vector2(-70, 30));
            var bDnB = NewBtn(bar.transform, "BranchDown", "-", new Vector2(-70, -30));
            NewLabel(bar.transform, "BranchLabel", "線數", new Vector2(-140, 0));

            // ---- SPIN 按鈕（原始圖）----
            var spinGo = new GameObject("SpinButton", typeof(Image), typeof(Button));
            spinGo.transform.SetParent(bar.transform, false);
            var sImg = spinGo.GetComponent<Image>();
            var spinSprite = Ui("BTN_unite_spin01");
            if (spinSprite != null)
            {
                sImg.sprite = spinSprite;
                sImg.SetNativeSize();
            }
            else
            {
                sImg.color = new Color(0.9f, 0.3f, 0.3f);
                ((RectTransform)spinGo.transform).sizeDelta = new Vector2(160, 100);
            }
            spinGo.GetComponent<Button>().targetGraphic = sImg;
            ((RectTransform)spinGo.transform).anchoredPosition = new Vector2(280, 0);

            // ---- 結果文字 ----
            var resTxt = NewText(canvasGo.transform, "ResultText", "",
                new Vector2(0, -170), new Vector2(700, 110), 26);
            resTxt.color = new Color(1f, 0.85f, 0.3f);

            ctrl.BindRefs(betTxt, resTxt, spinGo.GetComponent<Button>(),
                mUpB, mDnB, bUpB, bDnB, reels, hl);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SlotScene.unity");
            Debug.Log("[SlotUiSceneBuilder] UI scene saved (2b).");
        }

        // ---------- helpers ----------
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void StretchRt(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        private static Image NewImage(Transform parent, string name, Color c)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = c;
            return img;
        }

        private static Image NewPanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var img = NewImage(parent, name, Color.white);
            var rt = (RectTransform)img.transform;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            return img;
        }

        private static Button NewBtn(Transform parent, string name, string label, Vector2 pos)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.25f, 0.55f, 0.75f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(64, 52);
            rt.anchoredPosition = pos;
            NewText(go.transform, "label", label, Vector2.zero, new Vector2(64, 52), 30);
            return btn;
        }

        private static void NewLabel(Transform parent, string name, string content, Vector2 pos)
        {
            NewText(parent, name, content, pos, new Vector2(90, 40), 24);
        }

        private static Text NewText(Transform parent, string name, string content,
            Vector2 pos, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return t;
        }
    }
}
