using Devil.AI;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(BehaviourTreeRunner), true)]
    public class BehaviourTreeRunnerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SerializedProperty obj = serializedObject.FindProperty("m_Blackboard");
            BlackboardAsset aseet = obj.objectReferenceValue as BlackboardAsset;
            if (aseet != null )
                BlackboardAssetInspector.OnBlackboardInspectorGUI(aseet);
        }
    }
}