using UnityEngine;
using UnityEngine.SceneManagement;

namespace Devil.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    public class UIScaler3D : MonoBehaviour
    {
        [SerializeField]
        float m_PhysicsWidth = 2;
        [SerializeField]
        Vector2 m_PixelSize = new Vector2(400, 300);

        Canvas mCanvas;
        public Canvas canvas
        {
            get
            {
                if (mCanvas == null)
                    mCanvas = GetComponent<Canvas>();
                return mCanvas;
            }
        }

        [ContextMenu("Resize")]
        private void OnValidate()
        {
            SetSize(m_PixelSize, m_PhysicsWidth);
        }
        
        public void SetSize(Vector2 pixelSize, float physicWidth)
        {
            m_PixelSize.x = Mathf.Max(pixelSize.x, 1);
            m_PixelSize.y = Mathf.Max(pixelSize.y, 1);
            m_PhysicsWidth = Mathf.Max(0, physicWidth);
            float scale = m_PhysicsWidth / m_PixelSize.x;
            RectTransform rect = transform as RectTransform;
            rect.sizeDelta = m_PixelSize;
            transform.localScale = scale * Vector3.one;
        }

        private void OnEnable()
        {
            SetSize(m_PixelSize, m_PhysicsWidth);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadMod)
        {
            SetSize(m_PixelSize, m_PhysicsWidth);
        }
        
    }
}