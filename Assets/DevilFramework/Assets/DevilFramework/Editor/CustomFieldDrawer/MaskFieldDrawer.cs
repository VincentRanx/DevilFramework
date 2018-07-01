using UnityEngine;
using UnityEditor;
using System.Text;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(MaskFieldAttribute))]
    public class MaskFieldDrawer : PropertyDrawer
    {
        //StringBuilder builder = new StringBuilder();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            int lv = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            //builder.Remove(0, builder.Length);
            //for (int i = 0; i < lv; i++)
            //{
            //    builder.Append("    ");
            //}
            //builder.Append(label.text);
            //label.text = builder.ToString();
            position = EditorGUI.PrefixLabel(position, label);
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                property.intValue = EditorGUI.MaskField(position, property.intValue, property.enumDisplayNames);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Enum Instead of " + property.propertyType);
            }
            EditorGUI.indentLevel = lv;
            EditorGUI.EndProperty();
        }
    }
}