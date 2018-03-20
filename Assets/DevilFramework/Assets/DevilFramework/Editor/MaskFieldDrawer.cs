using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(MaskFieldAttribute))]
    public class MaskFieldDrawer : PropertyDrawer
    {
        StringBuilder builder = new StringBuilder();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MaskFieldAttribute range = attribute as MaskFieldAttribute;
            int lv = EditorGUI.indentLevel;
            builder.Remove(0, builder.Length);
            for (int i = 0; i < lv; i++)
            {
                builder.Append("    ");
            }
            EditorGUI.indentLevel = 0;
            builder.Append(label.text);
            label.text = builder.ToString();
            position = EditorGUI.PrefixLabel(position, label);
            MaskFieldAttribute maskattr = attribute as MaskFieldAttribute;
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                if (maskattr.IsMask)
                {
                   // int v = EditorGUI.MaskField(property.en)
                }
                else
                {
                    property.intValue = EditorGUI.MaskField(position, property.intValue, property.enumDisplayNames);
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Enum Instead of " + property.propertyType);
            }
            EditorGUI.indentLevel = lv;
        }
    }
}