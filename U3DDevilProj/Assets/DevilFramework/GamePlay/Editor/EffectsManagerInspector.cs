using Devil.GamePlay.Assistant;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(EffectsManager), true)]
    public class EffectsManagerInspector : Editor
    {
        AvlTree<EffectsManager.Pool> mPool;
        SerializedProperty mPreloadAssets;

        private void OnEnable()
        {
            mPreloadAssets = serializedObject.FindProperty("m_PreloadAssets");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying)
            {
                if (mPool == null)
                {
                    bool hasfield;
                    mPool = Ref.GetField(target, "mPools", out hasfield) as AvlTree<EffectsManager.Pool>;
                }
            }
            else
            {
                mPool = null;
            }

            EditorGUILayout.BeginVertical("helpbox");
            int num = 0;
            if (mPool != null)
            {
                foreach (var pool in mPool)
                {
                    num++;
                    var str = string.Format("\"{0}\"(id:{1}) caches:{2}/{3}", pool.Name, pool.PoolId, pool.Length, pool.Capacity);
                    EditorGUILayout.SelectableLabel(str, GUILayout.Height(20));
                }
            }
            else
            {
                for (int i = 0; i < mPreloadAssets.arraySize; i++)
                {
                    var asset = mPreloadAssets.GetArrayElementAtIndex(i).objectReferenceValue as PreloadAssets;
                    if (asset == null)
                        continue;
                    for (int j = 0; j < asset.m_Assets.Count; j++)
                    {
                        var go = asset.m_Assets[j];
                        if (go == null)
                            continue;
                        var str = string.Format("\"{0}\"(id:{1}) caches:{2}/{3}", go.name, EffectsManager.StringToId(go.name), asset.m_WarmUpNum, asset.m_CacheSize);
                        EditorGUILayout.SelectableLabel(str, GUILayout.Height(20));
                        num++;
                    }
                }
            }
            if (num == 0)
                EditorGUILayout.LabelField("No Preload asset.", GUILayout.Height(30));
            EditorGUILayout.EndVertical();
        }
    }
}