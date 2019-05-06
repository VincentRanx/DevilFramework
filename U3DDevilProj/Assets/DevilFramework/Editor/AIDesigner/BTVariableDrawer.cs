using Devil.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(BTVariableAttribute))]
	public class BTVariableDrawer : CachedPropertyDrawer 
	{
        int mSelectedIndex;
        int pid;
        GUIContent[] mPopNames;
        string mCusCache = "";
        List<BlackboardAsset.VariableDefine> mVars = new List<BlackboardAsset.VariableDefine>();

        string[] pnames = {"varName", "varValue" };
        int p_name = 0;
        int p_value = 1;

        protected override string[] GetRelativeProperties()
        {
            var attr = attribute as BTVariableAttribute;
            pnames[0] = attr.VarName;
            pnames[1] = attr.VarValue;
            return pnames;
        }

        void AllocSize(int size)
        {
            int len = mPopNames == null ? 0 : mPopNames.Length;
            if(len != size)
            {
                var content = new GUIContent[size]; 
                for (int i= 0; i < size; i++)
                {
                    if (i < len)
                        content[i] = mPopNames[i];
                    else
                        content[i] = new GUIContent();
                }
                mPopNames = content;
            }
        }

        void GetPopNames(SerializedProperty property, BTVariableAttribute attr)
        {
            mVars.Clear();
            var black = BehaviourTreeEditor.UsingBlackboard;
            if (attr != null && black != null)
            {
                for (int i = 0; i < black.Length; i++)
                {
                    var v = black[i];
                    if (attr.VarClass != null)
                    {
                        if (attr.VarClass.FullName != v.typeDef)
                            continue;
                        if (attr.VarType == EVarType.List && !v.isList)
                            continue;
                        if (attr.VarType == EVarType.Variable && v.isList)
                            continue;
                    }
                    mVars.Add(v);
                }
            }
            var len = mVars.Count + 1;
            AllocSize(len);
            mSelectedIndex = 0;
            if (attr.VarClass == null)
                mPopNames[0].text = "[自定义]";
            else
                mPopNames[0].text = string.Format("[自定义 {0} {1}]", attr.VarType, attr.VarClass.Name);
            for (int i = 1; i < len; i++)
            {
                mPopNames[i].text = mVars[i - 1].name;
                if (Prop(p_name).stringValue == mPopNames[i].text)
                    mSelectedIndex = i;
            }
            var id = property.serializedObject.targetObject.GetInstanceID();
            if (id != pid)
            {
                pid = id;
                mCusCache = Prop(p_name).stringValue;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Generic)
                return 20;
            ValidateProperty(property);
            GetPopNames(property, attribute as BTVariableAttribute);
            return EditorGUI.GetPropertyHeight(Prop(p_value)) + 40;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ValidateProperty(property);
            label = EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                var pos = position;
                EditorGUI.LabelField(pos, DevilEditorUtility.EmptyContent, (GUIStyle)"helpbox");
               
                int lv = EditorGUI.indentLevel;
                
                var h = pos.height - 40;
                EditorGUI.indentLevel++;
                var rect = new Rect(pos.x, pos.y + 3, pos.width, h);
                EditorGUI.PropertyField(rect, Prop(p_value), label, true);

                rect = new Rect(pos.x, pos.y + h + 3, pos.width, pos.height - h);
                label.text = " Blackboard Variable";
                position = EditorGUI.PrefixLabel(rect, label);
                EditorGUI.indentLevel = 0;

                rect = new Rect(position.x, position.y, position.width , 18);
                var index = mSelectedIndex;
                mSelectedIndex = EditorGUI.Popup(rect, mSelectedIndex, mPopNames, (GUIStyle)"ExposablePopupMenu");
                if (mSelectedIndex > 0)
                {
                    Prop(p_name).stringValue = mPopNames[mSelectedIndex].text;
                    rect = new Rect(position.x, position.y + 18, position.width, 20);
                    EditorGUI.LabelField(rect, mVars[mSelectedIndex - 1].comment, DevilEditorUtility.HintStyle("label"));
                }
                else
                {
                    if (index > 0)
                        mCusCache = "";
                    rect = new Rect(position.x, position.y + 18, position.width, 18);
                    mCusCache = EditorGUI.TextField(rect, mCusCache, (GUIStyle)"helpbox");
                    Prop(p_name).stringValue = mCusCache;
                }

                EditorGUI.indentLevel = lv;

            }
            else
            {
                EditorGUI.PropertyField(position, property);
            }
            //TextFieldDropDown TextFieldDropDownText
            EditorGUI.EndProperty();
        }
    }
}