using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIScaler3D : MonoBehaviour
    {
        [SerializeField]
        float m_Width = 2;

        [ContextMenu("Resize")]
        private void OnValidate()
        {
            RectTransform rect = transform as RectTransform;
            float scale = m_Width / rect.sizeDelta.x;
            transform.localScale = scale * Vector3.one;
        }

        public float Width
        {
            get { return m_Width; }
            set
            {
                if(m_Width != value)
                {
                    m_Width = value;
                    RectTransform rect = transform as RectTransform;
                    float scale = m_Width / rect.sizeDelta.x;
                    transform.localScale = scale * Vector3.one;
                }
            }
        }

        public void SetPixelSize(Vector2 pixelSize)
        {
            RectTransform rect = transform as RectTransform;
            rect.sizeDelta = pixelSize;
        }

        public void SetSize(Vector2 size, float fixedWidth)
        {
            float hdw = size.y / size.x;
            RectTransform rect = transform as RectTransform;
            rect.sizeDelta = new Vector2(fixedWidth, hdw);
            float scale = m_Width / rect.sizeDelta.x;
            transform.localScale = scale * Vector3.one;
        }
    }
}