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

        List<PanelStub> mStatus = new List<PanelStub>();
        List<PanelStub> mPanels = new List<PanelStub>();

        bool OpenPanel(Panel panel, EPanelMode mode, EPanelProperty properties)
        {

            return false;
        }
    }
}
