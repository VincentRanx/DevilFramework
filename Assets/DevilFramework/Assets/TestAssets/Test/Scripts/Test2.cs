using Devil.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test2 : MonoBehaviour
{
    private void Start()
    {
        PanelAsset asset = new PanelAsset(1, "PanelA", "PanelA", EPanelMode.Normal, EPanelProperty.SingleInstance);
        PanelManager.Instance.AddPanelAsset(asset);
        asset = new PanelAsset(2, "PanelB", "PanelB", EPanelMode.Dialog, 0);
        PanelManager.Instance.AddPanelAsset(asset);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SwitchPanel("PanelA", 1, Input.GetKey(KeyCode.LeftShift));
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
            SwitchPanel("PanelB", 2, Input.GetKey(KeyCode.LeftShift));
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchPanel("PanelC", 2, Input.GetKey(KeyCode.LeftShift));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PanelManager.ReleasePanels();
        }
    }

    void SwitchPanel(string panelName, int identifier, bool open)
    {
        Panel panel;
        if (open)
        {
            panel = PanelManager.OpenPanel(panelName);
        }
        else
        {
            panel = PanelManager.FindPanel((x) => x.m_CustomIdentifier == identifier);
            PanelManager.ClosePanel(panel);
        }
        if (panel != null)
            panel.m_CustomIdentifier = identifier;
    }
    
}
