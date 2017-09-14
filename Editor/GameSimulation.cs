using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevilTeam.Editor
{
    public class GameSimulation
    {
        public static string SceneBeforePlay { get; private set; }
        public static bool IsSimulating { get; private set; }
        public static bool IsReadyForSimulation { get; private set; }

        [InitializeOnLoadMethod]
        private static void OnInstallize()
        {
            IsReadyForSimulation = true;
            SceneBeforePlay = EditorSceneManager.GetActiveScene().path;
        }

        [MenuItem("Devil Framework/Simulation/Play", true)]
        private static bool CanSimulatePlay()
        {
            return IsReadyForSimulation && !IsSimulating && !EditorApplication.isPlaying;
        }

        [MenuItem("Devil Framework/Simulation/Play")]
        public static void SimulatePlay()
        {
            if (IsSimulating || EditorApplication.isPlaying)
                return;
            EditorBuildSettingsScene[] scene = EditorBuildSettings.scenes;
            if(scene == null || scene.Length == 0)
            {
                EditorUtility.DisplayDialog("Simulation Error", "无法模拟运行，请确保已经将场景加入到打包列表", "确定");
                return;
            }
            IsSimulating = true;
            IsReadyForSimulation = false;
            SceneBeforePlay = EditorSceneManager.GetActiveScene().path;
            Debug.Log("Simulate Scene " + scene[0].path);
            if(scene[0].path == SceneBeforePlay)
            {
                EditorApplication.isPlaying = true;
                EditorApplication.update += Update;
            }
            else
            {
                EditorSceneManager.sceneOpened += OnOpenSceneAndPlay;
                EditorSceneManager.OpenScene(scene[0].path);
            }
        }

        [MenuItem("Devil Framework/Simulation/Stop", true)]
        public static bool CanStopPlay()
        {
            return EditorApplication.isPlaying;
        }

        [MenuItem("Devil Framework/Simulation/Stop")]
        public static void StopPlay()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.isPlaying = false;
        }

        static void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= Update;
                OnStopSimulation();
            }
        }

        static void OnOpenSceneAndPlay(Scene scene, OpenSceneMode mode)
        {
            EditorApplication.isPlaying = true;
            EditorApplication.update += Update;
            EditorSceneManager.sceneOpened -= OnOpenSceneAndPlay;
        }

        static void OnOpenSceneAndStopPlay(Scene scene, OpenSceneMode mode)
        {
            EditorSceneManager.sceneOpened -= OnOpenSceneAndStopPlay;
            IsReadyForSimulation = true;
        }

        static void OnStopSimulation()
        {
            if (IsSimulating)
            {
                IsSimulating = false;
                if (EditorSceneManager.GetActiveScene().path != SceneBeforePlay)
                {
                    EditorSceneManager.sceneOpened += OnOpenSceneAndStopPlay;
                    EditorSceneManager.OpenScene(SceneBeforePlay);
                }
                else
                {
                    IsReadyForSimulation = true;
                }
            }
        }
    }
}