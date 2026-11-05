using NUnit.Framework;
using Slot.Core;

namespace Slot.Tests
{
    /// <summary>Wild(ID8) 與 9/10 高賠特例測試，對應 Lua CorrectIine 的 change 邏輯。</summary>
    public class WildCorrectTest
    {
        static SymbolId[] Empty()
        {
            var cells = new SymbolId[21];
            for (int i = 1; i <= 20; i++) cells[i] = SymbolId.S10;
            return cells;
        }

        // 直取 Line1 = {2,6,10,14,18}（row1 橫排）做 Wild 測試

        // ============ 中段 Wild 替代 ============

        [Test]
        public void Wild_InMiddle_Substitutes()
        {
            // Line1: [S1][Wild][S1][S1][S1] → Wild 取代 S1 → 5 連
            var cells = Empty();
            cells[2] = SymbolId.S1;
            cells[6] = SymbolId.S8; // wild 取代 S1
            cells[10] = cells[14] = cells[18] = SymbolId.S1;
            var r = Evaluate(1, cells, change: false);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
        }

        [Test]
        public void Wild_OnlyAtAnchorSubstitution_Counts()
        {
            // Line1: [S1][S1][Wild][S3][S3] → 前兩格 S1 + Wild → 3 連
            var cells = Empty();
            cells[2] = cells[6] = SymbolId.S1;
            cells[10] = SymbolId.S8;
            cells[14] = cells[18] = SymbolId.S3;
            var r = Evaluate(1, cells, change: false);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(3, r.Count);
        }

        // ============ 高賠 9/10 不接受 Wild ============

        [Test]
        public void HighPay10_DoesNotAcceptWild_Count1()
        {
            // Line1: [S10][Wild][S10][S10][S10] → 基準 S10 為高賠，第一格 Wild 即 break → 1 連不中
            var cells = Empty();
            cells[2] = SymbolId.S10;
            cells[6] = SymbolId.S8; // wild
            cells[10] = cells[14] = cells[18] = SymbolId.S10;
            var r = Evaluate(1, cells, change: false);
            Assert.IsFalse(r.IsWin);
            Assert.AreEqual(1, r.Count); // 基準 S10 高賠，Wild 不成立直接 stop
        }

        [Test]
        public void HighPay9_DoesNotAcceptWild_Count1()
        {
            var cells = Empty();
            cells[2] = SymbolId.S9;
            cells[6] = SymbolId.S8;
            cells[10] = cells[14] = cells[18] = SymbolId.S9;
            var r = Evaluate(1, cells, change: false);
            Assert.IsFalse(r.IsWin);
            Assert.AreEqual(1, r.Count);
        }

        // ============ change=true（第一格為 Wild） ============

        [Test]
        public void FirstIsWild_Change_SubsequentSame_Count5()
        {
            // Line1: [Wild][S3][S3][S3][S3] → change=true，基準 S3，5 連
            var cells = Empty();
            cells[2] = SymbolId.S8; // 第一格 wild
            cells[6] = cells[10] = cells[14] = cells[18] = SymbolId.S3;
            var r = Evaluate(1, cells, change: true);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
            Assert.AreEqual(SymbolId.S3, r.Symbol);
        }

        [Test]
        public void FirstIsWild_AllSubsequentWild_Count5_SymbolWild()
        {
            // Line1: 全 Wild → 5 連，基準無法鎖定 → 符號為 Wild
            var cells = Empty();
            for (int i = 1; i <= 5; i++) cells[line1[i - 1]] = SymbolId.S8;
            var r = Evaluate(1, cells, change: true);
            Assert.IsTrue(r.IsWin);
            Assert.AreEqual(5, r.Count);
            Assert.AreEqual(SymbolId.S8, r.Symbol);
        }

        [Test]
        public void FirstIsWild_FollowedByHighPay_Breaks_Count1()
        {
            // Line1: [Wild][S10][S10][S10][S10] → 第二格為高賠10 → 不接受，break → 1 連不中
            var cells = Empty();
            cells[2] = SymbolId.S8;
            cells[6] = cells[10] = cells[14] = cells[18] = SymbolId.S10;
            var r = Evaluate(1, cells, change: true);
            Assert.IsFalse(r.IsWin);
            Assert.AreEqual(1, r.Count);
        }

        // ============ 全部線的第一格 Wild 通用 ============

        [Test]
        public void AnyLine_FirstWild_AllSame_Count5()
        {
            foreach (var line in SlotPaylines.All)
            {
                var cells = Empty();
                cells[line.Cells[0]] = SymbolId.S8;
                for (int col = 1; col < 5; col++) cells[line.Cells[col]] = SymbolId.S2;
                var r = Evaluate(line.Id, cells, change: true);
                Assert.IsTrue(r.IsWin, $"Line{line.Id} 第一格 Wild 且其餘 S2 應中獎");
                Assert.AreEqual(5, r.Count);
            }
        }

        static readonly int[] line1 = { 2, 6, 10, 14, 18 };

        static WinResult Evaluate(int lineId, SymbolId[] cells, bool change)
        {
            var line = SlotPaylines.All[lineId - 1];
            return PaylineEvaluator.EvaluateLine(line, cells, change);
        }
    }
}
