using System;
using System.Collections;
using NUnit.Framework;
using Slot.Core;
using Slot.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace Slot.Tests
{
    public class UpgradeReelPlayTests
    {
        private GameObject root;

        private ReelColumn CreateReel(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(root.transform, false);
            var reel = go.AddComponent<ReelColumn>();
            reel.Init(go.transform);
            reel.SetRandomAll(new System.Random(42));
            return reel;
        }

        [SetUp]
        public void SetUp() { root = new GameObject("Upgrade test root"); }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static IEnumerator WaitForIdle(ReelColumn reel)
        {
            float deadline = Time.realtimeSinceStartup + 4f;
            while (!reel.IsIdle && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(reel.IsIdle, "Reel must finish braking within four seconds.");
        }

        private static void AssertVisibleGrid(ReelColumn reel)
        {
            var symbols = reel.GetVisibleSymbols();
            Assert.AreEqual(4, symbols.Length);
            foreach (var symbol in symbols)
                Assert.That((int)symbol, Is.InRange(1, 10));
            for (int row = 0; row < ReelColumn.Rows; row++)
            {
                int matching = 0;
                foreach (Transform child in reel.transform)
                    if (Mathf.Abs(((RectTransform)child).anchoredPosition.y - ReelColumn.SlotY[row]) < 0.1f) matching++;
                Assert.AreEqual(1, matching, "Each visible grid cell needs exactly one icon.");
                Assert.AreEqual(ReelCell.GetFruitSprite(symbols[row]), reel.GetVisibleIcon(row).sprite,
                    "The visible image and evaluated symbol must agree after reel wraps.");
            }
        }

        [UnityTest]
        public IEnumerator MissingIncompleteOrDuplicateReelsDisableSpinInsteadOfEvaluating()
        {
            for (int scenario = 0; scenario < 4; scenario++)
            {
                var reels = new ReelColumn[5];
                for (int i = 0; i < 5; i++) reels[i] = CreateReel("guard reel " + i);
                if (scenario == 0) reels[2] = null;
                if (scenario == 1) reels[2] = reels[1];
                if (scenario == 2) UnityEngine.Object.DestroyImmediate(reels[2].transform.Find("icon0").gameObject);
                if (scenario == 3) reels = new ReelColumn[4];
                var controlGo = new GameObject("invalid controller");
                controlGo.transform.SetParent(root.transform);
                var button = new GameObject("spin", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<Button>();
                button.transform.SetParent(controlGo.transform);
                var message = new GameObject("error", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
                message.transform.SetParent(controlGo.transform);
                var controller = controlGo.AddComponent<SlotGameController>();
                controller.BindRefs(null, message, button, null, null, null, null, reels, null);
                LogAssert.Expect(LogType.Error, new Regex(@"\[Slot\] Configuration error:"));
                yield return null;
                button.onClick.Invoke();
                controller.SendMessage("OnSpinClicked");
                Assert.IsFalse(button.interactable);
                StringAssert.Contains("unavailable", message.text);
                foreach (var reel in reels) if (reel != null) Assert.IsTrue(reel.IsIdle);
                UnityEngine.Object.Destroy(controlGo);
                yield return null;
            }
        }

        [Test]
        public void OffGridReelCannotFabricateAVisibleSymbol()
        {
            var reel = CreateReel("off grid");
            var icon = (RectTransform)reel.transform.Find("icon1");
            icon.anchoredPosition += new Vector2(0, 10);
            Assert.Throws<InvalidOperationException>(() => reel.GetVisibleSymbols());
        }

        [UnityTest]
        public IEnumerator ReelBrakesAndKeepsSettledSymbols()
        {
            var reel = CreateReel("reel");
            reel.StartScroll();
            yield return new WaitForSeconds(0.15f);
            reel.BeginBrake();
            yield return WaitForIdle(reel);
            AssertVisibleGrid(reel);
            var stopped = reel.GetVisibleSymbols();
            yield return new WaitForSeconds(0.15f);
            CollectionAssert.AreEqual(stopped, reel.GetVisibleSymbols());
        }

        [UnityTest]
        public IEnumerator RepeatedStopRequestsDoNotBreakTheGrid()
        {
            var reel = CreateReel("reel");
            reel.StartScroll();
            yield return new WaitForSeconds(0.05f);
            for (int i = 0; i < 20; i++) reel.BeginBrake();
            yield return WaitForIdle(reel);
            AssertVisibleGrid(reel);
        }

        [UnityTest]
        public IEnumerator ThirtySequentialSpinsCanRestartWithoutMissingCells()
        {
            var reel = CreateReel("reel");
            for (int i = 0; i < 30; i++)
            {
                reel.StartScroll();
                yield return new WaitForSeconds(0.03f + i % 3 * 0.03f);
                reel.BeginBrake();
                yield return WaitForIdle(reel);
                AssertVisibleGrid(reel);
            }
        }

        [UnityTest]
        public IEnumerator ControllerAcceptsStopBurstAndReturnsToIdle()
        {
            var reels = new ReelColumn[5];
            for (int i = 0; i < reels.Length; i++) reels[i] = CreateReel("reel" + i);
            var controller = root.AddComponent<SlotGameController>();
            controller.BindRefs(null, null, null, null, null, null, null, reels, null);
            yield return null;
            controller.SendMessage("OnSpinClicked");
            for (int i = 0; i < 20; i++) controller.SendMessage("OnSpinClicked");
            foreach (var reel in reels) { yield return WaitForIdle(reel); AssertVisibleGrid(reel); }
            yield return new WaitForSeconds(2f);
            controller.SendMessage("OnSpinClicked");
            Assert.IsTrue(reels[0].IsScrolling, "A second spin must start after the first one completes.");
            foreach (var reel in reels) reel.BeginBrake();
            foreach (var reel in reels) yield return WaitForIdle(reel);
        }
    }
}
