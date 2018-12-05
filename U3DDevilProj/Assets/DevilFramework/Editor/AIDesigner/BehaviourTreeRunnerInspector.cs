using Devil.AI;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(BehaviourTreeRunner), true)]
    public class BehaviourTreeRunnerInspector : Editor
    {
        SerializedProperty m_Blackboard;

        private void OnEnable()
        {
            m_Blackboard = serializedObject.FindProperty("m_Blackboard");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            BlackboardAsset aseet = m_Blackboard.objectReferenceValue as BlackboardAsset;
            if (aseet != null )
                BlackboardAssetInspector.OnBlackboardInspectorGUI(aseet);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(40);
            if (GUILayout.Button("打开编辑器", "LargeButton"))
            {
                BehaviourTreeEditor.OpenBTEditor(target as BehaviourTreeRunner);
            }
            GUILayout.Space(40);
            EditorGUILayout.EndHorizontal();
        }
    }
}