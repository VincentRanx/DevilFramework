using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    [DefaultExecutionOrder(-200)]
    public class GameInput : MonoBehaviour
    {
        static GameInput sActiveInst;

        public interface IOverrideAxis : ITick
        {
            string AxisName { get; }
            float Value { get; }
            float Sensitivity { get; set; } // 敏感度
            float DeadArea { get; set; } // 死区
        }

        public interface IOverrideButton
        {
            string ButtonName { get; }
            bool IsDown { get; }
            bool IsUp { get; }
            bool HasButton { get; }
        }

        private static IOverrideAxis GetOverrideAxis(string axisName)
        {
            var inst = sActiveInst;
            if (inst == null)
                return null;
            int len = inst.mAxises.Count;
            for(int i= 0; i < len; i++)
            {
                var axis = inst.mAxises[i];
                if (axis.AxisName == axisName)
                    return axis;
            }
            return null;
        }

        private static IOverrideButton GetOverrideButton(string buttonName)
        {
            var inst = sActiveInst;
            if (inst == null)
                return null;
            int len = inst.mButtons.Count;
            for(int i = 0; i < len; i++)
            {
                var btn = inst.mButtons[i];
                if (btn.ButtonName == buttonName)
                    return btn;
            }
            return null;
        }

        public static float GetAxis(string axisName)
        {
            var axis = GetOverrideAxis(axisName);
            if (axis == null)
                return Input.GetAxis(axisName);
            else
                return axis.Value;
        }

        public static bool GetButton(string buttonName)
        {
            var btn = GetOverrideButton(buttonName);
            if (btn == null)
                return Input.GetButton(buttonName);
            else
                return btn.HasButton;
        }

        public static bool GetButtonDown(string buttonName)
        {
            var btn = GetOverrideButton(buttonName);
            if (btn == null)
                return Input.GetButtonDown(buttonName);
            else
                return btn.IsDown;
        }

        public static bool GetButtonUp(string buttonName)
        {
            var btn = GetOverrideButton(buttonName);
            if (btn == null)
                return Input.GetButtonUp(buttonName);
            else
                return btn.IsUp;
        }

        #region implement

        [SerializeField]
        private bool m_DontDestroyOnLoad = true;

        private List<IOverrideAxis> mAxises = new List<IOverrideAxis>();
        private List<IOverrideButton> mButtons = new List<IOverrideButton>();

        private void Awake()
        {
            if (sActiveInst == null)
            {
                sActiveInst = this;
                if(m_DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if(sActiveInst == this)
            {
                sActiveInst = null;
            }
        }

        private void Update()
        {
            for(int i= 0; i < mAxises.Count; i++)
            {
                mAxises[i].OnTick(Time.unscaledDeltaTime);
            }
        }
        #endregion
    }
}