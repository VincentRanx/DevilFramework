using Devil.GamePlay;
using Devil.Utility;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    [CustomEditor(typeof(Panel), true)]
    public class PanelInspector : Editor
    {
        Panel mTarget;
        StringBuilder mBuilder = new StringBuilder();

        string GetPanelInfo(Panel panel)
        {
            mBuilder.Remove(0, mBuilder.Length);
            mBuilder.Append("Mode:{").Append((int)panel.m_Mode).Append("} Property:{").Append((int)panel.Properties).Append("}");
            return mBuilder.ToString();
        }

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
            GUILayout.Label(string.Format("Name Hash(id): {0}", StringUtil.ToHash(mTarget.name)));
            GUILayout.Label(GetPanelInfo(mTarget));
        }
    }
}