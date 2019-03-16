using UnityEngine;
using UnityEditor;
using Devil.ContentProvider;
using Devil.Utility;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(TextRef))]
    public class TextRefDrawer : CachedPropertyDrawer
    {
        string[] properties = { "id", "txt" };
        readonly int p_id = 0;
        readonly int p_txt = 1;

        protected override string[] GetRelativeProperties()
        {
            return properties;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ValidateProperty(property);
            return (Prop(p_txt).stringValue.IndexOf('\n') != -1 ? 80 : 17);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ValidateProperty(property);
            label = EditorGUI.BeginProperty(position, label, property);
            var lv = EditorGUI.indentLevel;

            var pos = EditorGUI.PrefixLabel(position, label);

            string txt;
            if(position.height < 20)
            {
                EditorGUI.indentLevel = 0;
                txt = EditorGUI.TextArea(new Rect(pos.x, pos.y, pos.width - 90, pos.height), Prop(p_txt).stringValue);
                DevilEditorUtility.Hint(new Rect(pos.xMax - 90, pos.y, 90, pos.height), Prop(p_id).intValue.ToString());
            }
            else
            {
                DevilEditorUtility.Hint(pos, Prop(p_id).intValue.ToString());
                txt = EditorGUI.TextArea(new Rect(position.x, position.y + 15, position.width, position.height - 15), Prop(p_txt).stringValue);
            }
            Prop(p_id).intValue = StringUtil.ToHash(txt);
            Prop(p_txt).stringValue = txt;
            
            EditorGUI.indentLevel = lv;
            EditorGUI.EndProperty();
        }
    }
}
