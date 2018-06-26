using Devil.GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPanel : Panel
{
    public override bool IsPanelOpened { get { return true; } }

    public override bool IsPanelClosed { get { return true; } }

    public override bool OnPanelClose()
    {
        return true;
    }

    public override void OnPanelClosed()
    {
    }

    public override void OnPanelGainFoucs()
    {
    }

    public override void OnPanelLoaded()
    {
    }

    public override void OnPanelLostFocus()
    {
    }

    public override bool OnPanelOpen()
    {
        return true;
    }

    public override void OnPanelOpened()
    {
    }

    public override void OnPanelUnloaded()
    {
    }

    public override void OnReceiveResult(int request, object result)
    {
    }

    public override void OnRequestForResult(int request, object arg)
    {
    }
}
