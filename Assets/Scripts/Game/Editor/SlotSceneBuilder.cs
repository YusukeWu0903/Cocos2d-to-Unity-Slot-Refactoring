using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slot.EditorTools
{
    /// <summary>
    /// Phase 0：以程式建立最小可視場景 SlotScene（Camera + Root[SlotBootstrap]）。
    /// 從選單執行: Tools/Slot/Phase0/Build Minimal Scene
    /// </summary>
    public static class SlotSceneBuilder
    {
        [MenuItem("Tools/Slot/Phase0/Build Minimal Scene")]
        public static void Build()
        {
            string scenePath = "Assets/Scenes/SlotScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.09f, 0.12f);
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 0, -10);

            // Root (SlotBootstrap)
            var rootGo = new GameObject("SlotRoot");
            rootGo.AddComponent<Slot.Game.SlotBootstrap>();

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SlotSceneBuilder] Saved: {scenePath}");
        }
    }
}
