using UnityEngine;

namespace Devil
{

    public class CursorLock : SingletonMono<CursorLock>
    {

        [SerializeField]
        bool m_LockCursor;

        public static bool LockCursor
        {
            get { return GetOrNewInstance().m_LockCursor; }
            set
            {
                CursorLock locker = GetOrNewInstance();
                if (locker.m_LockCursor ^ value)
                {
                    locker.m_LockCursor = value;
                    if (locker.m_LockCursor)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                }
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (m_LockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(m_LockCursor && Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
#endif
    }
}