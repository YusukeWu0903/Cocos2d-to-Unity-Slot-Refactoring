using NUnit.Framework;
using Slot.Core;

namespace Slot.Tests
{
    /// <summary>連數門檻：3/4/5 連邊界、2 連不中、斷線後續不計。</summary>
    public class CountThresholdTest
    {
        static SymbolId[] Empty()
        {
            var cells = new SymbolId[21];
            for (int i = 1; i <= 20; i++) cells[i] = SymbolId.S10;
            return cells;
        }

        // Line2 = {3,7,11,15,19}（row2 橫排）

        [Test]
        public void Exactly5_AllSame_Count5()
        {
            var cells = Empty();
            foreach (int p in new[] { 3, 7, 11, 15, 19 }) cells[p] = SymbolId.S1;
            var r = Evaluate(2, cells);
            Assert.AreEqual(5, r.Count);
            Assert.IsTrue(r.IsWin);
        }

        [Test]
        public void Exactly4_RestDiff_Count4()
        {
            var cells = Empty();
            cells[3] = cells[7] = cells[11] = cells[15] = SymbolId.S1;
            cells[19] = SymbolId.S2; // 第 5 格不同 → 斷在 4 連
            var r = Evaluate(2, cells);
            Assert.AreEqual(4, r.Count);
            Assert.IsTrue(r.IsWin);
        }

        [Test]
        public void Exactly3_RestDiff_Count3()
        {
            var cells = Empty();
            cells[3] = cells[7] = cells[11] = SymbolId.S1;
            cells[15] = cells[19] = SymbolId.S2;
            var r = Evaluate(2, cells);
            Assert.AreEqual(3, r.Count);
            Assert.IsTrue(r.IsWin);
        }

        [Test]
        public void TwoOnly_NotWin()
        {
            var cells = Empty();
            cells[3] = cells[7] = SymbolId.S1;
            cells[11] = cells[15] = cells[19] = SymbolId.S2;
            var r = Evaluate(2, cells);
            Assert.AreEqual(2, r.Count);
            Assert.IsFalse(r.IsWin);
        }

        [Test]
        public void One_NotWin()
        {
            var cells = Empty();
            cells[3] = SymbolId.S1;
            cells[7] = cells[11] = cells[15] = cells[19] = SymbolId.S2;
            var r = Evaluate(2, cells);
            Assert.AreEqual(1, r.Count);
            Assert.IsFalse(r.IsWin);
        }

        [Test]
        public void BrokenLine_AfterMismatch_NoContinue()
        {
            // [S1][S1][S2][S1][S1] → 第三格 S2 斷線 → count=2，後續第4/5格的 S1 不計
            var cells = Empty();
            cells[3] = cells[7] = SymbolId.S1;
            cells[11] = SymbolId.S2;
            cells[15] = cells[19] = SymbolId.S1;
            var r = Evaluate(2, cells);
            Assert.AreEqual(2, r.Count);
            Assert.IsFalse(r.IsWin);
        }

        [Test]
        public void WildCannotBridgeAcrossMismatch()
        {
            // [S1][S2][Wild][S1][S1] → 第二格 S2 斷 → 後續 wild/S1 不計 → 1 連
            var cells = Empty();
            cells[3] = SymbolId.S1;
            cells[7] = SymbolId.S2;
            cells[11] = SymbolId.S8;
            cells[15] = cells[19] = SymbolId.S1;
            var r = Evaluate(2, cells);
            Assert.AreEqual(1, r.Count);
            Assert.IsFalse(r.IsWin);
        }

        static WinResult Evaluate(int lineId, SymbolId[] cells)
        {
            var line = SlotPaylines.All[lineId - 1];
            bool change = cells[line.Cells[0]] == SymbolId.S8;
            return PaylineEvaluator.EvaluateLine(line, cells, change);
        }
    }
}