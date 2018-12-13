using Devil.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(BTVariableReferenceAttribute))]
	public class BTVariableReferenceDrawer : PropertyDrawer 
	{
        int mSelectedIndex;
        int pid;
        GUIContent[] mPopNames;
        string mCusCache = "";
        List<BlackboardAsset.VariableDefine> mVars = new List<BlackboardAsset.VariableDefine>();

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

        void GetPopNames(SerializedProperty property, BTVariableReferenceAttribute attr)
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
                if (property.stringValue == mPopNames[i].text)
                    mSelectedIndex = i;
            }
            var id = property.serializedObject.targetObject.GetInstanceID();
            if (id != pid)
            {
                pid = id;
                mCusCache = property.stringValue;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return 20;
            GetPopNames(property, attribute as BTVariableReferenceAttribute);
            return 40;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                position = EditorGUI.PrefixLabel(position, label);
                int lv = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;


                var rect = new Rect(position.x, position.y, position.width, 15);
                var index = mSelectedIndex;
                mSelectedIndex = EditorGUI.Popup(rect, mSelectedIndex, mPopNames);
                if (mSelectedIndex > 0)
                {
                    property.stringValue = mPopNames[mSelectedIndex].text;
                    rect = new Rect(position.x, position.y + 20, position.width, 18);
                    EditorGUI.LabelField(rect, mVars[mSelectedIndex - 1].comment, DevilEditorUtility.HintStyle("label"));
                }
                else
                {
                    if (index > 0)
                        mCusCache = "";
                    rect = new Rect(position.x, position.y + 18, position.width, 20);
                    mCusCache = EditorGUI.TextField(rect, mCusCache);
                    property.stringValue = mCusCache;
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