using UnityEngine;
using UnityEditor;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(MaskFieldAttribute))]
    public class MaskFieldDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            int lv = EditorGUI.indentLevel;
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            var att = attribute as MaskFieldAttribute;
            if(property.propertyType == SerializedPropertyType.Integer && att.Names != null && att.Names.Length > 0)
            {
                if(att.IsToggle)
                    property.intValue = DrawGUI(position, property.intValue, att.MultiSelectable, att.Names);
                else
                    property.intValue = EditorGUI.MaskField(position, property.intValue, att.Names);
            }
            else if (property.propertyType == SerializedPropertyType.Enum)
            {
                string[] names;
                if (att.Names != null && att.Names.Length > 0)
                    names = att.Names;
                else
                    names = property.enumDisplayNames;
                if(att.IsToggle)
                    property.intValue = DrawGUI(position, property.intValue, att.MultiSelectable, names);
                else
                    property.intValue = EditorGUI.MaskField(position, property.intValue, names);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Enum Instead of " + property.propertyType);
            }
            EditorGUI.indentLevel = lv;
            EditorGUI.EndProperty();
        }

        public static int DrawGUI(Rect rect, int mask, bool multiSelectable, string[] names)
        {
            int len = Mathf.Min(names.Length, 32);
            if (len == 0)
                return mask;
            if(len == 1)
            {
                var tog = GUI.Toggle(rect,  (mask & 1) != 0, names[0], (GUIStyle)"button");
                if (tog)
                {
                    if (multiSelectable)
                        mask |= 1;
                    else
                        mask = 1;
                }
                else
                {
                    mask &= ~1;
                }
                return mask;
            }
            float w = rect.width / (float)len;
            for (int i = 0; i < len; i++)
            {
                int n = 1 << i;
                var style = i == 0 ? "ButtonLeft" : (i == len - 1 ? "ButtonRight" : "ButtonMid");
                var tog = GUI.Toggle(new Rect(rect.x + i * w, rect.y, w, rect.height),
                   (mask & n) != 0, names[i],
                    (GUIStyle)style);
                if (tog)
                {
                    if (multiSelectable)
                        mask |= n;
                    else
                        mask = n;
                }
                else
                {
                    mask &= ~n;
                }
            }
            return mask;
        }
    }
}