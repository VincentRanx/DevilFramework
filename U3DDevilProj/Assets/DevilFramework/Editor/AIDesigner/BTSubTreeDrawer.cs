using Devil.AI;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(BTSubBehaviourTreeAttribute))]
	public class BTSubTreeDrawer : PropertyDrawer 
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            var rect = new Rect(position.x, position.y, position.width - 50, position.height);
            EditorGUI.PropertyField(rect, property);
            EditorGUI.EndDisabledGroup();

            rect = new Rect(position.xMax - 50, position.y, 50, position.height);
            var open = GUI.Button(rect, "打开");
            EditorGUI.EndProperty();
            if (open)
                BehaviourTreeEditor.OpenBTEditor(property.objectReferenceValue as BehaviourTreeAsset);
        }
    }
}