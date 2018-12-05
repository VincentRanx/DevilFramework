using Devil.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(TightImage))]
    public class TightImageInspector : GraphicEditor
    {
        SerializedProperty m_Sprite;
        SerializedProperty m_BlurStep;
        SerializedProperty m_BlurIters;

        GUIContent m_SpriteContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_SpriteContent = new GUIContent("Source Image");
            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_BlurStep = serializedObject.FindProperty("m_BlurStep");
            m_BlurIters = serializedObject.FindProperty("m_BlurIters");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            OnPostEffectGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void OnPostEffectGUI()
        {
            EditorGUILayout.PropertyField(m_BlurIters);
            if(m_BlurIters.intValue > 0)
            {
                EditorGUILayout.PropertyField(m_BlurStep);
            }
        }

        /// <summary>
        /// Draw the atlas and Image selection fields.
        /// </summary>

        protected void SpriteGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Sprite, m_SpriteContent);
            if (EditorGUI.EndChangeCheck())
            {
                
            }
        }
        
        ///// <summary>
        ///// All graphics have a preview.
        ///// </summary>

        //public override bool HasPreviewGUI() { return true; }

        ///// <summary>
        ///// Draw the Image preview.
        ///// </summary>

        //public override void OnPreviewGUI(Rect rect, GUIStyle background)
        //{
        //    TightImage image = target as TightImage;
        //    if (image == null) return;

        //    Sprite sf = image.sprite;
        //    if (sf == null) return;

        //    SpriteDrawUtility.DrawSprite(sf, rect, image.canvasRenderer.GetColor());
        //}

        ///// <summary>
        ///// Info String drawn at the bottom of the Preview
        ///// </summary>

        //public override string GetInfoString()
        //{
        //    TightImage image = target as TightImage;
        //    Sprite sprite = image.sprite;

        //    int x = (sprite != null) ? Mathf.RoundToInt(sprite.rect.width) : 0;
        //    int y = (sprite != null) ? Mathf.RoundToInt(sprite.rect.height) : 0;

        //    return string.Format("Image Size: {0}x{1}", x, y);
        //}
    }
}