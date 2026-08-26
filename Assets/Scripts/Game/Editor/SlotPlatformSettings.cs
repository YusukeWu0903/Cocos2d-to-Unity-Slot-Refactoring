using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Slot.EditorTools
{
    /// <summary>
    /// Phase 0：集中設定 Player Settings（解析度 960×640、橫向、WebGL 解析度、產品名）。
    /// 從選單執行: Tools/Slot/Phase0/Apply Platform Settings
    /// </summary>
    public static class SlotPlatformSettings
    {
        [MenuItem("Tools/Slot/Phase0/Apply Platform Settings")]
        public static void Apply()
        {
            // 產品識別
            PlayerSettings.productName = "SlotUnity";
            PlayerSettings.companyName = "SlotUnity";

            // 橫向
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // 桌面 / 各平台預設解析度（960×640，對應原始 cocos2d 設計解析度）
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 640;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

            // 相容性：以最低目標（WebGL 對 2D 完全綽綽有餘）
            PlayerSettings.SetApiCompatibilityLevel(
                BuildTargetGroup.WebGL,
                ApiCompatibilityLevel.NET_Standard);

            AssetDatabase.SaveAssets();
            Debug.Log("[SlotPlatformSettings] Applied: 960x640, landscape, WebGL+Standalone friendly");
        }
    }
}
