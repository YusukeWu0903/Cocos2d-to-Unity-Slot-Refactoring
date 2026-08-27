using UnityEditor;
using UnityEngine;

namespace Slot.EditorTools
{
    /// <summary>
    /// 將 Resources/Sprites 下所有 PNG 設為 Sprite 類型（Unity 預設是 Default，
    /// 導致 Resources.Load&lt;Sprite&gt; 回 null、畫面全白）。
    /// Tools/Slot/Fix Import: Set Sprites
    /// </summary>
    public static class SpriteImportFixer
    {
        [MenuItem("Tools/Slot/Fix Import/Set Sprites")]
        public static void FixAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites" });
            int n = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;
                if (imp.textureType != TextureImporterType.Sprite)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.mipmapEnabled = false;
                    imp.SaveAndReimport();
                    n++;
                }
            }
            Debug.Log($"[SpriteImportFixer] {n} textures set to Sprite.");
        }
    }
}
