using UnityEngine;
using UnityEditor;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(RangeFieldAttribute))]
    public class RangeFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            int lv = EditorGUI.indentLevel;
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            var att = attribute as RangeFieldAttribute;

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                var ts = 27f;
                float w = Mathf.Min(50, (position.width - ts * 2) * 0.5f - 10);
                float sp = position.width - w * 2 - ts * 2;

                EditorGUI.LabelField(new Rect(position.center.x - sp * 0.5f + 2, position.y, sp - 4, position.height), DevilEditorUtility.EmptyContent, (GUIStyle)"horizontalslider");
                var v = property.vector2Value;
                v.y = Mathf.Min(v.y, att.Max);
                v.x = Mathf.Max(v.x, att.Min);

                EditorGUI.LabelField(new Rect(position.x, position.y, ts, position.height), "Min");
                EditorGUI.LabelField(new Rect(position.xMax - ts, position.y, ts, position.height), "Max");
                v.x = Mathf.Clamp(EditorGUI.FloatField(new Rect(position.x + ts, position.y, w, position.height), v.x), att.Min, v.y );
                v.y = Mathf.Clamp(EditorGUI.FloatField(new Rect(position.xMax - w - ts, position.y, w, position.height), v.y), v.x, att.Max);
                property.vector2Value = v;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Vector2 Instead of " + property.propertyType);
            }
            EditorGUI.indentLevel = lv;
            EditorGUI.EndProperty();
        }
        
    }
}