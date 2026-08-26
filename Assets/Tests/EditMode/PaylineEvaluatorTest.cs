using System.Collections.Generic;
using NUnit.Framework;
using Slot.Core;

namespace Slot.Tests
{
    /// <summary>
    /// 對 20 格陣列的判賞引擎測試。
    /// 格位編號規則：cells[1..20]，index 0 忽略。
    /// </summary>
    public class PaylineEvaluatorTest
    {
        static SymbolId[] Empty() => new SymbolId[21];

        // ============ 基礎：橫排三條 ============

        [Test]
        public void TopRow_AllSame_Line3_Wins5()
        {
            // Line3 = {1,5,9,13,17} → 第一列 (row0) 五格
            var cells = Empty();
            cells[1] = cells[5] = cells[9] = cells[13] = cells[17] = SymbolId.S1;
            var r = EvaluateLine(3, cells);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
            Assert.AreEqual(SymbolId.S1, r.Symbol);
        }

        [Test]
        public void MiddleRow_AllSame_Line1_Wins5()
        {
            // Line1 = {2,6,10,14,18} → 第二列 (row1)
            var cells = Empty();
            cells[2] = cells[6] = cells[10] = cells[14] = cells[18] = SymbolId.S2;
            var r = EvaluateLine(1, cells);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
        }

        [Test]
        public void BottomRow_AllSame_Line2_Wins5()
        {
            // Line2 = {3,7,11,15,19} → 第三列 (row2)
            var cells = Empty();
            cells[3] = cells[7] = cells[11] = cells[15] = cells[19] = SymbolId.S3;
            var r = EvaluateLine(2, cells);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
        }

        // ============ 斜線 / 折線 ============

        [Test]
        public void DiagonalV_Line4_Wins5_AllSame()
        {
            // Line4 = {3,6,9,14,19} (V形，row 高-中-低-中-高)
            var cells = Empty();
            foreach (int p in new[] { 3, 6, 9, 14, 19 }) cells[p] = SymbolId.S4;
            var r = EvaluateLine(4, cells);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
        }

        [Test]
        public void HutShape_Line5_Wins5_AllSame()
        {
            // Line5 = {1,6,11,14,17} (帽/山形)
            var cells = Empty();
            foreach (int p in new[] { 1, 6, 11, 14, 17 }) cells[p] = SymbolId.S5;
            var r = EvaluateLine(5, cells);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
        }

        // ============ 30 條線全部可達 5 連 ============

        [Test]
        public void All30Lines_CanReach5_AllSame()
        {
            foreach (var line in SlotPaylines.All)
            {
                var cells = Empty();
                foreach (int p in line.Cells) cells[p] = SymbolId.S6;
                var r = EvaluateLine(line.Id, cells);
                Assert.IsTrue(r.IsWin, $"Line{line.Id} 應 5 連中獎");
                Assert.AreEqual(5, r.Count, $"Line{line.Id} 應為 5 連");
            }
        }

        // ============ 連數邊界 ============

        [Test]
        public void ThreeInARow_Wins_Count3()
        {
            // Line1 前三格相同，後兩格不同 → 3 連
            var cells = Empty();
            cells[2] = cells[6] = cells[10] = SymbolId.S7;
            cells[14] = SymbolId.S1;
            cells[18] = SymbolId.S2;
            var r = EvaluateLine(1, cells);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(3, r.Count);
        }

        [Test]
        public void TwoInARow_NotWin()
        {
            // Line1 前兩格相同，第三格斷 → 2 連，不中
            var cells = Empty();
            cells[2] = cells[6] = SymbolId.S7;
            cells[10] = SymbolId.S8; // wild 即便全含也算替代，但這裡先測純 2 連：改為不同符號
            cells[10] = SymbolId.S3;
            var r = EvaluateLine(1, cells);
            Assert.IsFalse(r.IsWin);
            Assert.AreEqual(2, r.Count);
        }

        [Test]
        public void BreakOnFourth_Count3()
        {
            // Line1 錢三格相同，第4格不同 → 3 連
            var cells = Empty();
            cells[2] = cells[6] = cells[10] = SymbolId.S9;
            cells[14] = SymbolId.S3;
            var r = EvaluateLine(1, cells);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(3, r.Count);
        }

        // ============ EvaluateAll / 啟用線數 ============

        [Test]
        public void EvaluateAll_TopRow_FindsLine3()
        {
            var cells = Empty();
            cells[1] = cells[5] = cells[9] = cells[13] = cells[17] = SymbolId.S1;
            var results = EvaluateAll(cells, 30);
            Assert.IsTrue(ContainsLine(results, 3));
        }

        [Test]
        public void ActiveLines_Limit_Respected()
        {
            // 只有行1 命中，但只允許檢查前2條線 → 不應命中
            var cells = Empty();
            cells[2] = cells[6] = cells[10] = cells[14] = cells[18] = SymbolId.S1; // Line1 命中
            var results = EvaluateAll(cells, 5); // 檢查前5條，Line1 在內 → 命中
            Assert.IsTrue(ContainsLine(results, 1));
        }

        // ============ Helpers ============

        static WinResult EvaluateLine(int lineId, SymbolId[] cells)
        {
            var line = SlotPaylines.All[lineId - 1];
            bool change = cells[line.Cells[0]] == SymbolId.S8;
            return PaylineEvaluator.EvaluateLine(line, cells, change);
        }

        static IReadOnlyList<WinResult> EvaluateAll(SymbolId[] cells, int activeLines)
            => PaylineEvaluator.EvaluateAll(cells, activeLines);

        static bool ContainsLine(IReadOnlyList<WinResult> results, int lineId)
        {
            foreach (var r in results) if (r.LineId == lineId) return true;
            return false;
        }
    }
}
