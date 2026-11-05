#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Slot.Game;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Slot.Tests
{
    public class CabinetFlowTests
    {
        private Scene scene;
        [UnityTearDown] public IEnumerator Cleanup()
        {if(scene.IsValid()&&scene.isLoaded)yield return SceneManager.UnloadSceneAsync(scene);}
        [UnityTest] public IEnumerator CabinetCompletesOneHundredMixedAutomaticAndStopRounds()
        {
            scene=EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/SlotCabinet.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;
            var canvas=scene.GetRootGameObjects().Single(o=>o.name=="Canvas").transform;
            var bar=canvas.Find("ControlBar");
            var start=bar.Find("SpinButton").GetComponent<Button>();
            var reels=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<ReelColumn>()).ToArray();
            float originalScale=Time.timeScale;
            try
            {
                Time.timeScale=8;
                for(int round=0;round<100;round++)
                {
                    start.onClick.Invoke();
                    if(round%3!=0)
                    {
                        yield return new WaitForSeconds(.013f+(round%11)*.031f);
                        for(int repeat=0;repeat<1+round%7;repeat++)start.onClick.Invoke();
                    }
                    float deadline=Time.realtimeSinceStartup+4;
                    while(start.GetComponentInChildren<Text>().text.Contains("STOP")&&Time.realtimeSinceStartup<deadline)yield return null;
                    Assert.AreEqual("開始\nSTART",start.GetComponentInChildren<Text>().text,$"Round {round}");
                    Assert.That(canvas.Find("StateMeter/Value").GetComponent<Text>().text,Is.EqualTo("WIN").Or.EqualTo("NO WIN"));
                    foreach(var reel in reels)
                    {
                        Assert.IsTrue(reel.IsIdle,$"Round {round}");
                        var symbols=reel.GetVisibleSymbols();
                        for(int row=0;row<4;row++)Assert.AreEqual(ReelCell.GetFruitSprite(symbols[row]),reel.GetVisibleIcon(row).sprite);
                    }
                }
            }
            finally {Time.timeScale=originalScale;}
        }
        [UnityTest] public IEnumerator ErrorStateCannotKeepAnApparentlyRunningStopButton()
        {
            scene=EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/SlotCabinet.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;
            var canvas=scene.GetRootGameObjects().Single(o=>o.name=="Canvas").transform;
            var view=scene.GetRootGameObjects().Single(o=>o.name=="SlotRoot").GetComponent<SlotPresentation>();
            view.BeginSpin();view.ShowError();
            Assert.AreEqual("暫停\nERROR",canvas.Find("ControlBar/SpinButton").GetComponentInChildren<Text>().text);
            Assert.AreEqual("ERROR",canvas.Find("StateMeter/Value").GetComponent<Text>().text);
        }
        [UnityTest] public IEnumerator CabinetMetersLightsAndPhysicalStartControlFollowTheRealRound()
        {
            scene=EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/SlotCabinet.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            yield return null;
            var canvas=scene.GetRootGameObjects().Single(o=>o.name=="Canvas").transform;
            Assert.AreEqual(new Vector2(1024,800),canvas.GetComponent<CanvasScaler>().referenceResolution);
            foreach(var label in canvas.GetComponentsInChildren<Text>(true))
                foreach(char c in label.text)if(!char.IsWhiteSpace(c))Assert.IsTrue(label.font.HasCharacter(c),$"Missing cabinet glyph {c}");
            var bar=canvas.Find("ControlBar");
            Assert.AreEqual("01",canvas.Find("UnitMeter/Value").GetComponent<Text>().text);
            Assert.AreEqual("08 / 40",canvas.Find("LinesMeter/Value").GetComponent<Text>().text);
            Assert.Greater(canvas.Find("CabinetLine8").GetComponent<Image>().color.maxColorComponent,.7f);
            Assert.Less(canvas.Find("CabinetLine9").GetComponent<Image>().color.maxColorComponent,.2f);
            bar.Find("MoneyUp").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual("02",canvas.Find("UnitMeter/Value").GetComponent<Text>().text);
            for(int i=0;i<2;i++)bar.Find("BranchUp").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual("24 / 40",canvas.Find("LinesMeter/Value").GetComponent<Text>().text);
            Assert.Greater(canvas.Find("CabinetLine24").GetComponent<Image>().color.maxColorComponent,.7f);
            Assert.Less(canvas.Find("CabinetLine25").GetComponent<Image>().color.maxColorComponent,.2f);
            canvas.Find("Rules").GetComponent<Button>().onClick.Invoke();
            var overlay=canvas.Find("RulesOverlay").gameObject;Assert.IsTrue(overlay.activeSelf);
            overlay.transform.Find("RulesPanel/LineChoice4").GetComponent<Button>().onClick.Invoke();
            Assert.IsFalse(overlay.activeSelf);
            StringAssert.Contains("04 / 24",canvas.Find("PathLabel").GetComponent<Text>().text);
            var start=bar.Find("SpinButton").GetComponent<Button>();
            Assert.AreSame(start.GetComponent<RetroCabinetGraphic>(),start.targetGraphic);
            Assert.AreEqual("開始\nSTART",start.GetComponentInChildren<Text>().text);
            start.onClick.Invoke();
            Assert.AreEqual("停輪\nSTOP",start.GetComponentInChildren<Text>().text);
            Assert.AreEqual("SPIN",canvas.Find("StateMeter/Value").GetComponent<Text>().text);
            Assert.IsFalse(bar.Find("MoneyUp").GetComponent<Button>().interactable);
            for(int i=0;i<20;i++)start.onClick.Invoke();
            float deadline=Time.realtimeSinceStartup+5;
            while(!bar.Find("MoneyUp").GetComponent<Button>().interactable&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.IsTrue(bar.Find("MoneyUp").GetComponent<Button>().interactable);
            Assert.AreEqual("開始\nSTART",start.GetComponentInChildren<Text>().text);
            Assert.That(canvas.Find("StateMeter/Value").GetComponent<Text>().text,Is.EqualTo("WIN").Or.EqualTo("NO WIN"));
            Assert.IsTrue(int.TryParse(canvas.Find("MatchesMeter/Value").GetComponent<Text>().text,out var count));
            Assert.That(count,Is.InRange(0,40));
        }
    }
}
#endif
