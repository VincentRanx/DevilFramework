using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Devil.AI;

namespace DevilEditor
{
    public class BehaviourRootGUI : PaintElement
    {

        bool mUseEvents;
        public bool Selected { get; set; }
        BehaviourTreeDesignerWindow mWindow;
        bool mDrag;
        string mTitle;

        public BehaviourRootGUI(BehaviourTreeDesignerWindow window) : base()
        {
            mWindow = window;
        }

        public void UpdateLocalData()
        {
            if (mWindow.BehaviourAsset == null)
            {
                mTitle = "<color=#808080><b>[NO TREE]</b></color>";
            }
            else
            {
                mTitle = string.Format("<b>{0}</b>",  mWindow.BehaviourAsset.name);
            }
            Rect rect = new Rect();
            Installizer.titleStyle.fontSize = 30;
            rect.size = Installizer.SizeOfTitle(mTitle) + new Vector2(20, 60);
            rect.position = -0.5f * rect.size;
            LocalRect = rect;
        }

        void ModifyThisAsParent()
        {
            PaintElement ele = mWindow.GetNodeBTParent(mWindow.EditTarget);
            if (ele != null)
            {
                mWindow.TreeGraph.RemovePath(0, ele, mWindow.EditTarget);
            }
            mWindow.TreeGraph.AddPath(0, this, mWindow.EditTarget);
            mWindow.BeginEditNode(null, BehaviourTreeDesignerWindow.ENodeEditMode.none);
            mWindow.RebuildExecutionOrder();
        }

        public override void OnGUI(Rect clipRect)
        {
            GUI.Label(GlobalRect, "", Selected ? "flow node 0 on" : "flow node 0");
            Rect rect = new Rect();
            rect.size = new Vector2(LocalRect.width - 20, LocalRect.height - 35) * GlobalScale;
            rect.center = GlobalRect.center;
            if (rect.size.y > 1)
            {
                GUI.Label(rect, "", "flow node 6");
                GUI.Label(rect, "", "Icon.OutlineBorder");
                Installizer.titleStyle.normal.textColor = Color.white;
                Installizer.titleStyle.fontSize = Mathf.Max(1, (int)(30 * GlobalScale));
                Installizer.titleContent.text = mTitle;
                GUI.Label(rect, Installizer.titleContent, Installizer.titleStyle);
            }

            rect.size = new Vector2(LocalRect.width - 30, 15) * GlobalScale;
            if (rect.size.y > 1)
            {
                rect.center = new Vector2(GlobalRect.center.x, GlobalRect.yMax - rect.size.y * 0.5f);
                bool inrect = rect.Contains(mWindow.GlobalMousePosition);
                if(mWindow.TreeGraph.GetChildCount(0, this) > 0)
                {
                    GUI.Label(rect, "", "textarea");
                }
                else if (mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_parent && inrect)
                {
                    if (GUI.Button(rect, "", "flow node 0 on"))
                        ModifyThisAsParent();
                }
                else if (mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.none)
                {
                    if (GUI.Button(rect, "", inrect ? "flow node 0 on" : "textarea"))
                    {
                        mWindow.BeginEditNode(this, BehaviourTreeDesignerWindow.ENodeEditMode.modify_child);
                    }
                }
                else
                {
                    GUI.Label(rect, "", "textarea");
                }
            }
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            if (mWindow.BehaviourAsset != null && !Application.isPlaying)
                EditorGUIUtility.PingObject(mWindow.BehaviourAsset);
            return true;
        }
    }
}