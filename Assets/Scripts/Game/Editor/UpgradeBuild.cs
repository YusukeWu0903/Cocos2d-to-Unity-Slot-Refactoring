using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Slot.EditorTools
{
    // Separate upgrade builds preserve the historical builder and baseline output.
    public static class UpgradeBuild
    {
        [Serializable]
        private class BuildRecord
        {
            public string unityVersion, profile, output, result;
            public long totalBytes;
            public double seconds;
            public int errors, warnings;
        }

        public static void Build()
        {
            try
            {
                string profile = Argument("-slotProfile") ?? "development";
                if (profile != "development" && profile != "release")
                    throw new ArgumentException("Unknown build profile: " + profile);
                string output = Path.GetFullPath(Argument("-slotOutput") ?? throw new ArgumentException("Missing -slotOutput"));
                string allowed = Path.GetFullPath("D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/");
                if (!output.StartsWith(allowed, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Output must remain in the upgrade comparison folder.");
                if (Directory.Exists(output))
                    throw new IOException("Output already exists; choose a new versioned folder.");

                bool development = profile == "development";
                PlayerSettings.WebGL.dataCaching = false;
                PlayerSettings.WebGL.compressionFormat = development ? WebGLCompressionFormat.Disabled : WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = false;
                PlayerSettings.WebGL.nameFilesAsHashes = true;
                PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Minimal);
                PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Scenes/SlotScene.unity" },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = development ? BuildOptions.Development : BuildOptions.None
                });
                var summary = report.summary;
                File.WriteAllText(output + "-report.json", JsonUtility.ToJson(new BuildRecord
                {
                    unityVersion = Application.unityVersion, profile = profile, output = output,
                    result = summary.result.ToString(), totalBytes = (long)summary.totalSize,
                    seconds = summary.totalTime.TotalSeconds, errors = (int)summary.totalErrors,
                    warnings = (int)summary.totalWarnings
                }, true));
                if (summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + summary.result);
                Debug.Log("[UpgradeBuild] SUCCESS " + output);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        private static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}
