using Devil.AI;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(BehaviourTreeAsset))]
    public class BehaviourTreeAssetInspector : Editor
    {
        private void OnEnable()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("打开编辑器", "LargeButton"))
            {
                BehaviourTreeDesignerWindow.OpenEditor(target as BehaviourTreeAsset);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}