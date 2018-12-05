using UnityEngine;
using UnityEngine.UI;

namespace Devil.GamePlay
{
    public class PanelLauncher : MonoBehaviour
    {
        public bool m_SendRequest;
        public int m_RequestId;
        public string m_PanelName;
        public int m_DisableGroup;
        int mPanelId;
        bool mUseId;
        bool mOpenPanel;

        private void Start()
        {
            if (int.TryParse(m_PanelName, out mPanelId))
                mUseId = true;
            Button btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            var panel = PanelManager.TopDialogOrPanel;
            if (panel != null && panel.name == m_PanelName)
                return;
            if (m_DisableGroup != 0)
            {
                PanelManager.ClosePanelByGroup(m_DisableGroup);
                PanelManager.WaittingForOpenPanel = true;
            }
            mOpenPanel = true;
        }

        private void Update()
        {
            if (mOpenPanel)
            {
                if (PanelManager.HasAnyPanelClosing)
                    return;
                mOpenPanel = false;
                IPanelIntent req = null;
                if (m_SendRequest)
                {
                    PanelIntent intent;
                    intent.requestId = m_RequestId;
                    intent.requestData = this;
                    intent.handler = null;
                    req = intent;
                }
                if (mUseId)
                {
                    var panel = PanelManager.OpenPanel(mPanelId, req);
                    if (panel != null)
                        m_PanelName = panel.name;
                }
                else if (!string.IsNullOrEmpty(m_PanelName))
                    PanelManager.OpenPanel(m_PanelName, req);
            }
        }
    }
}