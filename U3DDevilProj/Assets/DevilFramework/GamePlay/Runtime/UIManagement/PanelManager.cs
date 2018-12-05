using Devil.GamePlay.Assistant;
using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devil.GamePlay
{
    public struct PanelIntent : IPanelIntent
    {
        public int requestId;
        public int RequestId { get { return requestId; } set { requestId = value; } }

        public object requestData;
        public object RequestData { get { return requestData; } set { requestData = value; } }

        public System.Action<int, object> handler;
        public System.Action<int, object> Handler { get { return handler; } set { handler = value; } } // <requestid, result>

        public void HandleResult(object data)
        {
            if (Handler != null)
                Handler(RequestId, data);
        }

        public static PanelIntent Instantiate(int reqId, object request)
        {
            PanelIntent sender = new PanelIntent();
            sender.RequestId = reqId;
            sender.RequestData = request;
            return sender;
        }
    }

    [DefaultExecutionOrder(-300)]
    public sealed class PanelManager : MonoBehaviour
    {
        //static int SORT_NORMAL = 0;
        //static int SORT_STATUS = 1;
        //static int SORT_DIALOG = 2;

        [System.Serializable]
        public class PanelMask
        {
            public Canvas m_RootCanvas;
            public Graphic m_TargetGraphic;
            [Range(0, 1)]
            public float m_MaskAlpha = 0.7f;
            [Range(0, 1)]
            public float m_TransDuration = 0.2f;

            bool mUpdate;
            bool mBegin;
            bool mEnableMask;
            float mTargetValue;
            float mTransSpeed;
            int mSortingLayer;
            int mSortingOrder;
            float mTime;

            public float Alpha
            {
                get
                {
                    if (m_TargetGraphic == null)
                        return 0;
                    else
                        return m_TargetGraphic.color.a;
                }
                set
                {
                    if (m_TargetGraphic != null)
                    {
                        Color col = m_TargetGraphic.color;
                        col.a = value;
                        m_TargetGraphic.color = col;
                    }
                }
            }

            public void Reset()
            {
                Alpha = 0;
                if (m_RootCanvas != null)
                    m_RootCanvas.enabled = false;
            }

            public void SetTarget(Panel basepanel)
            {
                if (m_RootCanvas == null)
                    return;
                if (basepanel != null)
                {
                    mTargetValue = m_MaskAlpha * basepanel.MaskMultipier;
                    Canvas can = basepanel.GetCanvas();
                    mSortingLayer = can.sortingLayerID;
                    mSortingOrder = can.sortingOrder - 1;

                    if (m_TransDuration > 0)
                    {
                        mTransSpeed = Mathf.Abs(mTargetValue - Alpha) / m_TransDuration;
                        mUpdate = true;
                        mBegin = !mEnableMask || mSortingLayer != m_RootCanvas.sortingLayerID || mSortingOrder != m_RootCanvas.sortingOrder;
                    }
                    else
                    {
                        m_RootCanvas.enabled = true;
                        Alpha = mTargetValue;
                    }
                    mEnableMask = true;
                }
                else
                {
                    mTargetValue = 0;
                    if (m_TransDuration > 0)
                    {
                        mTransSpeed = Mathf.Abs(mTargetValue - Alpha) / m_TransDuration;
                        mBegin = mEnableMask || mSortingLayer != m_RootCanvas.sortingLayerID || mSortingOrder != m_RootCanvas.sortingOrder;
                        mUpdate = true;
                    }
                    else
                    {
                        m_RootCanvas.enabled = false;
                        Alpha = mTargetValue;
                    }
                    mEnableMask = false;
                }
            }

            public void Update()
            {
                if (!mUpdate)
                    return;
                if (mBegin)
                {
                    mBegin = false;
                    mTime = 0;
                    if (mEnableMask)
                    {
                        m_RootCanvas.sortingLayerID = mSortingLayer;
                        m_RootCanvas.sortingOrder = mSortingOrder;
                    }
                    m_RootCanvas.enabled = true;
                }
                float a = Alpha;
                mTime += Time.unscaledDeltaTime;
                Alpha = Mathf.MoveTowards(a, mTargetValue, mTransSpeed * Time.unscaledDeltaTime);
                if (mTime >= m_TransDuration)
                {
                    mUpdate = false;
                    if (!mEnableMask)
                        m_RootCanvas.enabled = false;
                }
            }

            public void SendTexture(RenderTexture tex)
            {
                var img = m_TargetGraphic as RawImage;
                if (img != null)
                    img.texture = tex;
            }
        }

        class PanelStub
        {
            public PanelManager Mgr { get; private set; }
            public PanelAsset Asset { get; set; }
            public Panel Instance { get; set; }
            public int Group { get { return Instance == null ? 0 : Instance.Group; } }
            public EPanelMode Mode { get { return Asset == null ? 0 : Asset.Mode; } }
            public EPanelProperty Property { get { return Asset == null ? 0 : Asset.Properties; } }
            bool mFocus;
            public bool HasFocus
            {
                get { return mFocus; }
                set
                {
                    if (mFocus != value)
                    {
                        mFocus = value;
                        if (Instance != null)
                        {
                            if (mFocus)
                                Instance.OnPanelGainFoucs();
                            else
                                Instance.OnPanelLostFocus();
                            if (Mgr != null)
                                Mgr.NotifyFocusPanel(this, value);
                            if (EventSystem.current != null)
                                EventSystem.current.SetSelectedGameObject(value && Instance.m_AutoSelectDefault && Instance.m_DefaultSelectable != null ? Instance.m_DefaultSelectable.gameObject : null);
                        }
                    }
                }
            }

            public PanelStub(PanelManager mgr)
            {
                Mgr = mgr;
            }
        }

        [SerializeField]
        Camera m_UICamera;
        public Camera UICamera { get { return m_UICamera; } }
        List<PanelAsset> mAssets = new List<PanelAsset>();

        [SortingLayerField]
        [SerializeField]
        int m_NormalSortingLayer;
        public int NormalSortingLayer { get { return m_NormalSortingLayer; } }

        [SortingLayerField]
        [SerializeField]
        int m_StatusSortingLayer;
        public int StatusSortingLayer { get { return m_StatusSortingLayer; } }

        [SortingLayerField]
        [SerializeField]
        int m_DialogSortingLayer;
        public int DialogSortingLayer { get { return m_DialogSortingLayer; } }

        [SortingLayerField]
        [SerializeField]
        int m_TopStatusSortingLayer;
        public int TopStatusSortingLayer { get { return m_TopStatusSortingLayer; } }

        [SerializeField]
        private string m_CancelButtonName = "Cancel";
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";

        [SerializeField]
        PanelMask m_EventMask = new PanelMask();

        [SerializeField]
        bool m_DontDestroyOnLoad = true;

        List<PanelStub> mStatus = new List<PanelStub>();
        List<PanelStub> mPanels = new List<PanelStub>();
        List<PanelStub> mDialogs = new List<PanelStub>();

        List<PanelStub> mClosing = new List<PanelStub>();
        PanelStub mFocusStub = null;
        bool mFindFocusWindow;

        public event System.Action<Panel> OnPanelBecomeForeground = (x) => {
#if UNITY_EDITOR
            RTLog.LogFormat(LogCat.UI, "'{0}' become foreground.", x.name);
#endif
        };
        public event System.Action<Panel> OnPanelBecomeBackground = (x) => {
#if UNITY_EDITOR
        RTLog.LogFormat(LogCat.UI, "'{0}' become backgroud.", x.name);
#endif
        };
        public event System.Action<Panel> OnPanelOpened = (x) => {
#if UNITY_EDITOR
            RTLog.LogFormat(LogCat.UI, "'{0}' opened.", x.name);
#endif
        };
        public event System.Action<Panel> OnPanelClosed = (x) => {
#if UNITY_EDITOR
            RTLog.LogFormat(LogCat.UI, "'{0}' closed.", x.name);
#endif
        };
        public event System.Action<RenderTexture> OnBufferTextureUpdated = (x) => { };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Panel[] panels = GetComponentsInChildren<Panel>();
                int len = panels == null ? 0 : panels.Length;
                for (int i = 0; i < len; i++)
                {
                    PanelAsset asset = new PanelAsset(panels[i]);
                    if (asset.IsUsable)
                    {
                        mAssets.Add(asset);
                    }
                }
                if (m_EventMask != null)
                    m_EventMask.Reset();
                if (m_DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (Instance == this)
            {
                SceneHelper sh = SceneHelper.Instance;
                if (sh != null)
                {
                    sh.OnLoadBegin += OnSceneLoadBegin;
                    sh.OnSceneWillLoad += OnSceneWillLoad;
                    OnSceneLoadBegin(sh.ActiveScene.name);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                SceneHelper sh = SceneHelper.Instance;
                if (sh != null)
                {
                    sh.OnLoadBegin -= OnSceneLoadBegin;
                    sh.OnSceneWillLoad -= OnSceneWillLoad;
                }
            }
        }

        void NotifyFocusPanel(PanelStub stub, bool focus)
        {
            if (focus)
                OnPanelBecomeForeground(stub.Instance);
            else
                OnPanelBecomeBackground(stub.Instance);
        }

        // recentCoutner: 0:last 1, 1:last 2, 2:last 3
        PanelStub GetRecentPanel(List<PanelStub> list, int recentCounter)
        {
            int n = list.Count - 1 - recentCounter;
            if (n >= 0 && n < list.Count)
                return list[n];
            else
                return null;
        }

        bool ContainsPanel(List<PanelStub> list, Panel panel)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Instance == panel)
                    return true;
            }
            return false;
        }

        int GetPanelIndex(List<PanelStub> list, Panel panel)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Instance == panel)
                    return i;
            }
            return -1;
        }

        public void AddPanelAsset(PanelAsset asset)
        {
            if (asset == null)
                return;
            if (GetAsset(asset.Id) != null)
            {
                return;
            }
            mAssets.Add(asset);
        }

        void ClearPanelForAsset(PanelAsset asset, List<PanelStub> panels)
        {
            for(int i = panels.Count - 1; i >= 0; i--)
            {
                PanelStub stub = panels[i];
                if(stub.Asset == asset)
                {
                    if (mFocusStub == stub)
                    {
                        stub.HasFocus = false;
                        mFocusStub = null;
                        mFindFocusWindow = true;
                    }
                    stub.Instance.OnPanelLostAsset();
                    asset.UnuseAsset(stub.Instance);
                    panels.RemoveAt(i);
                }
            }
        }

        public void RemoveAsset(PanelAsset asset)
        {
            if (asset == null || !mAssets.Contains(asset))
                return;
            ClearPanelForAsset(asset, mClosing);
            ClearPanelForAsset(asset, mDialogs);
            ClearPanelForAsset(asset, mStatus);
            ClearPanelForAsset(asset, mPanels);
            mAssets.Remove(asset);
            asset.Release();
            mFindFocusWindow = true;
        }

        public PanelAsset GetAsset(int panelId)
        {
            for (int i = 0; i < mAssets.Count; i++)
            {
                PanelAsset asset = mAssets[i];
                if (asset.Id == panelId)
                    return asset;
            }
            return null;
        }

        public PanelAsset GetAsset(string assetName)
        {
            for(int i = 0; i < mAssets.Count; i++)
            {
                PanelAsset asset = mAssets[i];
                if (asset.Name == assetName)
                    return asset;
            }
            return null;
        }

        Panel OpenPanelAsStatus(PanelAsset asset, IPanelIntent request)
        {
            return OpenPanelFor(mStatus, asset.Mode == EPanelMode.Status ? m_StatusSortingLayer : m_TopStatusSortingLayer, asset, request, false);
        }

        Panel OpenPanelAsDialog(PanelAsset asset, IPanelIntent request)
        {
            return OpenPanelFor(mDialogs, m_DialogSortingLayer, asset, request, true);
        }

        Panel OpenPanelAsNormal(PanelAsset asset, IPanelIntent request)
        {
            if (mDialogs.Count > 0)
                return null;
            return OpenPanelFor(mPanels, m_NormalSortingLayer, asset, request, true);
        }
        
        // asset must be usable.
        Panel OpenPanelFor(List<PanelStub> list, int sortLayer, PanelAsset asset, IPanelIntent request, bool useFocus)
        {
            Panel panel = asset.InstantiateAsset();
            if (panel == null || panel.IsClosing())
                return null;
            bool open;
            if (request != null)
                open = panel.OpenPanelForResult(request);
            else
                open = panel.OpenPanel();
            if (!open)
            {
                asset.UnuseAsset(panel);
                return null;
            }
            panel.OnPanelOpened();
            OnPanelOpened(panel);
            int close = GetPanelIndex(mClosing, panel);
            if (close != -1)
                mClosing.RemoveAt(close);
            //panel.transform.SetParent(transform, false);
            WaittingForOpenPanel = false;
            int old = GetPanelIndex(list, panel);
            if (old != -1)
                list.RemoveAt(old);
            PanelStub stub = new PanelStub(this);
            stub.Asset = asset;
            stub.Instance = panel;
            list.Add(stub);
            PanelStub prev = GetRecentPanel(list, 1);
            Canvas prevcan = prev == null ? null : prev.Instance.GetCanvas();
            Canvas can = panel.GetCanvas();
            can.renderMode = RenderMode.ScreenSpaceCamera;
            can.worldCamera = m_UICamera;
            can.sortingLayerID = sortLayer;
            if (asset.Mode != EPanelMode.Status)
                can.sortingOrder = prevcan == null ? 1 : prevcan.sortingOrder + 2;
            if (useFocus)
            {
                if (mFocusStub != null)
                {
                    mFocusStub.HasFocus = false;
                    mFocusStub = null;
                }
                //else
                //{
                //    mFocusStub = stub;
                //    mFocusStub.HasFocus = true;
                //}
                mFindFocusWindow = true;
            }
            if (asset.IsUseMask)
                UpdateMask();
            return panel;
        }

        void MovePanelForeground(List<PanelStub> list, Panel panel)
        {
            var old = GetPanelIndex(list, panel);
            if (old == -1 || old == list.Count - 1)
                return;
            var stub = list[old];
            list.RemoveAt(old);
            list.Add(stub);
            if (mFocusStub != null)
            {
                mFocusStub.HasFocus = false;
                mFocusStub = null;
            }
            mFindFocusWindow = true;
            if (stub.Asset.IsUseMask)
            {
                UpdateSortingOrder(list, old);
                UpdateMask();
            }
            else
            {
                var prev = GetRecentPanel(list, 1);
                Canvas prevcan = prev == null ? null : prev.Instance.GetCanvas();
                Canvas can = panel.GetCanvas();
                can.sortingOrder = prevcan == null ? 1 : prevcan.sortingOrder + 2;
            }
        }

        bool ClosePanelAsDialog(PanelStub stub)
        {
            return ClosePanelFor(mDialogs, stub, true);
        }

        bool ClosePanelAsNormal(PanelStub stub)
        {
            return ClosePanelFor(mPanels, stub, true);
        }

        bool ClosePanelAsStatus(PanelStub stub)
        {
            return ClosePanelFor(mStatus, stub, false);
        }

        int ClosePanelFor(List<PanelStub> list, FilterDelegate<PanelStub> filter, bool useFocus)
        {
            bool useMask = false;
            int num = 0;
            for(int i = list.Count - 1; i >= 0; i--)
            {
                PanelStub stub = list[i];
                if (!filter(stub))
                    continue;
                if (stub.Instance.IsClosing())
                    continue;
                bool close = stub.Instance.ClosePanel();
                if (!close)
                    continue;
                list.RemoveAt(i);
                if (useFocus)
                {
                    if (mFocusStub == stub)
                    {
                        mFocusStub = null;
                        stub.HasFocus = false;
                        mFindFocusWindow = true;
                    }
                }
                stub.Instance.OnPanelClosed();
                OnPanelClosed(stub.Instance);
                if (stub.Instance.IsClosing())
                    mClosing.Add(stub);
                else
                    stub.Asset.UnuseAsset(stub.Instance);
                num++;
                
                useMask |= stub.Asset.IsUseMask;
            }
            if (useMask)
                UpdateMask();
            return num;
        }

        bool ClosePanelFor(List<PanelStub> list, PanelStub stub, bool useFocus)
        {
            if (stub.Instance.IsClosing())
                return true;
            bool close = stub.Instance.ClosePanel();
            if (!close)
                return false;
            list.Remove(stub);
            if (useFocus)
            {
                if (mFocusStub == stub)
                {
                    mFocusStub = null;
                    stub.HasFocus = false;
                    mFindFocusWindow = true;
                }
            }
            stub.Instance.OnPanelClosed();
            OnPanelClosed(stub.Instance);
            if (stub.Instance.IsClosing())
                mClosing.Add(stub);
            else
                stub.Asset.UnuseAsset(stub.Instance);
            if (stub.Asset.IsUseMask)
                UpdateMask();
            return true;
        }

        void UpdateSortingOrder(List<PanelStub> list, int startIndex)
        {
            if (startIndex >= list.Count)
                return;
            int order = 1;
            if(startIndex > 0)
            {
                var can = list[startIndex - 1].Instance.GetCanvas();
                if (can != null)
                    order = can.sortingOrder + 2;
            }
            for(int i = startIndex; i < list.Count; i++)
            {
                var stub = list[i];
                var can = stub.Instance.GetCanvas();
                if (can != null)
                {
                    can.sortingOrder = order;
                    order += 2;
                }
            }
        }

        void UpdateMask()
        {
            if (m_EventMask.m_RootCanvas == null)
                return;
            
            Panel basepanel = GetMaskPanel(mDialogs);
            if (basepanel == null)
                basepanel = GetMaskPanel(mPanels);
            m_EventMask.SetTarget(basepanel);
        }

        Panel GetMaskPanel(List<PanelStub> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                PanelStub stub = list[i];
                if (stub.Asset.IsUseMask)
                {
                    return stub.Instance;
                }
            }
            return null;
        }

        private void Update()
        {
            for (int i = mClosing.Count - 1; i >= 0; i--)
            {
                PanelStub stub = mClosing[i];
                if(!stub.Instance.IsClosing())
                {
                    mClosing.RemoveAt(i);
                    stub.Asset.UnuseAsset(stub.Instance);
                }
            }
            if (!WaittingForOpenPanel || mClosing.Count == 0)
                m_EventMask.Update();

            if (mFindFocusWindow)
            {
                mFindFocusWindow = false;
                var stub = GetRecentPanel(mDialogs, 0);
                if (stub == null)
                    stub = GetRecentPanel(mPanels, 0);
                if (stub != null)
                {
                    mFocusStub = stub;
                    stub.HasFocus = true;
                }
            }

            if (GameInput.GetButtonDown(m_CancelButtonName) && !string.IsNullOrEmpty(m_CancelButtonName))
            {
                InteractCancel();
            }
            if (mFocusStub != null && mFocusStub.Instance.m_DefaultSelectable != null
                && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                if (Mathf.Abs(GameInput.GetAxis(m_HorizontalAxis)) > 0.5f || Mathf.Abs(GameInput.GetAxis(m_VerticalAxis)) > 0.5f ||
                    GameInput.GetButtonDown(m_HorizontalAxis) || GameInput.GetButtonDown(m_VerticalAxis))
                {
                    EventSystem.current.SetSelectedGameObject(mFocusStub.Instance.m_DefaultSelectable.gameObject);
                }
            }
        }

        public void InteractCancel()
        {
            for (int i = mDialogs.Count - 1; i >= 0; i--)
            {
                var p = mDialogs[i];
                if (p.Instance.InteractCancel())
                {
                    return;
                }
            }
            for(int i= mPanels.Count - 1; i >= 0; i--)
            {
                var p = mPanels[i];
                if (p.Instance.InteractCancel())
                    return;
            }
        }

        void OnSceneLoadBegin(string scene)
        {
            FilterDelegate<PanelStub> filter = (x) => x.Asset.AutoCloseOnLoadScene;
            ClosePanelFor(mDialogs, filter, true);
            ClosePanelFor(mPanels, filter, true);
            ClosePanelFor(mStatus, filter, false);
        }

        // 场景即将卸载
        void OnSceneWillLoad(string scene)
        {
            //for (int i = 0; i < mAssets.Count; i++)
            //{
            //    mAssets[i].Release();
            //}
        }

        #region opend method

        public static PanelManager Instance { get; private set; }

        public static bool WaittingForOpenPanel { get; set; }

        public static Panel FocusPanel
        {
            get
            {
                PanelManager inst = Instance;
                if (inst == null || inst.mFocusStub == null)
                    return null;
                else
                    return inst.mFocusStub.Instance;
            }
            set
            {
                var inst = Instance;
                if (value == null || value.m_Mode != EPanelMode.Normal || inst == null || inst.mDialogs.Count > 0)
                    return;
                inst.MovePanelForeground(inst.mPanels, value);
            }
        }

        public static void ReleasePanels()
        {
            PanelManager inst = Instance;
            if (inst == null)
                return;
            for(int i = 0; i < inst.mAssets.Count; i++)
            {
                PanelAsset asset = inst.mAssets[i];
                asset.Release();
            }
        }

        static PanelStub FindPanelStub(List<PanelStub> list, FilterDelegate<PanelStub> filter)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                PanelStub stub = list[i];
                if (filter(stub))
                    return stub;
            }
            return null;
        }

        static PanelStub FindPanel(FilterDelegate<PanelStub> filter)
        {
            if (filter == null)
                return null;
            PanelManager inst = Instance;
            if (inst == null)
                return null;
            PanelStub panel;
            panel = FindPanelStub(inst.mPanels, filter);
            if (panel != null)
                return panel;
            panel = FindPanelStub(inst.mDialogs, filter);
            if (panel != null)
                return panel;
            panel = FindPanelStub(inst.mStatus, filter);
            if (panel != null)
                return panel;
            return null;
        }

        public static Panel FindPanel(FilterDelegate<Panel> filter)
        {
            if (filter == null)
                return null;
            PanelManager inst = Instance;
            if (inst == null)
                return null;
            PanelStub panel;
            FilterDelegate<PanelStub> stubf = (x) => filter(x.Instance);
            panel = FindPanelStub(inst.mPanels, stubf);
            if (panel != null)
                return panel.Instance;
            panel = FindPanelStub(inst.mDialogs, stubf);
            if (panel != null)
                return panel.Instance;
            panel = FindPanelStub(inst.mStatus, stubf);
            if (panel != null)
                return panel.Instance;
            return null;
        }

        public static Panel OpenPanel(int id, IPanelIntent request = null)
        {
            PanelManager inst = Instance;
            if (inst == null)
                return null;
            PanelAsset asset = inst.GetAsset(id);
            if (asset == null || !asset.IsUsable)
                return null;
            else if (asset.Mode == EPanelMode.Dialog)
                return inst.OpenPanelAsDialog(asset, request);
            else if (asset.Mode == EPanelMode.Status || asset.Mode == EPanelMode.TopStatus)
                return inst.OpenPanelAsStatus(asset, request);
            else if (asset.Mode == EPanelMode.Normal)
                return inst.OpenPanelAsNormal(asset, request);
            else
                return null;
        }

        public static Panel OpenPanel(string panelName, IPanelIntent request = null)
        {
            PanelManager inst = Instance;
            if (inst == null)
                return null;
            PanelAsset asset = inst.GetAsset(panelName);
            if (asset == null || !asset.IsUsable)
                return null;
            else if (asset.Mode == EPanelMode.Dialog)
                return inst.OpenPanelAsDialog(asset, request);
            else if (asset.Mode == EPanelMode.Status || asset.Mode == EPanelMode.TopStatus)
                return inst.OpenPanelAsStatus(asset, request);
            else if (asset.Mode == EPanelMode.Normal)
                return inst.OpenPanelAsNormal(asset, request);
            else
                return null;
        }

        public static bool ClosePanel(string pname)
        {
            Panel p = FindPanel((x) => x.name == pname);
            return ClosePanel(p);
        }

        public static bool ClosePanel(int pid)
        {
            PanelStub p = FindPanel((x) => x.Asset.Id == pid);
            if (p == null)
                return false;
            else
                return ClosePanel(p.Instance);
        }

        public static bool ClosePanel(Panel panel)
        {
            if (panel == null)
                return false;
            PanelManager inst = Instance;
            if (inst == null)
                return false;
            int index;
            if (panel.m_Mode == EPanelMode.Dialog)
            {
                index = inst.GetPanelIndex(inst.mDialogs, panel);
                return index == -1 ? false : inst.ClosePanelAsDialog(inst.mDialogs[index]);
            }
            else if(panel.m_Mode == EPanelMode.Normal)
            {
                index = inst.GetPanelIndex(inst.mPanels, panel);
                return index == -1 ? false : inst.ClosePanelAsNormal(inst.mPanels[index]);
            }
            else if(panel.m_Mode == EPanelMode.Status || panel.m_Mode == EPanelMode.TopStatus)
            {
                index = inst.GetPanelIndex(inst.mStatus, panel);
                return index == -1 ? false : inst.ClosePanelAsStatus(inst.mStatus[index]);
            }
            else
            {
                return false;
            }
        }

        public static int ClosePanelByGroup(int panelGroup)
        {
            PanelManager inst = Instance;
            if (inst == null)
                return 0;
            int num = 0;
            FilterDelegate<PanelStub> filter = (x) => x.Group == panelGroup;
            num += inst.ClosePanelFor(inst.mStatus, filter, false);
            num += inst.ClosePanelFor(inst.mPanels, filter, true);
            num += inst.ClosePanelFor(inst.mDialogs, filter, true);
            return num;
        }

        public static bool HasAnyPanelClosing
        {
            get
            {
                PanelManager inst = Instance;
                return inst != null && inst.mClosing.Count > 0;
            }
        }

        public static Panel TopDialog
        {
            get
            {
                var inst = Instance;
                if (inst == null)
                    return null;
                else if (inst.mDialogs.Count > 0)
                    return inst.mDialogs[inst.mDialogs.Count - 1].Instance;
                else
                    return null;
            }
        }

        public static Panel TopDialogOrPanel
        {
            get {
                var inst = Instance;
                if (inst == null)
                    return null;
                else if (inst.mDialogs.Count > 0)
                    return inst.mDialogs[inst.mDialogs.Count - 1].Instance;
                else if (inst.mPanels.Count > 0)
                    return inst.mPanels[inst.mPanels.Count - 1].Instance;
                else
                    return null;
            }
        }

        public static Panel TopFullScreenPanel
        {
            get
            {
                var inst = Instance;
                if (inst == null)
                    return null;
                for (int i = inst.mDialogs.Count - 1; i >= 0; i--)
                {
                    var dlg = inst.mDialogs[i];
                    if ((dlg.Property & EPanelProperty.FullScreen) != 0)
                        return dlg.Instance;
                }
                for(int i= inst.mPanels.Count - 1;i>= 0; i--)
                {
                    var p = inst.mPanels[i];
                    if ((p.Property & EPanelProperty.FullScreen) != 0)
                        return p.Instance;
                }
                return null;
            }
        }

        public static void SetPanelActive(Panel panel, bool active)
        {
            panel.gameObject.SetActive(active);
        }

        public static bool IsPanelActive(Panel panel)
        {
            return panel.gameObject.activeSelf;
        }
        
        #endregion
    }
}
