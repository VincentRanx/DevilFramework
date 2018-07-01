using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Devil.GamePlay
{
    public class PanelManager : SingletonMono<PanelManager>
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
        }

        class PanelStub
        {
            public PanelAsset Asset { get; set; }
            public Panel Instance { get; set; }
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
                        }
                    }
                }
            }

            public PanelStub()
            {

            }
        }

        [SerializeField]
        Camera m_UICamera;
        public Camera UICamera { get { return m_UICamera; } }
        List<PanelAsset> mAssets = new List<PanelAsset>();

        [SortingLayerField]
        [SerializeField]
        int m_NormalSortingLayer;

        [SortingLayerField]
        [SerializeField]
        int m_StatusSortingLayer;

        [SortingLayerField]
        [SerializeField]
        int m_DialogSortingLayer;

        [SerializeField]
        PanelMask m_EventMask = new PanelMask();


        List<PanelStub> mStatus = new List<PanelStub>();
        List<PanelStub> mPanels = new List<PanelStub>();
        List<PanelStub> mDialogs = new List<PanelStub>();

        List<PanelStub> mClosing = new List<PanelStub>();

        PanelStub mFocusStub = null;

        protected override void Awake()
        {
            base.Awake();
            Panel[] panels = GetComponentsInChildren<Panel>();
            int len = panels == null ? 0 : panels.Length;
            for(int i = 0; i < len; i++)
            {
                PanelAsset asset = new PanelAsset(panels[i]);
                if (asset.IsUsable)
                {
                    mAssets.Add(asset);
                }
            }
            UpdateMask();
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
#if UNITY_EDITOR
                Debug.LogErrorFormat("Add Panel Asset(id:{0}, name:{1}) is exists.", asset.Id, asset.Name);
#endif
                return;
            }
            mAssets.Add(asset);
        }

        public void RemoveAsset(PanelAsset asset)
        {
            if (asset == null)
                return;

            mAssets.Remove(asset);
            asset.Release();
        }

        PanelAsset GetAsset(int panelId)
        {
            for (int i = 0; i < mAssets.Count; i++)
            {
                PanelAsset asset = mAssets[i];
                if (asset.Id == panelId)
                    return asset;
            }
            return null;
        }

        PanelAsset GetAsset(string assetName)
        {
            for(int i = 0; i < mAssets.Count; i++)
            {
                PanelAsset asset = mAssets[i];
                if (asset.Name == assetName)
                    return asset;
            }
            return null;
        }

        Panel OpenPanelAsStatus(PanelAsset asset)
        {
            return OpenPanelFor(mStatus, m_StatusSortingLayer, asset, null, false);
        }

        Panel OpenPanelAsDialog(PanelAsset asset, IPanelMessager request)
        {
            return OpenPanelFor(mDialogs, m_DialogSortingLayer, asset, request, true);
        }

        Panel OpenPanelAsNormal(PanelAsset asset, IPanelMessager request)
        {
            if (mDialogs.Count > 0)
                return null;
            return OpenPanelFor(mPanels, m_NormalSortingLayer, asset, request, true);
        }

        // asset must be usable.
        Panel OpenPanelFor(List<PanelStub> list, int sortLayer, PanelAsset asset, IPanelMessager request, bool useFocus)
        {
            Panel panel = asset.InstantiateAsset();
            if (panel == null || panel.IsClosing())
                return null;
            bool open;
            if (request != null)
                open = panel.OnPanelOpenForResult(request);
            else
                open = panel.OnPanelOpen();
            if (!open)
            {
                asset.UnuseAsset(panel);
                return null;
            }
            int close = GetPanelIndex(mClosing, panel);
            if (close != -1)
                mClosing.RemoveAt(close);
            panel.transform.SetParent(transform, false);

            int old = GetPanelIndex(list, panel);
            if (old != -1)
                list.RemoveAt(old);
            PanelStub stub = new PanelStub();
            stub.Asset = asset;
            stub.Instance = panel;
            list.Add(stub);
            PanelStub prev = GetRecentPanel(list, 1);
            Canvas prevcan = prev == null ? null : prev.Instance.GetCanvas();
            Canvas can = panel.GetCanvas();
            can.renderMode = RenderMode.ScreenSpaceCamera;
            can.worldCamera = m_UICamera;
            can.sortingLayerID = sortLayer;
            can.sortingOrder = prevcan == null ? 1 : prevcan.sortingOrder + 2;
            if (useFocus)
            {
                if (mFocusStub != null)
                    mFocusStub.HasFocus = false;
                mFocusStub = stub;
                mFocusStub.HasFocus = true;
            }
            if (asset.IsUseMask)
                UpdateMask();
            return panel;
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

        bool ClosePanelFor(List<PanelStub> list, PanelStub stub, bool useFocus)
        {
            if (stub.Instance.IsClosing())
                return true;
            bool close = stub.Instance.OnPanelClose();
            if (!close)
                return false;
            list.Remove(stub);
            if (stub.Instance.IsClosing())
                mClosing.Add(stub);
            else
                stub.Asset.UnuseAsset(stub.Instance);
            if (useFocus)
            {
                if (mFocusStub == stub)
                {
                    mFocusStub.HasFocus = false;
                    mFocusStub = null;
                }
                if (mFocusStub == null && list.Count > 0)
                {
                    mFocusStub = GetRecentPanel(list, 0);
                    mFocusStub.HasFocus = true;
                }
            }
            if (stub.Asset.IsUseMask)
                UpdateMask();
            return true;
        }

        void UpdateMask()
        {
            if (m_EventMask.m_RootCanvas == null)
                return;
            Panel basepanel = GetMaskPanel(mDialogs);
            if (basepanel == null)
                basepanel = GetMaskPanel(mPanels);
            if(basepanel != null)
            {
                Canvas can = basepanel.GetCanvas();
                m_EventMask.m_RootCanvas.sortingLayerID = can.sortingLayerID;
                m_EventMask.m_RootCanvas.sortingOrder = can.sortingOrder - 1;
                m_EventMask.m_RootCanvas.enabled = true;
                if(m_EventMask.m_TargetGraphic != null)
                {
                    Color color = m_EventMask.m_TargetGraphic.color;
                    color.a = m_EventMask.m_MaskAlpha * basepanel.MaskMultipier;
                    m_EventMask.m_TargetGraphic.color = color;
                }
            }
            else
            {
                m_EventMask.m_RootCanvas.enabled = false;
            }
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
        }

        #region opend method

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

        static Panel FindPanel(List<PanelStub> list, FilterDelegate<Panel> filter)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                PanelStub stub = list[i];
                if (filter(stub.Instance))
                    return stub.Instance;
            }
            return null;
        }
        
        public static Panel FindPanel(FilterDelegate<Panel> filter)
        {
            if (filter == null)
                return null;
            PanelManager inst = Instance;
            if (inst == null)
                return null;
            Panel panel;
            panel = FindPanel(inst.mPanels, filter);
            if (panel != null)
                return panel;
            panel = FindPanel(inst.mDialogs, filter);
            if (panel != null)
                return panel;
            panel = FindPanel(inst.mStatus, filter);
            if (panel != null)
                return panel;
            return null;
        }

        public static Panel OpenPanel(int id, IPanelMessager request = null)
        {
            PanelManager inst = Instance;
            if (inst == null)
                return null;
            PanelAsset asset = inst.GetAsset(id);
            if (asset == null || !asset.IsUsable)
                return null;
            else if (asset.Mode == EPanelMode.Dialog)
                return inst.OpenPanelAsDialog(asset, request);
            else if (asset.Mode == EPanelMode.Status)
                return inst.OpenPanelAsStatus(asset);
            else if (asset.Mode == EPanelMode.Normal)
                return inst.OpenPanelAsNormal(asset, request);
            else
                return null;
        }

        public static Panel OpenPanel(string panelName, IPanelMessager request = null)
        {
            PanelManager inst = Instance;
            if (inst == null)
                return null;
            PanelAsset asset = inst.GetAsset(panelName);
            if (asset == null || !asset.IsUsable)
                return null;
            else if (asset.Mode == EPanelMode.Dialog)
                return inst.OpenPanelAsDialog(asset, request);
            else if (asset.Mode == EPanelMode.Status)
                return inst.OpenPanelAsStatus(asset);
            else if (asset.Mode == EPanelMode.Normal)
                return inst.OpenPanelAsNormal(asset, request);
            else
                return null;
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
            else if(panel.m_Mode == EPanelMode.Status)
            {
                index = inst.GetPanelIndex(inst.mStatus, panel);
                return index == -1 ? false : inst.ClosePanelAsStatus(inst.mStatus[index]);
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
