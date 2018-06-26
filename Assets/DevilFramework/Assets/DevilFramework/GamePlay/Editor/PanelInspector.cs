using Devil.GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(Panel), true)]
    public class PanelInspector : Editor
    {
        Panel mTarget;
        private void OnEnable()
        {
            mTarget = target as Panel;
        }

        public override void OnInspectorGUI()
        {
            PanelManager mgr = PanelManager.Instance;
            if (mgr != null && mTarget.transform.IsChildOf(mgr.transform))
            {
                Canvas can = mTarget.GetCanvas();
                can.worldCamera = mgr.UICamera;
                if (can.worldCamera != null)
                    can.renderMode = RenderMode.ScreenSpaceCamera;
            }
            base.OnInspectorGUI();
        }
    }
}