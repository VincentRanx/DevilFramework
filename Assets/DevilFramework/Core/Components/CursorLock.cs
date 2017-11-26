using UnityEngine;

namespace DevilTeam
{

    public class CursorLock : MonoBehaviour
    {

        [SerializeField]
        bool m_LockCursor;

        private CursorLockMode mDefautlMode;

        private void Awake()
        {
            mDefautlMode = Cursor.lockState;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (m_LockCursor)
            {
                if (focus)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.lockState = mDefautlMode;
                }
            }
        }

        private void OnEnable()
        {
            if (m_LockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnDisable()
        {
            if (m_LockCursor)
            {
                Cursor.lockState = mDefautlMode;
            }
        }

        public bool LockCursor
        {
            get { return m_LockCursor; }
            set
            {
                if (m_LockCursor ^ value)
                {
                    m_LockCursor = value;
                    if (m_LockCursor)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                    else
                    {
                        Cursor.lockState = mDefautlMode;
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (m_LockCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.lockState = mDefautlMode;
                }
            }
        }

        private void Update()
        {
            if (m_LockCursor && Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                OnValidate();
            }
        }
#endif
    }
}