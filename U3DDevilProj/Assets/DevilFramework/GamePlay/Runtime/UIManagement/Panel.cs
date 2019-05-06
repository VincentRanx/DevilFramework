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
        TopStatus = 3, // 顶层状态
    }

    public enum EPanelProperty
    {
        SingleInstance = 1, // 单例
        AutoCloseOnLoadScene = 2, // 在加载场景时自动关闭
        DisableMask = 4, // 禁用遮罩
        FullScreen = 8, // 全屏
    }

    public interface IPanelIntent<T>
    {
        // 处理消息数据
        void HandleIntent(T intent);
    }
    
    public class PanelIntentData
    {
        private PanelIntentData() { }
        public static readonly PanelIntentData NONE = new PanelIntentData();
    }

    public interface IPanelEvent
    {

        /// <summary>
        /// 开启窗口回调
        /// </summary>
        /// <returns>当可以打开时，返回 true，否则返回 false</returns>
        bool OpenPanel();
        
        /// <summary>
        /// 关闭窗口回调
        /// </summary>
        /// <returns>当可以关闭时，返回 true，否则返回 false</returns>
        bool ClosePanel();

        /// <summary>
        /// 窗口是否正在关闭
        /// </summary>
        bool IsClosing();

        void OnPanelOpened();

        void OnPanelGainFoucs();

        void OnPanelLostFocus();

        void OnPanelClosed();

        /// <summary>
        /// 资源丢失
        /// </summary>
        void OnPanelLostAsset();
    }

    public interface ISubPanel
    {
        void SetRootCanvas(Canvas canvas);
    }

    public interface IPanelEventHandler
    {

    }

    public interface IPanelOpenHandler : IPanelEventHandler
    {
        void OnPanelOpened();
    }

    public interface IPanelCloseHandler : IPanelEventHandler
    {
        void OnPanelClosed();
    }

    public interface IPanelFocusHandler : IPanelEventHandler
    {
        void OnPanelGetFocus();
        void OnPanelLostFocus();
    }

    public interface IPanelLostAssetHandler : IPanelEventHandler
    {
        void OnPanelLostAsset();
    }

    // ui panel
    [RequireComponent(typeof(Canvas))]
    public class Panel : UIBehaviour, IPanelEvent
    {
        public int m_CustomIdentifier;

        public EPanelMode m_Mode;
        public Selectable m_DefaultSelectable;
        public bool m_AutoSelectDefault;

        [SerializeField]
        int m_Group;
        public int Group { get { return m_Group; } }

        [SerializeField]
        protected bool m_ResponseCancelButton = true;

        [MaskField(IsToggle = false)]
        [SerializeField]
        EPanelProperty m_Properties = EPanelProperty.SingleInstance | EPanelProperty.AutoCloseOnLoadScene;
        public EPanelProperty Properties { get { return m_Properties; } set { m_Properties = value; } }

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

        public Camera GetRenderCamera()
        {
            var can = GetCanvas();
            if (can == null || can.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            else
                return can.worldCamera;
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

        System.Action mOpenedHandler;
        System.Action mFocusHandler;
        System.Action mLostFocusHandler;
        System.Action mClosedHandler;
        System.Action mLostAssetHandler;
        bool mGetHandles;

        public bool HasAnyProperty(EPanelProperty prorety)
        {
            return (m_Properties & prorety) != 0;
        }

        public bool HasAllProperty(EPanelProperty property)
        {
            return (m_Properties & property) == property;
        }

        protected void GetEventHandlers()
        {
            if (mGetHandles)
                return;
            mGetHandles = true;
            var handlers = GetComponents<IPanelEventHandler>();
            int len = handlers == null ? 0 : handlers.Length;
            for (int i = 0; i < len; i++)
            {
                var hand = handlers[i];
                if (hand is IPanelOpenHandler)
                {
                    IPanelOpenHandler oh = (IPanelOpenHandler)hand;
                    mOpenedHandler += oh.OnPanelOpened;
                }
                if (hand is IPanelCloseHandler)
                {
                    IPanelCloseHandler ch = (IPanelCloseHandler)hand;
                    mClosedHandler += ch.OnPanelClosed;
                }
                if (hand is IPanelFocusHandler)
                {
                    IPanelFocusHandler fh = (IPanelFocusHandler)hand;
                    mFocusHandler += fh.OnPanelGetFocus;
                    mLostFocusHandler += fh.OnPanelLostFocus;
                }
                if (hand is IPanelLostAssetHandler)
                {
                    mLostAssetHandler += ((IPanelLostAssetHandler)hand).OnPanelLostAsset;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_DefaultSelectable == null)
                m_DefaultSelectable = GetComponentInChildren<Selectable>();
        }

        /// <summary>
        /// 开启窗口回调
        /// </summary>
        /// <returns>当可以打开时，返回 true，否则返回 false</returns>
        public virtual bool OpenPanel()
        {
            GetEventHandlers();
            PanelManager.SetPanelActive(this, true);
            return true;
        }

        /// <summary>
        /// 关闭窗口回调
        /// </summary>
        /// <returns>当可以关闭时，返回 true，否则返回 false</returns>
        public virtual bool ClosePanel()
        {
            return true;
        }

        /// <summary>
        /// 窗口是否正在关闭
        /// </summary>
        public virtual bool IsClosing() { return false; }

        public virtual void OnPanelOpened()
        {
            if (mOpenedHandler != null)
                mOpenedHandler();
        }

        public virtual void OnPanelGainFoucs()
        {
            if (mFocusHandler != null)
                mFocusHandler();
        }

        public virtual void OnPanelLostFocus()
        {
            if (mLostFocusHandler != null)
                mLostFocusHandler();
        }

        public virtual void OnPanelClosed()
        {
            PanelManager.SetPanelActive(this, false);
            if (mClosedHandler != null)
                mClosedHandler();
        }

        /// <summary>
        /// 资源丢失
        /// </summary>
        public virtual void OnPanelLostAsset()
        {
            if (mLostAssetHandler != null)
                mLostAssetHandler();
        }

        /// <summary>
        /// 截获返回按键操作
        /// </summary>
        /// <returns></returns>
        public virtual bool InteractCancel()
        {
            if (m_ResponseCancelButton)
                PanelManager.ClosePanel(this);
            return true;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if(m_DefaultSelectable == null)
            {
                m_DefaultSelectable = GetComponentInChildren<Selectable>();
            }
        }

        [ContextMenu("Bind Targets")]
        protected void BindTargets()
        {
            Utility.ObjectBinder.BindPropertyiesByName(this);
            UnityEditor.EditorUtility.SetDirty(this);
            if (gameObject.activeInHierarchy)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        }
        [ContextMenu("Rename Targets' Name")]
        protected void RenameTargets()
        {
            Utility.ObjectBinder.RenameBindableProperties(this);
            if (gameObject.activeInHierarchy)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
#endif
    }
}