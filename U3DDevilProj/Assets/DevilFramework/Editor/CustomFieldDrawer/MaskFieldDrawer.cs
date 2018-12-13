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
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            var att = attribute as MaskFieldAttribute;
            if(property.propertyType == SerializedPropertyType.Integer && att.Names != null && att.Names.Length > 0)
            {
                property.intValue = EditorGUI.MaskField(position, property.intValue, att.Names);
            }
            else if (property.propertyType == SerializedPropertyType.Enum)
            {
                string[] names;
                if (att.Names != null && att.Names.Length > 0)
                    names = att.Names;
                else
                    names = property.enumDisplayNames;
                property.intValue = EditorGUI.MaskField(position, property.intValue, names);
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