using NUnit.Framework;
using Slot.Core;

namespace Slot.Tests
{
    /// <summary>Phase 0 骨架 smoke test：驗證 Slot.Tests assembly 與 Slot.Core 正確連結。</summary>
    public class SkeletonSmokeTest
    {
        [Test]
        public void SymbolId_HasWild()
        {
            Assert.AreEqual(SymbolId.S8, (SymbolId)8);
            Assert.IsTrue((int)SymbolId.S8 == 8);
        }

        [Test]
        public void SymbolId_All_1_Through_10_Valid()
        {
            for (int i = 1; i <= 10; i++)
            {
                Assert.AreNotEqual(SymbolId.None, (SymbolId)i);
            }
        }
    }
}