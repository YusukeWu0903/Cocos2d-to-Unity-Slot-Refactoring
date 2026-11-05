using System;
using NUnit.Framework;
using Slot.Core;

namespace Slot.Tests
{
    public class InvalidBoardTests
    {
        private static SymbolId[] Board()
        {
            var cells = new SymbolId[21];
            for (int i = 1; i <= 20; i++) cells[i] = SymbolId.S2;
            return cells;
        }

        [Test] public void MissingStorageCannotBeEvaluated()
        {
            Assert.Throws<ArgumentNullException>(() => PaylineEvaluator.EvaluateAll(null));
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateAll(new SymbolId[20]));
        }

        [Test] public void EmptyBoardCannotProduceWildWins()
        {
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateAll(new SymbolId[21]));
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(SlotPaylines.All[0], new SymbolId[21], false));
        }

        [TestCase(0)] [TestCase(-1)] [TestCase(11)]
        public void InvalidCellIsRejectedEvenAfterAWinningPrefix(int invalid)
        {
            var cells = Board();
            cells[18] = (SymbolId)invalid;
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateAll(cells));
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(SlotPaylines.All[0], cells, false));
        }

        [Test] public void UnusedIndexZeroDoesNotAffectResults()
        {
            var cells = Board();
            cells[0] = (SymbolId)999;
            Assert.AreEqual(40, PaylineEvaluator.EvaluateAll(cells).Count);
        }

        [Test] public void MalformedLinesCannotReadOutsideTheBoardOrRepeatAReel()
        {
            var cells = Board();
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(default, cells, false));
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(new Payline(1, new[] {2,6}), cells, false));
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(new Payline(1, new[] {0,6,10,14,18}), cells, false));
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(new Payline(1, new[] {2,6,10,14,21}), cells, false));
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(new Payline(1, new[] {2,2,10,14,18}), cells, false));
        }

        [TestCase(-3, 0)] [TestCase(0, 0)] [TestCase(1, 1)]
        [TestCase(6, 6)] [TestCase(15, 15)] [TestCase(32, 32)] [TestCase(40, 40)] [TestCase(100, 40)]
        public void EnabledLineCountIsClampedAndOnlyThoseLinesAreEvaluated(int requested, int expected)
        {
            var wins = PaylineEvaluator.EvaluateAll(Board(), requested);
            Assert.AreEqual(expected, wins.Count);
            foreach (var win in wins) Assert.That(win.LineId, Is.InRange(1, expected));
        }

        [Test] public void WrongWildModeIsRejected()
        {
            var cells = Board();
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(SlotPaylines.All[0], cells, true));
            cells[2] = SymbolId.S8;
            Assert.Throws<ArgumentException>(() => PaylineEvaluator.EvaluateLine(SlotPaylines.All[0], cells, false));
        }
    }
}
