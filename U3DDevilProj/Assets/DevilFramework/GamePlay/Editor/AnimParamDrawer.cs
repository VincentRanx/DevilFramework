using Devil.GamePlay;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(AnimParam))]
    public class AnimParamDrawer : CachedPropertyDrawer
    {
        string[] props = { "_name", "_id", "type", "_value" };
        readonly int p_name = 0;
        readonly int p_id = 1;
        readonly int p_type = 2;
        readonly int p_value = 3;
        
        static Animator sAnim;
        static List<AnimParam> sList = new List<AnimParam>();
        static string[] sNames;

        static void GetNames()
        {
            sNames = new string[sList.Count + 1];
            sNames[0] = "[NONE]";
            for (int i = 0; i < sList.Count; i++)
            {
                sNames[i + 1] = string.Format("[{0}] {1}", sList[i].type, sList[i].name);
            }
        }

        static void GetParams(Animator anim)
        {
            sAnim = anim;
            sList.Clear();
            if (anim != null)
            {
                foreach (var t in anim.parameters)
                {
                    sList.Add(AnimParam.ReferenceTo(t));
                }
            }
            GetNames();
        }

        void FindAnim()
        {
            var t = property.serializedObject.targetObject as Component;
            if (t != null && t.gameObject.activeInHierarchy)
            {
                sAnim = t.GetComponent<Animator>();
                if (sAnim == null)
                    sAnim = t.GetComponentInParent<Animator>();
            }
            else
            {
                sAnim = null;
            }
            GetParams(sAnim);
        }

        protected override void OnValidateProperty()
        {
            base.OnValidateProperty();
            FindAnim();
        }

        protected override string[] GetRelativeProperties()
        {
            return props;
        }

        int FindParamIndex(int id)
        {
            for (int i = 0; i < sList.Count; i++)
            {
                if (sList[i].id == id)
                    return i + 1;
            }
            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ValidateProperty(property);
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            var lv = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            if (sNames != null && sNames.Length > 1)
            {
                var w2 = position.width - 15f;
                var type = (AnimatorControllerParameterType)Prop(p_type).intValue;
                if (type == AnimatorControllerParameterType.Bool)
                    w2 -= 16f;
                else if (type == AnimatorControllerParameterType.Float || type == AnimatorControllerParameterType.Int)
                    w2 = Mathf.Max(w2 * 0.5f, w2 - 60);
                var w3 = position.width - 15 - w2;

                var index = EditorGUI.Popup(new Rect(position.x, position.y, w2, position.height), FindParamIndex(Prop(p_id).intValue), sNames);
                if (index > 0)
                {
                    Prop(p_name).stringValue = sList[index - 1].name;
                    Prop(p_id).intValue = sList[index - 1].id;
                    Prop(p_type).intValue = (int)sList[index - 1].type;
                }
                else
                {
                    Prop(p_name).stringValue = "";
                    Prop(p_id).intValue = 0;
                    Prop(p_type).intValue = 0;
                }
                if (type == AnimatorControllerParameterType.Bool)
                    Prop(p_value).floatValue = EditorGUI.Toggle(new Rect(position.x + w2, position.y, w3, position.height), Prop(p_value).floatValue > 0.2f) ? 1 : 0;
                else if (type == AnimatorControllerParameterType.Float)
                    Prop(p_value).floatValue = DevilEditorUtility.FloatField(new Rect(position.x + w2, position.y, w3, position.height), "value", Prop(p_value).floatValue);
                else if (type == AnimatorControllerParameterType.Int)
                    Prop(p_value).floatValue = DevilEditorUtility.IntField(new Rect(position.x + w2, position.y, w3, position.height), (int)Prop(p_value).floatValue, "value");
            }
            else
            {
                var w = position.width - 15;
                w = Mathf.Min(w * 0.5f, 60);
                var type = (AnimatorControllerParameterType)EditorGUI.EnumPopup(new Rect(position.x, position.y, w, position.height),
                    (AnimatorControllerParameterType)Prop(p_type).intValue);
                Prop(p_type).intValue = (int)type;

                var w2 = position.width - w - 15;
                if (type == AnimatorControllerParameterType.Bool)
                    w2 -= 16f;
                else if (type == AnimatorControllerParameterType.Float || type == AnimatorControllerParameterType.Int)
                    w2 = Mathf.Max(w2 * 0.5f, w2 - 60);
                var w3 = position.width - 15 - w - w2;

                Prop(p_name).stringValue = DevilEditorUtility.TextField(new Rect(position.x + w, position.y, w2, position.height), Prop(p_name).stringValue, "param name");
                Prop(p_id).intValue = Animator.StringToHash(Prop(p_name).stringValue);
                if (type == AnimatorControllerParameterType.Bool)
                    Prop(p_value).floatValue = EditorGUI.Toggle(new Rect(position.x + w + w2, position.y, w3, position.height), Prop(p_value).floatValue > 0.2f) ? 1 : 0;
                else if (type == AnimatorControllerParameterType.Float)
                    Prop(p_value).floatValue = DevilEditorUtility.FloatField(new Rect(position.x + w + w2, position.y, w3, position.height), "value", Prop(p_value).floatValue);
                else if (type == AnimatorControllerParameterType.Int)
                    Prop(p_value).floatValue = DevilEditorUtility.IntField(new Rect(position.x + w + w2, position.y, w3, position.height), (int)Prop(p_value).floatValue, "value");
            }
            if(GUI.Button(new Rect(position.xMax - 15, position.y, 15,15), DevilEditorUtility.EmptyContent, "Icon.ExtrapolationPingPong"))
            {
                FindAnim();
            }
            EditorGUI.indentLevel = lv;
            EditorGUI.EndProperty();
            
        }
    }
}