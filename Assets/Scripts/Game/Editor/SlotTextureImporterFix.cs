using UnityEditor;
using UnityEngine;

namespace Slot.EditorTools
{
    /// <summary>
    /// 將 Resources/Sprites 下所有 PNG 匯入為 Sprite（專案非 2D 模板，預設是 Texture）。
    /// </summary>
    public static class SlotTextureImporterFix
    {
        [MenuItem("Tools/Slot/Fix/Import All Sprites As Sprite")]
        public static void FixAll()
        {
            int fixedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                    fixedCount++;
                }
            }
            Debug.Log($"[SlotTextureImporterFix] Converted {fixedCount} textures to Sprite.");
        }

        /// <summary>供 batchmode 呼叫。</summary>
        public static void FixAllBatch()
        {
            FixAll();
        }
    }
}
