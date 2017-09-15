using UnityEngine;
using UnityEditor;
using System.IO;
using DevilTeam.Utility;

namespace DevilTeam.Editor
{
    public class CubemapCaptureWindow : EditorWindow
    {
        public enum Resolution
        {
            _128x128 = 7,
            _256x256 = 8,
            _512x512 = 9,
            _1024x1024 = 10,
            _2048x2048 = 11,
            _4096x4096 = 12,
        }

        [MenuItem("Devil Framework/Cubemap Capture")]
        public static void OpenThis()
        {
            CubemapCaptureWindow win = GetWindow<CubemapCaptureWindow>();
            win.name = "Cubemap Capture";
            win.Show();
        }

        bool mEnabled;
        Camera mCam;
        Texture mTex;
        Transform mLocal;
        Vector3 mPos;
        GUIStyle mStyle;
        Resolution mResolution = Resolution._512x512;
        string mPath = "TempAssets";
        string mFileName = "Cubemap";

        private void OnEnable()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            if (!mTex)
                mTex = AssetDatabase.LoadAssetAtPath<Texture>("Assets/TempAssets/Textures/cubeIcon.png");
            mStyle = new GUIStyle();
            mStyle.richText = true;
            mStyle.fontSize = 20;
            mEnabled = true;
        }

        private void OnDisable()
        {
            mEnabled = false;
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        private void OnFocus()
        {
            if (!mCam)
            {
                mCam = Camera.main;
            }
        }

        private void OnGUI()
        {
            mCam = EditorGUILayout.ObjectField("Current Camera", mCam, typeof(Camera), true) as Camera;
            mPath = EditorGUILayout.TextField("Relative Path", mPath);
            mFileName = EditorGUILayout.TextField("File Name", mFileName);
            mLocal = EditorGUILayout.ObjectField("View Root", mLocal, typeof(Transform), true) as Transform;
            mResolution = (Resolution)EditorGUILayout.EnumPopup("Resolution", mResolution);
            if (GUILayout.Button("Create Cubemap"))
            {
                if (!mCam)
                {
                    EditorUtility.DisplayDialog("Error", "Camera is invalid.", "OK");
                }
                if (string.IsNullOrEmpty(mPath) || string.IsNullOrEmpty(mFileName))
                {
                    EditorUtility.DisplayDialog("Error", "NAME or PATH is not setted.", "OK");
                }
                else
                {
                    string path = mPath;
                    if (!path.EndsWith("/"))
                        path += "/";
                    if (path.StartsWith("/"))
                        path = path.Substring(1);
                    string fullPath = Application.dataPath + "/" + path;
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);
                    string fname = mFileName;
                    if (!fname.EndsWith(".cubemap"))
                        fname += ".cubemap";
                    fullPath += fname;
                    Vector3 camPos = mCam.transform.position;
                    mCam.transform.position = mLocal ? mLocal.transform.position : mPos;
                    Cubemap cube = new Cubemap(1 << (int)mResolution, TextureFormat.ARGB32, false);
                    mCam.RenderToCubemap(cube);
                    mCam.transform.position = camPos;
                    AssetDatabase.CreateAsset(cube, "Assets/" + path + fname);
                    AssetDatabase.Refresh();
                }
            }


        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!mEnabled || mLocal)
                return;
            mPos = sceneView.camera.transform.position;
            //mPos = Handles.PositionHandle(mPos, Quaternion.identity);
            if (mTex)
            {
                Vector3 right = sceneView.camera.transform.right;
                Vector3 up = sceneView.camera.transform.up;
                right *= mTex.width * 0.5f * GizmosUtil.FactorFromSceneViewPixel(null, mPos);
                up *= mTex.height * 0.5f * GizmosUtil.FactorFromSceneViewPixel(null, mPos);

                Handles.Label(mPos - right + up, mTex);
            }
            else
            {
                Color c = Color.yellow;
                c.a = 0.5f;
                Handles.color = c;
                //Handles.SphereHandleCap(1, mPos, Quaternion.identity, 30 * GizmosUtil.FactorFromSceneViewPixel(null, mPos),EventType.repaint);
                Handles.Label(mPos, "<b>Cubemap Center</b>", mStyle);
            }
        }

    }
}