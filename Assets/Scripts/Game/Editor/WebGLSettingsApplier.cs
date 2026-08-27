using UnityEditor;
using UnityEngine;

namespace Slot.EditorTools
{
    public static class WebGLSettingsApplier
    {
        [MenuItem("Tools/Slot/Build/Apply Minimal Stripping")]
        public static void ApplyMinimalStripping()
        {
            // 設定 WebGL Stripping Level = Minimal
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Minimal);
            Debug.Log("[WebGLSettingsApplier] Managed Stripping Level set to Minimal.");
        }

        [MenuItem("Tools/Slot/Build/Build WebGL (Minimal Stripping)")]
        public static void BuildWebGLMinimal()
        {
            ApplyMinimalStripping();
            SlotWebGLBuilder.BuildWebGL();
        }
    }
}