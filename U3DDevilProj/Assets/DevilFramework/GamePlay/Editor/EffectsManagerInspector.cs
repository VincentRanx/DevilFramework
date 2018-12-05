using Devil.GamePlay.Assistant;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(EffectsManager))]
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

            GUILayout.BeginVertical("box");
            if (mPool != null)
            {
                foreach (var pool in mPool)
                {
                    var str = string.Format("\"{0}\"(id:{1}) caches:{2}/{3}", pool.Name, pool.PoolId, pool.Length, pool.Capacity);
                    GUILayout.Label(str);
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
                        GUILayout.Label(str);
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }
}