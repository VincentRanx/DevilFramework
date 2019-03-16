using Devil.AI;
using Devil.Utility;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(BlackboardAsset))]
    public class BlackboardEditor : Editor
    {
        SerializedProperty m_Properties;
        int editIndex = -1;
        BlackboardAsset mAsset;

        readonly string comment = "Edit Comment...";

        string[] contents;
        ReorderableList propertyList;
        bool submitEdit;
        
        int raycastIndex;
        
        void UpdateContents()
        {
            contents = new string[mAsset.Length];
            for(int i= 0; i < mAsset.Length; i++)
            {
                var p = mAsset[i];
                contents[i] = GetDescript(p);
            }
        }

        string GetDescript(BlackboardAsset.VariableDefine pdef)
        {
            return StringUtil.Concat(
                "<i><color=#606060>",
                    string.IsNullOrEmpty(pdef.comment) ? pdef.name : pdef.comment,
                    "</color></i>\n",
                    "<b>", pdef.name, "</b><color=#4040f0>  :",
                    pdef.typeDef,
                    pdef.isList ? "[]</color>" : "</color>"
                    );
        }

        private void OnEnable()
        {
            m_Properties = serializedObject.FindProperty("m_Properties");
            mAsset = target as BlackboardAsset;
            editIndex = -1;
            submitEdit = false;
            UpdateContents();
            propertyList = new ReorderableList(serializedObject, m_Properties, true, true, true, true);
            propertyList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Blackboard Propreties");
            };
            propertyList.elementHeightCallback = (index) => index == editIndex ? 65 : 40;
            propertyList.drawElementBackgroundCallback = DrawBackgorund;
            propertyList.drawElementCallback = DrawElement;
            propertyList.onReorderCallback = (x) => { UpdateContents(); };
            propertyList.onAddCallback = (x) => {
                m_Properties.arraySize++;
                editIndex = m_Properties.arraySize - 1;
                var newit = m_Properties.GetArrayElementAtIndex(editIndex);
                newit.FindPropertyRelative("comment").stringValue = "";
                //propertyList.draggable = false;
                propertyList.index = editIndex;
                submitEdit = false;
                UpdateContents();
            };
            propertyList.onRemoveCallback = (x) => 
            {
                if(propertyList.index >= 0 && propertyList.index < m_Properties.arraySize)
                    m_Properties.DeleteArrayElementAtIndex(propertyList.index);
                if (editIndex != -1)
                    submitEdit = true;
            };
        }

        void DrawBackgorund(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.Contains(Event.current.mousePosition))
                raycastIndex = index;
            if (isActive && index != editIndex)
                GUI.Label(new Rect(rect.x + 5, rect.y, rect.width - 10, rect.height), DevilEditorUtility.EmptyContent, "AC BoldHeader");
        }

        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index == editIndex)
            {
                EditorGUI.PropertyField(rect, m_Properties.GetArrayElementAtIndex(index));
            }
            else if(contents!= null && index < contents.Length)
            {
                GUI.Label(new Rect(rect.x, rect.y + 3, rect.width, rect.height - 5), contents[index]);
            }
        }


        public override void OnInspectorGUI()
        {
            if (contents == null || contents.Length != m_Properties.arraySize)
            {
                UpdateContents();
            }
            GUI.skin.label.richText = true;
            raycastIndex = -1;
            serializedObject.Update();
            propertyList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            bool repaint = false;

            if(editIndex != -1 &&  Event.current.type == EventType.ContextClick)
            {
                submitEdit = true;
                Event.current.Use();
            }

            if (raycastIndex == -1 && Event.current.type == EventType.MouseDown)
            {
                Event.current.Use();
                propertyList.index = -1;
                if (editIndex != -1)
                    submitEdit = true;
            }

            if (raycastIndex != -1 && raycastIndex == propertyList.index && Event.current.clickCount > 1)
            {
                if (editIndex != -1 && editIndex < mAsset.Length)
                    contents[editIndex] = GetDescript(mAsset[editIndex]);
                editIndex = raycastIndex;
                propertyList.index = raycastIndex;
                Event.current.Use();
                repaint = true;
            }
            
            if(submitEdit)
            {
                submitEdit = false;
                if (editIndex != -1 && editIndex < mAsset.Length)
                    contents[editIndex] = GetDescript(mAsset[editIndex]);
                editIndex = -1;
                //propertyList.draggable = true;
                repaint = true;
            }
            if (repaint)
                Repaint();
           
        }
    }
}
