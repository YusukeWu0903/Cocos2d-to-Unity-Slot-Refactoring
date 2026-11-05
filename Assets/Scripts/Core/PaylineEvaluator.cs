using System;
using System.Collections.Generic;

namespace Slot.Core
{
    /// <summary>
    /// 判賞引擎：對 20 格符號陣列，逐一檢查啟用的支付線，回傳所有中獎結果。
    /// 移植自 Lua MainScene:CorrectIine + Result，沿用 Unity 的鈴鐺替代規則；GOOD LUCK / BAR 不接受替代。
    /// </summary>
    public static class PaylineEvaluator
    {
        /// <summary>Wild 符號。</summary>
        public const SymbolId Wild = SymbolId.S8;

        /// <summary>特殊圖案（GOOD LUCK / BAR，不接受 Wild 替代）— S9 / S10。</summary>
        public static bool IsHighPaySymbol(SymbolId s) => s == SymbolId.S9 || s == SymbolId.S10;

        /// <summary>
        /// 判定單條線是否中獎。
        /// </summary>
        /// <param name="line">支付線定義（Cells 為 5 個格位編號）。</param>
        /// <param name="cells">20 個格位的符號（索引 1..20，index 0 未使用）。</param>
        /// <param name="change">是否「變」（第一個符號為 Wild 時為 true，對應 Lua FruitsId[i]==8）。</param>
        public static WinResult EvaluateLine(Payline line, SymbolId[] cells, bool change)
        {
            ValidateStorage(cells);
            ValidateLine(line, cells);
            if (change != (cells[line.Cells[0]] == Wild))
                throw new ArgumentException("Wild mode must match the first symbol.", nameof(change));
            int count = 1;               // 基準格已至少算 1
            SymbolId anchor = SymbolId.None;
            int lastIndex = -1;          // 記錄最後相符格位，供測試定位

            var lineCells = line.Cells;
            SymbolId first = cells[lineCells[0]];

            if (!change)
            {
                // ---- change == false：以第一格為基準 ----
                anchor = first;
                for (int col = 1; col < 5; col++)
                {
                    SymbolId sym = cells[lineCells[col]];
                    if (sym == Wild)
                    {
                        // Wild 視為同符；但基準為 9/10 時 Wild 不成立（break）
                        if (!IsHighPaySymbol(anchor))
                        {
                            count++;
                            lastIndex = col;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (sym == anchor)
                    {
                        count++;
                        lastIndex = col;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                // ---- change == true：第一個為 Wild，需抓到第一個非9/10者當基準 ----
                anchor = SymbolId.None;   // 尚未鎖定
                for (int col = 1; col < 5; col++)
                {
                    SymbolId sym = cells[lineCells[col]];
                    if (sym == Wild)
                    {
                        count++;
                        lastIndex = col;
                    }
                    else if (anchor == SymbolId.None)
                    {
                        // 鎖定第一個非 Wild 為基準；9/10 特殊圖案不接受（break）
                        if (!IsHighPaySymbol(sym))
                        {
                            anchor = sym;
                            count++;
                            lastIndex = col;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (sym == anchor)
                    {
                        count++;
                        lastIndex = col;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            bool isWin = count >= 3;
            SymbolId resultSymbol = isWin ? (anchor == SymbolId.None ? Wild : anchor) : SymbolId.None;
            return new WinResult(line.Id, count, resultSymbol, isWin);
        }

        private static void ValidateStorage(SymbolId[] cells)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            if (cells.Length < 21)
                throw new ArgumentException("A board requires indices 1 through 20.", nameof(cells));
        }

        private static void ValidateSymbol(SymbolId symbol, int cell)
        {
            if (symbol < SymbolId.S1 || symbol > SymbolId.S10)
                throw new ArgumentException($"Cell {cell} has no valid symbol.", "cells");
        }

        private static void ValidateLine(Payline line, SymbolId[] cells)
        {
            if (line.Cells == null || line.Cells.Length != 5)
                throw new ArgumentException("A payline requires five cells.", nameof(line));
            for (int col = 0; col < 5; col++)
            {
                int cell = line.Cells[col];
                if (cell < 1 || cell > 20 || (cell - 1) / 4 != col)
                    throw new ArgumentException("A payline must use one cell per reel, left to right.", nameof(line));
                ValidateSymbol(cells[cell], cell);
            }
        }

        /// <summary>
        /// 判定所有啟用線，回傳中獎線（IsWin == true）清單。
        /// </summary>
        /// <param name="cells">20 格符號，index 1..20（index 0 忽略）。</param>
        /// <param name="activeLines">啟用線數（0..40；此值僅控制「檢查前 N 條」，對應 branch 乘數。傳 40 全查）。</param>
        public static IReadOnlyList<WinResult> EvaluateAll(SymbolId[] cells, int activeLines = 40)
        {
            ValidateStorage(cells);
            for (int cell = 1; cell <= 20; cell++) ValidateSymbol(cells[cell], cell);
            var results = new List<WinResult>();
            int limit = System.Math.Clamp(activeLines, 0, SlotPaylines.All.Length);
            for (int i = 0; i < limit; i++)
            {
                var line = SlotPaylines.All[i];
                bool change = cells[line.Cells[0]] == Wild; // 第一格為 Wild → 變
                var r = EvaluateLine(line, cells, change);
                if (r.IsWin)
                {
                    results.Add(r);
                }
            }
            return results;
        }
    }
}
