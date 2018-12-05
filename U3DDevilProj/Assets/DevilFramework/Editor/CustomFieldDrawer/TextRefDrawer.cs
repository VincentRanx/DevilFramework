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
            return 75;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ValidateProperty(property);
            var pos = EditorGUI.PrefixLabel(position, label);
            var txt = EditorGUI.TextArea(new Rect(position.x, position.y + 15, position.width, position.height - 15), Prop(p_txt).stringValue);
            Prop(p_id).intValue = StringUtil.ToHash(txt);
            Prop(p_txt).stringValue = txt;
            DevilEditorUtility.Hint(pos, Prop(p_id).intValue.ToString());
        }
    }
}
