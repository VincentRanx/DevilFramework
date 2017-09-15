using UnityEngine;

namespace DevilTeam.UI
{

    public class UIRoot : SingletonMono<UIRoot>
    {

        [SerializeField]
        private Camera m_UICamera;
        public Camera UICamera { get { return m_UICamera; } }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        private void OnValidate()
        {
            if (!m_UICamera)
                m_UICamera = GetComponent<Camera>();
        }
    }
}