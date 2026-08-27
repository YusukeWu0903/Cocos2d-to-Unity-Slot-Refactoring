using UnityEngine;
using UnityEngine.UI;
using Slot.Core;

namespace Slot.Game
{
    /// <summary>單一轉輪格子：顯示一個符號 Sprite。</summary>
    public class ReelCell : MonoBehaviour
    {
        [SerializeField] private Image icon;

        private static Sprite[] fruitSprites;

        /// <summary>載入 Resources 下的水果 sprite（icon_fruitbonus_1..10）。</summary>
        public static Sprite GetFruitSprite(SymbolId s)
        {
            if (fruitSprites == null)
            {
                fruitSprites = new Sprite[11];
                for (int i = 1; i <= 10; i++)
                {
                    fruitSprites[i] = Resources.Load<Sprite>(
                        $"Sprites/fruits/icon_fruitbonus_{i}");
                }
            }
            return fruitSprites[(int)s];
        }

        public void SetSymbol(SymbolId s)
        {
            var sp = GetFruitSprite(s);
            if (sp != null) icon.sprite = sp;
        }
    }
}