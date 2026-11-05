using System.Linq;
using NUnit.Framework;
using Slot.Core;

namespace Slot.Tests
{
    public class FourRowPaylineTests
    {
        private static SymbolId[] Board()
        {
            var board = new SymbolId[21];
            for (int cell = 1; cell <= 20; cell++) board[cell] = (SymbolId)((cell - 1) / 4 + 1);
            return board;
        }

        [Test] public void FortyUniquePathsUseOneCellPerColumn()
        {
            Assert.AreEqual(40, SlotPaylines.All.Length);
            Assert.AreEqual(40, SlotPaylines.All.Select(p => string.Join(",", p.Cells)).Distinct().Count());
            for (int i = 0; i < 40; i++)
            {
                var line = SlotPaylines.All[i];
                Assert.AreEqual(i + 1, line.Id);
                for (int col = 0; col < 5; col++) Assert.AreEqual(col, (line.Cells[col]-1)/4);
            }
        }

        [TestCase(0,3)] [TestCase(1,1)] [TestCase(2,2)] [TestCase(3,4)]
        public void EveryVisibleRowCanWinAtLowestLineSetting(int row, int lineId)
        {
            var board = Board();
            for (int col = 0; col < 5; col++) board[col*4+row+1] = SymbolId.S7;
            var result = PaylineEvaluator.EvaluateAll(board, 8).Single(r => r.LineId == lineId);
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(SymbolId.S7, result.Symbol);
        }

        [TestCase(8)] [TestCase(16)] [TestCase(24)] [TestCase(32)] [TestCase(40)]
        public void EveryLineSettingIncludesAllFourHorizontalRows(int enabled)
        {
            var board = Board();
            for (int i=1;i<=20;i++) board[i]=SymbolId.S1;
            var wins=PaylineEvaluator.EvaluateAll(board, enabled);
            Assert.AreEqual(enabled,wins.Count);
            foreach (int id in new[] {1,2,3,4}) Assert.IsTrue(wins.Any(w=>w.LineId==id));
        }

        [Test] public void BottomRowFoldCanWinAndDisabledFoldDoesNotWin()
        {
            var board=Board();
            var line=SlotPaylines.All[39];
            Assert.IsTrue(line.Cells.Any(c=>(c-1)%4==3));
            foreach(int cell in line.Cells) board[cell]=SymbolId.S6;
            Assert.IsFalse(PaylineEvaluator.EvaluateAll(board,32).Any(w=>w.LineId==40));
            Assert.AreEqual(5,PaylineEvaluator.EvaluateAll(board,40).Single(w=>w.LineId==40).Count);
        }
    }
}
