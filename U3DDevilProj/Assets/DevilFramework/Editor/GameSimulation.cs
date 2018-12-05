using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevilEditor
{
    public class GameSimulation
    {
        static bool IsSimulating;
        static bool IsReadyForSimulation;

        [InitializeOnLoadMethod]
        private static void OnInstallize()
        {
            IsReadyForSimulation = true;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("Devil Framework/Simulation Play", true)]
        private static bool CanSimulatePlay()
        {
            return IsReadyForSimulation && !IsSimulating && !EditorApplication.isPlaying;
        }

        [MenuItem("Devil Framework/Simulation Play")]
        public static void SimulatePlay()
        {
            if (IsSimulating || EditorApplication.isPlaying)
                return;
            EditorBuildSettingsScene[] scene = EditorBuildSettings.scenes;
            if (scene == null || scene.Length == 0)
            {
                EditorUtility.DisplayDialog("Simulation Error", "无法模拟运行，请确保已经将场景加入到打包列表", "确定");
                return;
            }
            IsSimulating = true;
            EditorPrefs.SetBool("simulate", true);
            IsReadyForSimulation = false;
            var current = EditorSceneManager.GetActiveScene().path;
            Debug.Log("Simulate Scene " + scene[0].path);
            EditorApplication.isPlaying = true;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool("simulate"))
                {
                    IsSimulating = true;
                    var current = SceneManager.GetActiveScene();
                    var scene = EditorBuildSettings.scenes[0];
                    if (scene.path != current.path)
                    {
                        SceneManager.LoadScene(0);
                    }
                }
            }
            else if (obj == PlayModeStateChange.EnteredEditMode)
            {
                EditorPrefs.SetBool("simulate", false);
                IsSimulating = false;
            }
        }

    }
}