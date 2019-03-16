using Devil.AI;
using Devil.Utility;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(BlackboardAsset.VariableDefine))]
    public class BlackboardVariableDrawer : CachedPropertyDrawer
    {
        string[] props = {"name", "comment", "typeDef", "isList" };
        readonly int p_name = 0;
        readonly int p_comment = 1;
        readonly int p_type = 2;
        readonly int p_lst = 3;

        readonly string comHint = "Edit Comment...";
        readonly string[] dtType = { "Data Type", "List Type"};

        static SerializedProperty sEditTarget;

        protected override string[] GetRelativeProperties()
        {
            return props;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 65;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ValidateProperty(property);
            label = EditorGUI.BeginProperty(position, label, property);

            var lv = EditorGUI.indentLevel;
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUI.LabelField(position, DevilEditorUtility.EmptyContent, (GUIStyle)"helpbox");
            var row = new Rect(position.x + 3, position.y + 5, position.width - 6, 18);
            //EditorGUI.LabelField(row, DevilEditorUtility.EmptyContent, "helpbox");
            var s = Prop(p_comment).stringValue;
            s = EditorGUI.TextField(row, string.IsNullOrEmpty(s) ? comHint : s, DevilEditorUtility.HintStyle("label"));
            Prop(p_comment).stringValue = s == comHint ? "" : s;

            EditorGUI.indentLevel = lv;
            label.text = " Varialbe Name";
            row.y += 20;
            var pos = EditorGUI.PrefixLabel(row, label);
            float size = pos.x - row.x;
            EditorGUI.indentLevel = 0;
            Prop(p_name).stringValue = EditorGUI.TextField(pos, Prop(p_name).stringValue);

            EditorGUI.indentLevel = lv;
            row.y += 20;
            //label.text = " Data Type";
            pos = new Rect(row.x, row.y, size - 5, row.height);
            Prop(p_lst).boolValue = EditorGUI.Popup(pos, Prop(p_lst).boolValue ? 1 : 0, dtType, (GUIStyle)"ShurikenPopup") != 0;
            //pos = EditorGUI.PrefixLabel(row, label);
            pos = new Rect(row.x + size, row.y, row.width - size, row.height);

            EditorGUI.indentLevel = 0;
            var sel = GlobalUtil.FindIndex(AIModules.SharedTypeNames, (x) => x == Prop(p_type).stringValue);
            if (sel == -1)
                sel = 0;
            //pos.width -= 20;
            sel = EditorGUI.Popup(pos, sel, AIModules.SharedTypeNames, (GUIStyle)"ExposablePopupMenu"); 
            Prop(p_type).stringValue = AIModules.SharedTypeNames[sel];
            //pos = new Rect(pos.xMax + 3, pos.y, 15, 15);
            //Prop(p_lst).boolValue = EditorGUI.Toggle(pos, Prop(p_lst).boolValue, (GUIStyle)"MuteToggle");

            EditorGUI.indentLevel = lv;
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }
}