using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Slot.EditorTools
{
    /// <summary>
    /// Phase 0：WebGL 建置。從 Menu 或 -executeMethod 呼叫。
    /// 輸出到 Build/WebGL2，產出 index.html。
    /// </summary>
    public static class SlotWebGLBuilder
    {
        public const string ScenesRoot = "Assets/Scenes";

        [MenuItem("Tools/Slot/Build/Build WebGL")]
        public static void BuildWebGL()
        {
            // 用新輸出目錄釋出，避免舊 build 資源（loader/framework/wasm 時間戳不一致）造成 wasm 崩潰
            string outDir = Path.Combine(Directory.GetCurrentDirectory(), "Build", "WebGL2");

            // ✅ 每次建置前徹底清除舊目錄，確保乾淨乾淨
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }

            // ✅ 自動清理 IL2CPP 編譯快取（避免舊編譯快取干擾）
            CleanIl2cppCache();

            Directory.CreateDirectory(outDir);

            var scenes = new string[]
            {
                "Assets/Scenes/SlotScene.unity",
            };

            // ✅ 強制關閉 WebGL Data Caching（避免 IndexedDB 舊快取干擾）
            PlayerSettings.WebGL.dataCaching = false;

            // ✅ 強制 Stripping Level = Minimal（避免 High/Low 誤刪必要函式導致 null function）
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Minimal);

            // ✅ 強制開啟完整例外支援與開發者建置（暴露真正的 C# 例外，如 NullReferenceException）
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.Development, // ✅ 強制開發者建置（啟用完整例外堆疊）
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

        private static void CleanIl2cppCache()
        {
            string projectPath = Directory.GetCurrentDirectory();
            string[] cacheDirs = {
                Path.Combine(projectPath, "Library", "Bee"),
                Path.Combine(projectPath, "Library", "BuildCache")
            };

            foreach (string dir in cacheDirs)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        Debug.Log($"[SlotWebGLBuilder] Cleaned IL2CPP cache: {dir}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[SlotWebGLBuilder] Failed to clean {dir}: {e.Message}");
                    }
                }
            }
        }
    }
}