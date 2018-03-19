using Devil;
using System.Text;
using UnityEngine;
using Devil.Utility;

namespace Devil.GamePlay
{
    [DefaultExecutionOrder(-10)]
    public class GameInput : SingletonMono<GameInput>
    {
        
        [System.Serializable]
        public class KeyBind
        {
            public InputMask m_InputMask;
            public string m_InputAxis;
        }

        public const char BUTTON_COMBINE_MARK = '+';
        static StringBuilder builder = new StringBuilder();

        public static string MaskText(InputMask mask, char split = BUTTON_COMBINE_MARK)
        {
            builder.Remove(0, builder.Length);
            bool first = true;
            for (int i = 0; i < 32; i++)
            {
                InputMask m = (InputMask)(1u << i);
                if ((mask & m) != 0)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(' ').Append(split).Append(' ');
                    }
                    builder.Append(m);
                }
            }
            return builder.ToString();
        }

        public static InputMask ParseInputMask(string text, char split = BUTTON_COMBINE_MARK)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            InputMask mask = 0;
            string[] spl = text.Split(split);
            for (int i = 0; i < spl.Length; i++)
            {
                InputMask m = (InputMask)System.Enum.Parse(typeof(InputMask), spl[i].Trim(), true);
                mask |= m;
            }
            return mask;
        }

        public static InputMask Mask
        {
            get
            {
                GameInput input = GetOrNewInstance();
                return input.m_Mask & ~(input.m_DownMask | input.m_UpMask);
            }
        }

        public static InputMask DownMask { get { return GetOrNewInstance().m_DownMask; } }

        public static InputMask UpMask { get { return GetOrNewInstance().m_UpMask; } }

        public static Vector3 InputDirection { get { return GetOrNewInstance().m_InputDirection; } }

        public static Vector2 InputCameraRot { get { return GetOrNewInstance().m_CameraRotation; } }

        public static bool GetAllButtonDown(InputMask mask)
        {
            return (DownMask & mask) == mask;
        }

        public static bool GetAllButton(InputMask mask)
        {
            return (Mask & mask) == mask;
        }

        public static bool GetAllButtonUp(InputMask mask)
        {
            return (UpMask & mask) == mask;
        }

        public static bool GetAnyButtonDown(InputMask mask)
        {
            return (DownMask & mask) != 0;
        }

        public static bool GetAnyButton(InputMask mask)
        {
            return (Mask & mask) != 0;
        }

        public static bool GetAnyButtonUp(InputMask mask)
        {
            return (UpMask & mask) != 0;
        }

        [SerializeField]
        KeyBind[] m_UsedInput;

        [SerializeField]
        string m_HorizontalAxis = "Horizontal";

        [SerializeField]
        string m_VerticalAxis = "Vertical";

        [SerializeField]
        string m_CameraRotationX = "Mouse X";

        [SerializeField]
        string m_CameraRotationY = "Mouse Y";

        Camera m_MainCamera;
        Vector3 m_InputDirection;
        Vector2 m_CameraRotation;

        // 当前按键
        InputMask m_Mask;
        // 当前按下的按键
        InputMask m_DownMask;
        // 当前抬起按键
        InputMask m_UpMask;

        Vector3 tmpDir;

        private void Update()
        {
            ProcessInputButton();
            ProcessInputDirection();
        }

        void ProcessInputDirection()
        {
            if (!m_MainCamera || !m_MainCamera.isActiveAndEnabled)
            {
                m_MainCamera = ComponentUtil.MainCamera;
            }
            tmpDir.x = Input.GetAxis(m_HorizontalAxis);
            tmpDir.y = 0;
            tmpDir.z = Input.GetAxis(m_VerticalAxis);
            if (m_MainCamera && m_MainCamera.isActiveAndEnabled)
            {
                float len = tmpDir.magnitude;
                tmpDir = m_MainCamera.transform.localToWorldMatrix.MultiplyVector(tmpDir);
                tmpDir = Vector3.ProjectOnPlane(tmpDir, Vector3.up);
                tmpDir = tmpDir.normalized * len;
            }
            m_InputDirection = tmpDir;
            m_CameraRotation.x = Input.GetAxis(m_CameraRotationX);
            m_CameraRotation.y = Input.GetAxis(m_CameraRotationY);
        }

        void ProcessInputButton()
        {

            InputMask prev = m_Mask;
            m_Mask = 0;
            for(int i = 0; i < m_UsedInput.Length; i++)
            {
                if (Input.GetButton(m_UsedInput[i].m_InputAxis))
                    m_Mask |= m_UsedInput[i].m_InputMask;
            }
            m_DownMask = (prev ^ m_Mask) & m_Mask;
            m_UpMask = (prev ^ m_Mask) & prev;
        }

#if UNITY_EDITOR
        GUIStyle style;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, m_InputDirection * 10);
        }

        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle();
            }
            string s = "Input: " + MaskText(m_Mask);
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(1, 1, Screen.width, 30), s, style);
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, 30), s, style);
        }
#endif
    }
}