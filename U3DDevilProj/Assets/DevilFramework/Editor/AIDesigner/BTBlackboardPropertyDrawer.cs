using Devil.AI;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomPropertyDrawer(typeof(BTBlackboardPropertyAttribute))]
	public class BTBlackboardPropertyDrawer : PropertyDrawer 
	{
        bool mSelectable;
        int mSelectedIndex;
        int pid;
        GUIContent[] mPopNames;
        string mCusCache = "";

        void AllocSize(int size)
        {
            int len = mPopNames == null ? 0 : mPopNames.Length;
            if(len != size)
            {
                var content = new GUIContent[size];
                for(int i= 0; i < size; i++)
                {
                    if (i < len)
                        content[i] = mPopNames[i];
                    else
                        content[i] = new GUIContent();
                }
                mPopNames = content;
            }
        }

        void GetPopNames(SerializedProperty property)
        {
            var win = BehaviourTreeEditor.ActiveBTEditor;
            if (win == null || win.TargetRunner == null)
            {
                mSelectable = false;
                return;
            }
            var black = win.TargetRunner.BlackboardAsset;
            var len = black == null || black.m_Properties == null ? 0 : black.m_Properties.Length;
            if (len == 0)
            {
                mSelectable = false;
                return;
            }
            mSelectable = true;
            len++;
            AllocSize(len);
            mSelectedIndex = 0;
            mPopNames[0].text = "[自定义]";
            for (int i = 1; i < len; i++)
            {
                mPopNames[i].text = black.m_Properties[i - 1].m_Key;
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
            GetPopNames(property);
            return mSelectable && mSelectedIndex == 0 ? 30 : 15;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                position = EditorGUI.PrefixLabel(position, label);
                int lv = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                if (mSelectable)
                {
                    var rect = new Rect(position.x, position.y, position.width, 15);
                    var index = mSelectedIndex;
                    mSelectedIndex = EditorGUI.Popup(rect, mSelectedIndex, mPopNames);
                    if (mSelectedIndex > 0)
                    {
                        property.stringValue = mPopNames[mSelectedIndex].text;
                    }
                    else
                    {
                        if (index > 0)
                            mCusCache = "";
                        rect = new Rect(position.x, position.y + 15, position.width, 15);
                        mCusCache = EditorGUI.TextField(rect, mCusCache);
                        property.stringValue = mCusCache;
                    }
                }
                else
                {
                    property.stringValue = EditorGUI.TextField(position, property.stringValue);
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