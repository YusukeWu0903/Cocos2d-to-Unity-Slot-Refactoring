namespace Slot.Core
{
    /// <summary>
    /// 單條支付線：由 5 個格位編號組成（編號 1..20，對應 5軸×4行 的 FruitsId 陣列）。
    /// 格位編號 = 軸(0..4)*4 + 行(0..3) + 1，與原始 Lua ResultIndex 完全一致。
    /// </summary>
    public readonly struct Payline
    {
        /// <summary>線編號，1..30。</summary>
        public readonly int Id;

        /// <summary>5 個格位編號（1-based，與 Lua FruitsId 對應）。</summary>
        public readonly int[] Cells;

        public Payline(int id, int[] cells)
        {
            Id = id;
            Cells = cells;
        }

        public override string ToString() => $"Line{Id}: [{string.Join(",", Cells)}]";
    }

    /// <summary>Four visible rows, forty fixed paths. Original paths are retained, with a fourth horizontal and nine mirrored folds.</summary>
    public static class SlotPaylines
    {
        public const int LinesPerLevel = 8;
        public const int MaxLevel = 5;
        public static readonly Payline[] All = new Payline[]
        {
            new Payline(1, new[] { 2, 6, 10, 14, 18 }),
            new Payline(2, new[] { 3, 7, 11, 15, 19 }),
            new Payline(3, new[] { 1, 5, 9, 13, 17 }),
            new Payline(4, new[] { 4, 8, 12, 16, 20 }),
            new Payline(5, new[] { 3, 6, 9, 14, 19 }),
            new Payline(6, new[] { 1, 6, 11, 14, 17 }),
            new Payline(7, new[] { 2, 7, 11, 15, 18 }),
            new Payline(8, new[] { 2, 5, 9, 13, 18 }),
            new Payline(9, new[] { 3, 7, 10, 13, 17 }),
            new Payline(10, new[] { 1, 5, 10, 15, 19 }),
            new Payline(11, new[] { 2, 5, 10, 15, 18 }),
            new Payline(12, new[] { 2, 7, 10, 13, 18 }),
            new Payline(13, new[] { 3, 6, 10, 14, 18 }),
            new Payline(14, new[] { 1, 6, 10, 14, 17 }),
            new Payline(15, new[] { 3, 6, 11, 14, 19 }),
            new Payline(16, new[] { 1, 6, 9, 14, 17 }),
            new Payline(17, new[] { 2, 6, 11, 14, 18 }),
            new Payline(18, new[] { 2, 6, 9, 14, 18 }),
            new Payline(19, new[] { 3, 7, 9, 15, 19 }),
            new Payline(20, new[] { 1, 5, 11, 13, 17 }),
            new Payline(21, new[] { 3, 5, 9, 13, 19 }),
            new Payline(22, new[] { 1, 7, 11, 15, 17 }),
            new Payline(23, new[] { 2, 5, 11, 13, 18 }),
            new Payline(24, new[] { 2, 7, 9, 15, 18 }),
            new Payline(25, new[] { 3, 5, 11, 13, 19 }),
            new Payline(26, new[] { 1, 7, 9, 15, 17 }),
            new Payline(27, new[] { 1, 7, 10, 13, 19 }),
            new Payline(28, new[] { 3, 5, 10, 15, 17 }),
            new Payline(29, new[] { 3, 5, 10, 13, 19 }),
            new Payline(30, new[] { 1, 7, 10, 15, 17 }),
            new Payline(31, new[] { 1, 6, 11, 15, 18 }),
            new Payline(32, new[] { 2, 7, 12, 15, 18 }),
            new Payline(33, new[] { 4, 7, 10, 15, 20 }),
            new Payline(34, new[] { 3, 8, 12, 16, 19 }),
            new Payline(35, new[] { 2, 6, 11, 16, 20 }),
            new Payline(36, new[] { 4, 8, 11, 14, 18 }),
            new Payline(37, new[] { 3, 8, 11, 14, 19 }),
            new Payline(38, new[] { 3, 6, 11, 16, 19 }),
            new Payline(39, new[] { 4, 7, 11, 15, 20 }),
            new Payline(40, new[] { 4, 7, 12, 15, 20 }),
        };
    }
}
