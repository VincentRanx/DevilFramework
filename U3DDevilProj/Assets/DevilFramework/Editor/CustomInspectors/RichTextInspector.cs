using Devil;
using Devil.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(RichText), true)]
    [CanEditMultipleObjects]
    public class RichTextInspector : GraphicEditor
    {
        SerializedProperty m_Text;
        SerializedProperty m_FontData;
        SerializedProperty m_Atlas;
        SerializedProperty m_Anims;
        SerializedProperty m_SupportEmoji;

        List<RichText.EmojiButton> btns;

        protected override void OnEnable()
        {
            base.OnEnable();
            bool hasfield;
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
            m_Atlas = serializedObject.FindProperty("m_Atlas");
            m_Anims = serializedObject.FindProperty("m_Anims");
            m_SupportEmoji = serializedObject.FindProperty("m_SupportEmoji");
            btns = Ref.GetField(target, "mEmojiBtns", out hasfield) as List<RichText.EmojiButton>;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Text);
            EditorGUILayout.PropertyField(m_FontData);
            EditorGUILayout.PropertyField(m_Atlas);
            EditorGUILayout.PropertyField(m_Anims);
            EditorGUILayout.PropertyField(m_SupportEmoji);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (btns != null)
            {
                var trans = ((RichText)target).transform;
                for (int i = 0; i < btns.Count; i++)
                {
                    var btn = btns[i];
                    Handles.Label(trans.localToWorldMatrix.MultiplyPoint(new Vector3(btn.rect.xMin, btn.rect.yMax)),
                        StringUtil.Concat("Btn_", btn.clickId), "box");
                }
            }
        }
    }
}