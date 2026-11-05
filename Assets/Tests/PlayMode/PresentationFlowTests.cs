#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Slot.Core;
using NUnit.Framework;
using Slot.Game;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Slot.Tests
{
    public class PresentationFlowTests
    {
        private Scene scene;
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }
        private static int RenderedVertices(PaylineGraphic graphic)
        {
            using (var vertices=new VertexHelper())
            {
                typeof(PaylineGraphic).GetMethod("OnPopulateMesh",BindingFlags.Instance|BindingFlags.NonPublic,null,new[] { typeof(VertexHelper) },null)
                    .Invoke(graphic,new object[] {vertices});
                return vertices.currentVertCount;
            }
        }
        [UnityTest] public IEnumerator LosingRoundClearsManualPreviewUntilExplicitlyRequestedAgain()
        {
            scene=EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/SlotPresentation.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;
            var roots=scene.GetRootGameObjects();
            var canvas=roots.Single(o=>o.name=="Canvas").transform;
            var presentation=roots.Single(o=>o.name=="SlotRoot").GetComponent<SlotPresentation>();
            var toggle=canvas.Find("TogglePaths").GetComponent<Button>();
            var graphic=canvas.Find("ReelArea/PaylineOverlay").GetComponent<PaylineGraphic>();
            Assert.AreEqual(0,RenderedVertices(graphic));
            toggle.onClick.Invoke();
            Assert.Greater(RenderedVertices(graphic),0,"Precondition: manual preview must actually draw a path.");
            presentation.BeginSpin();
            Assert.AreEqual(0,RenderedVertices(graphic));
            Assert.AreEqual("查看路徑",toggle.GetComponentInChildren<Text>().text);
            presentation.FinishSpin(System.Array.Empty<WinResult>());
            Assert.AreEqual("本次未中獎",canvas.Find("StatusPanel/Summary").GetComponent<Text>().text);
            Assert.AreEqual(0,RenderedVertices(graphic),"A loss must not restore the earlier blue preview.");
            Assert.AreEqual("已啟用 8 條線",canvas.Find("PathLabel").GetComponent<Text>().text);
            toggle.onClick.Invoke();
            Assert.Greater(RenderedVertices(graphic),0,"Manual preview must still work after a loss.");
            presentation.BeginSpin();
            presentation.FinishSpin(new[] { new WinResult(4,3,SymbolId.S1,true) });
            Assert.Greater(RenderedVertices(graphic),0,"Winning lines must remain visible.");
            StringAssert.Contains("中獎線 04",canvas.Find("PathLabel").GetComponent<Text>().text);
        }
        [UnityTest] public IEnumerator AuthoredSceneShowsChineseRulesFortyLinesAndCompletesStopBurst()
        {
            scene=EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/SlotPresentation.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;
            var canvas=scene.GetRootGameObjects().Single(o=>o.name=="Canvas").transform;
            foreach(var text in canvas.GetComponentsInChildren<Text>(true))
                foreach(char c in text.text)
                    if(!char.IsWhiteSpace(c)) Assert.IsTrue(text.font.HasCharacter(c),$"Missing glyph {c} in {text.name}");
            var bar=canvas.Find("ControlBar");
            Assert.AreEqual("8 / 40",bar.Find("LineValue").GetComponent<Text>().text);
            Assert.IsFalse(bar.Find("MoneyDown").GetComponent<Button>().interactable);
            var overlay=canvas.Find("RulesOverlay").gameObject;
            canvas.Find("Rules").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(overlay.activeSelf);
            var rulePanel=overlay.transform.Find("RulesPanel");
            Assert.IsFalse(rulePanel.Find("LineChoice40").GetComponent<Button>().interactable);
            rulePanel.Find("CloseRules").GetComponent<Button>().onClick.Invoke();
            for(int i=0;i<4;i++)bar.Find("BranchUp").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual("40 / 40",bar.Find("LineValue").GetComponent<Text>().text);
            Assert.IsFalse(bar.Find("BranchUp").GetComponent<Button>().interactable);
            canvas.Find("Rules").GetComponent<Button>().onClick.Invoke();
            rulePanel.Find("LineChoice40").GetComponent<Button>().onClick.Invoke();
            Assert.IsFalse(overlay.activeSelf);
            StringAssert.Contains("40 / 40",canvas.Find("PathLabel").GetComponent<Text>().text);
            var spin=bar.Find("SpinButton").GetComponent<Button>();
            spin.onClick.Invoke();
            Assert.IsFalse(bar.Find("BranchDown").GetComponent<Button>().interactable);
            for(int i=0;i<20;i++)spin.onClick.Invoke();
            float deadline=Time.realtimeSinceStartup+5;
            while(!bar.Find("BranchDown").GetComponent<Button>().interactable&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.IsTrue(bar.Find("BranchDown").GetComponent<Button>().interactable);
            Assert.AreNotEqual("轉輪中…",canvas.Find("StatusPanel/Summary").GetComponent<Text>().text);
            Assert.IsTrue(canvas.Find("Rules").GetComponent<Button>().interactable);
            spin.onClick.Invoke();
            Assert.AreEqual("轉輪中…",canvas.Find("StatusPanel/Summary").GetComponent<Text>().text);
        }
    }
}
#endif
