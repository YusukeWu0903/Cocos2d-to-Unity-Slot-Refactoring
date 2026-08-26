using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Slot.EditorTools
{
    /// <summary>
    /// Phase 0：WebGL 建置。從 Menu 或 -executeMethod 呼叫。
    /// 輸出到 Build/WebGL，產出 index.html。
    /// </summary>
    public static class SlotWebGLBuilder
    {
        public const string ScenesRoot = "Assets/Scenes";

        [MenuItem("Tools/Slot/Build/Build WebGL")]
        public static void BuildWebGL()
        {
            string outDir = Path.Combine(Directory.GetCurrentDirectory(), "Build", "WebGL");
            Directory.CreateDirectory(outDir);

            var scenes = new string[]
            {
                "Assets/Scenes/SlotScene.unity",
            };

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[SlotWebGLBuilder] SUCCESS -> {outDir} (size {report.summary.totalSize})");
            }
            else
            {
                Debug.LogError($"[SlotWebGLBuilder] FAILED: {report.summary.result}");
            }
        }
    }
}