using Devil.Effects;
using Devil.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using GObject = UnityEngine.Object;

    [CustomEditor(typeof(CameraPostProcessing))]
    public class CameraPostProcessingInspector : Editor
    {
        SerializedProperty m_PostEffects;
        bool drop;
        int dropIndex;
        CameraPostProcessingAsset newAsset;

        List<Type> mAssetTypes = new List<Type>();
        GUIContent[] mAssetNames;

        Dictionary<Type, Editor> assetEditors = new Dictionary<Type, Editor>();
        Editor GetEditor(GObject target)
        {
            var type = target.GetType();
            Editor editor;
            if(!assetEditors.TryGetValue(type, out editor) || editor.target != target)
            {
                editor = CreateEditor(target);
                assetEditors[type] = editor;
            }
            return editor;
        }

        private void OnEnable()
        {
            mAssetTypes.Clear();

            m_PostEffects = serializedObject.FindProperty("m_PostEffects");
            var assets = AssetDatabase.FindAssets("t:script");
            var super = typeof(CameraPostProcessingAsset);
            foreach(var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var type = script.GetClass();
                if (type != null && type.IsSubclassOf(super))
                {
                    mAssetTypes.Add(type);
                }
            }
            mAssetNames = new GUIContent[mAssetTypes.Count];
            for (int i = 0; i < mAssetTypes.Count; i++)
            {
                var effname = mAssetTypes[i].FullName;
                mAssetNames[i] = new GUIContent(StringUtil.ReplaceLast(effname, '.', '/'));
            }
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            GUILayout.Space(10);
            for (int i = 0; i < m_PostEffects.arraySize; i++)
            {
                var eff = m_PostEffects.GetArrayElementAtIndex(i);
                if(eff.objectReferenceValue == null)
                {
                    if (i == dropIndex)
                        dropIndex = -1;
                    m_PostEffects.DeleteArrayElementAtIndex(i);
                    break;
                }
                drop = dropIndex == i;
                EditorGUILayout.BeginVertical(drop ? "flow overlay box" : "box");
                int ret = QuickGUI.TitleBarWithBtn(eff.objectReferenceValue.name, 12, 15, "OL Minus", "OL Plus");
                bool over = false;
                if (ret == 0)
                {
                    dropIndex = i;
                    if (eff.objectReferenceValue != null)
                        EditorGUIUtility.PingObject(eff.objectReferenceValue);
                }
                else if (ret == 1)
                {
                    m_PostEffects.DeleteArrayElementAtIndex(i);
                    over = true;
                }
                else if (drop)
                {
                    var editor = GetEditor(eff.objectReferenceValue);
                    if (editor != null)
                        editor.OnInspectorGUI();
                }
                EditorGUILayout.EndVertical();
                if (over)
                    break;
            }
            GUILayout.Space(5);
            var rect = EditorGUILayout.BeginHorizontal();
            newAsset = EditorGUILayout.ObjectField("New Effect", newAsset, typeof(CameraPostProcessingAsset), false) as CameraPostProcessingAsset;
            bool newpop = false;
            if(GUILayout.Button("Add", GUILayout.Width(40)))
            {
                if (newAsset == null)
                {
                    rect.x = rect.xMax;
                    newpop = true;
                }
                else
                {
                    var index = m_PostEffects.arraySize;
                    dropIndex = index;
                    m_PostEffects.InsertArrayElementAtIndex(index);
                    m_PostEffects.GetArrayElementAtIndex(index).objectReferenceValue = newAsset;
                    newAsset = null;
                }
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            if(newpop)
                EditorUtility.DisplayCustomMenu(rect, mAssetNames, -1, OnAddEffectSelected, null);
        }

        private void OnAddEffectSelected(object userData, string[] options, int selected)
        {
            var inst = ScriptableObject.CreateInstance(mAssetTypes[selected]);
            inst.name = mAssetTypes[selected].Name;
            var index = m_PostEffects.arraySize;
            dropIndex = index;
            m_PostEffects.InsertArrayElementAtIndex(index);
            m_PostEffects.GetArrayElementAtIndex(index).objectReferenceValue = inst;
            serializedObject.ApplyModifiedProperties();
        }
    }
}