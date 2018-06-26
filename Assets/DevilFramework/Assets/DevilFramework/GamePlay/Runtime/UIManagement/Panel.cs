using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        DontKeepTrace = 4, // 不保留启动记录
        AlwaysRender = 8, // 始终渲染（被遮挡时保持Canvas渲染）
        FullScreen = 16, // 全屏展示
    }

    // ui panel
    [RequireComponent(typeof(Canvas))]
    public abstract class Panel : UIBehaviour
    {
        [MaskField]
        [SerializeField]
        EPanelProperty m_Propoerties = EPanelProperty.SingleInstance | EPanelProperty.FullScreen;
        public EPanelProperty Properties { get { return m_Propoerties; } }
        
        Canvas mCanvas;
        public Canvas GetCanvas()
        {
            if (mCanvas == null)
                mCanvas = GetComponent<Canvas>();
            return mCanvas;
        }
        
        /// <summary>
        /// 加载完成时回调
        /// </summary>
        public abstract void OnPanelLoaded();

        /// <summary>
        /// 开启窗口回调
        /// </summary>
        /// <returns>当可以打开时，返回 true，否则返回 false</returns>
        public abstract bool OnPanelOpen();

        /// <summary>
        /// 窗口接收消息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="arg"></param>
        public abstract void OnRequestForResult(int request, object arg);

        /// <summary>
        /// 窗口接受返回数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public abstract void OnReceiveResult(int request, object result);

        /// <summary>
        /// 是否已完成开启
        /// </summary>
        public abstract bool IsPanelOpened { get; }

        /// <summary>
        /// 窗口开启时回调
        /// </summary>
        public abstract void OnPanelOpened();

        public abstract void OnPanelGainFoucs();

        public abstract void OnPanelLostFocus();

        /// <summary>
        /// 关闭窗口回调
        /// </summary>
        /// <returns>当可以关闭时，返回 true，否则返回 false</returns>
        public abstract bool OnPanelClose();

        /// <summary>
        /// 窗口是否已完成关闭
        /// </summary>
        public abstract bool IsPanelClosed { get; }

        /// <summary>
        /// 窗口关闭完成时回调
        /// </summary>
        public abstract void OnPanelClosed();

        /// <summary>
        /// 窗口准备卸载时回调
        /// </summary>
        public abstract void OnPanelUnloaded();
    }
}