namespace Slot.Core
{
    /// <summary>單條支付線的判賞結果。</summary>
    public readonly struct WinResult
    {
        /// <summary>命中的支付線編號（1..40）。</summary>
        public readonly int LineId;

        /// <summary>連號數（3/4/5）。</summary>
        public readonly int Count;

        /// <summary>基準符號（若全部為 Wild，則為 S8）。</summary>
        public readonly SymbolId Symbol;

        /// <summary>是否中獎（Count &gt;= 3）。</summary>
        public readonly bool IsWin;

        public WinResult(int lineId, int count, SymbolId symbol, bool isWin)
        {
            LineId = lineId;
            Count = count;
            Symbol = symbol;
            IsWin = isWin;
        }

        public override string ToString()
            => $"Line{LineId}: {Count}x {Symbol} (win={IsWin})";
    }
}
