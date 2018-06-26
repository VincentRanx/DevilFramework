using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public class PanelManager : SingletonMono<PanelManager>
    {
        class PanelStub
        {
            public int ProfabId { get; set; }
            public Panel Instance { get; set; }
            public EPanelMode Mode { get; set; }
            public EPanelProperty Properties { get; set; }
        }

        [SerializeField]
        Camera m_UICamera;
        public Camera UICamera { get { return m_UICamera; } }
        List<PanelAsset> mAssets = new List<PanelAsset>();

        List<PanelStub> mStatus = new List<PanelStub>();
        List<PanelStub> mPanels = new List<PanelStub>();

        // recentCoutner: 0:last 1, 1:last 2, 2:last 3
        PanelStub GetRecentPanel(int recentCounter)
        {
            int n = mPanels.Count - 1 - recentCounter;
            if (n >= 0)
                return mPanels[n];
            else
                return null;
        }

        PanelAsset GetAsset(int panelId)
        {
            int index = GlobalUtil.BinsearchIndex((x) => mAssets[x].Id - panelId, 0, mAssets.Count);
            return index == -1 ? null : mAssets[index];
        }
        
        bool OpenPanel(PanelAsset asset, EPanelMode mode)
        {

            Panel panel = asset.InstantiateAsset();
            if (panel == null)
                return false;

            return false;
        }
        
    }
}
