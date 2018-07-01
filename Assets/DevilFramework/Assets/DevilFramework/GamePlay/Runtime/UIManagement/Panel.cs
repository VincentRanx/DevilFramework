using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devil.GamePlay
{
    public enum EPanelMode
    {
        Normal = 0, // 普通模式
        Dialog = 1, // 对话框
        Status = 2, // 状态
    }

    public enum EPanelProperty
    {
        SingleInstance = 1, // 单例
        AutoCloseOnLoadScene = 2, // 在加载场景时自动关闭
        DisableMask = 4, // 禁用遮罩
    }

    public interface IPanelMessager
    {
        /// <summary>
        /// 请求id
        /// </summary>
        int RequestId { get; }

        /// <summary>
        /// 请求数据
        /// </summary>
        object RequestData { get; }

        /// <summary>
        /// 请求结果处理
        /// </summary>
        /// <param name="data"></param>
        void HandleResult(object data);
    }

    // ui panel
    [RequireComponent(typeof(Canvas))]
    public class Panel : UIBehaviour
    {
        public int m_CustomIdentifier;

        public EPanelMode m_Mode;

        [MaskField]
        public EPanelProperty m_Properties;

        [Range(0, 1)]
        [SerializeField]
        float m_MaskMultipier = 1;
        public float MaskMultipier { get { return m_MaskMultipier; } }

        Canvas mCanvas;
        public Canvas GetCanvas()
        {
            if (mCanvas == null)
            {
                mCanvas = GetComponent<Canvas>();
                mRaycaster = GetComponent<GraphicRaycaster>();
            }
            return mCanvas;
        }

        GraphicRaycaster mRaycaster;
        public GraphicRaycaster GetRaycaster()
        {
            if (mCanvas == null)
                GetCanvas();
            return mRaycaster;
        }

        public bool RaycastEvent
        {
            get
            {
                GraphicRaycaster ray = GetRaycaster();
                return ray != null && ray.enabled;
            }
            set
            {
                GraphicRaycaster ray = GetRaycaster();
                if (ray != null && ray.enabled != value)
                    ray.enabled = value;
            }
        }

        /// <summary>
        /// 开启窗口回调
        /// </summary>
        /// <returns>当可以打开时，返回 true，否则返回 false</returns>
        public virtual bool OnPanelOpen()
        {
            gameObject.SetActive(true);
            return true;
        }

        /// <summary>
        /// 窗口接收消息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="arg"></param>
        public virtual bool OnPanelOpenForResult(IPanelMessager sender) { return true; }

        public virtual void OnPanelGainFoucs() { }

        public virtual void OnPanelLostFocus() { }

        /// <summary>
        /// 关闭窗口回调
        /// </summary>
        /// <returns>当可以关闭时，返回 true，否则返回 false</returns>
        public virtual bool OnPanelClose()
        {
            gameObject.SetActive(false);
            return true;
        }

        /// <summary>
        /// 窗口是否正在关闭
        /// </summary>
        public virtual bool IsClosing() { return false; }
        
    }
}