using Devil.GamePlay;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NormalPanel : Panel
{
    bool mClosing = false;
    public override bool IsClosing() {  return mClosing;  }
    public CanvasGroup m_Group;

    public override bool OnPanelClose()
    {
        Debug.LogFormat("{0} Closed", name);
        mClosing = true;
        RaycastEvent = false;
        StartCoroutine("CloseProcess");
        return true;
    }

    public override void OnPanelGainFoucs()
    {
        Debug.LogFormat("{0} Get Focus", name);
    }
    
    public override void OnPanelLostFocus()
    {
        Debug.LogFormat("{0} Lost Focus", name);
    }

    public override bool OnPanelOpen()
    {
        Debug.LogFormat("{0} Opened", name);
        gameObject.SetActive(true);
        RaycastEvent = true;
        if (m_Group != null)
            m_Group.alpha = 1;
        return true;
    }

    public override bool OnPanelOpenForResult(IPanelMessager sender)
    {
        Debug.LogFormat("{0} Open for Result {1}(sender:{2})", name, sender.RequestId, sender);
        gameObject.SetActive(true);
        return true;
    }

    IEnumerator CloseProcess()
    {
        yield return null;
        float t = 0;
        if (m_Group == null)
            yield break;
        while (t < 2)
        {
            t += Time.deltaTime;
            m_Group.alpha = 1-Mathf.Clamp01(t * 0.5f);
            yield return null;
        }
        mClosing = false;
        gameObject.SetActive(false);
    }
}
