using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace DevilEditor
{
    public class BTRootNodeGUI : BehaviourNode
    {
        Vector2 mTitleSize;
        Vector2 mDetailSize;
        string detail;

        bool mResize;
        //AIModules.Module[] mServices = new AIModules.Module[0];
        List<BTNode> mServiceNodes = new List<BTNode>();
        List<string> mServiceNames = new List<string>();
        List<AIModules.Module> mServiceModules = new List<AIModules.Module>();
        float mDecorHeight;

        BehaviourTreeAsset mAsset;
        BTNode mContext;
        string mComment = "";
        Vector2 mCommentSize;

        public BTRootNodeGUI(BehaviourTreeEditor editor) : base(editor)
        {
            color = new Color(0.3f, 0.3f, 1f);
            title = "AI ROOT";
            detail = "";
            enableParent = false;
            enableChild = true;
            Resize();
        }

        string GetServiceName(BTNode node, AIModules.Module mod)
        {
            var s = node.Asset as BTServiceAsset;
            var name = s == null ? null : s.DisplayName;
            if (string.IsNullOrEmpty(name))
                name = mod.Title;
            return name;
        }

        void GetServices()
        {
            mAsset = editor.TargetTree;
            mServiceNodes.Clear();
            mServiceNames.Clear();
            mServiceModules.Clear();
            if (mAsset != null)
                mAsset.GetAllNodes(mServiceNodes, (x) => x.isService);
            for(int i= 0;i< mServiceNodes.Count;i++)
            {
                var mod = AIModules.Get(mServiceNodes[i]);
                mServiceModules.Add(mod);
                mServiceNames.Add(GetServiceName(mServiceNodes[i], mod));
            }
        }

        public override BTNode GetContext()
        {
            return mContext;
        }

        public override void Resize()
        {
            mResize = true;
            DontClip = true;
        }

        void DoResize()
        {
            mResize = false;
            DontClip = false;
            GetServices();
            mTitleSize = CalculateTitleSize(title, TITLE_SIZE + 4) + SPACE;
            mTitleSize.y = Mathf.Max(ICON_SIZE.y * 1.5f, mTitleSize.y);
            var buf = StringUtil.GetBuilder();
            buf.Append("[TARGET] ").Append(editor.TargetRunner == null ? "[NULL]" : editor.TargetRunner.name);
            buf.Append("\n[ASSET ] ").Append(editor.SourceTree == null ? "[NULL]" : editor.SourceTree.name);
            detail = StringUtil.ReleaseBuilder(buf);
            mDetailSize = CalculateTitleSize(detail) + SPACE;
            Vector2 content;
            content.x = Mathf.Max(mTitleSize.x + ICON_SIZE.x * 1.5f + SPACE.x * 2.5f, mDetailSize.x + SPACE.x * 2, 100);
            content.y = mTitleSize.y + SPACE.y * 2f + mDetailSize.y;

            Vector2 size = Vector2.zero;
            for (int i = 0; i < mServiceNames.Count; i++)
            {
                size = CalculateSubtitleSize(mServiceNames[i]);
                size.x += size.y;
                content.x = Mathf.Max(content.x, size.x + SPACE.x * 2);
            }
            mDecorHeight = size.y + SPACE.y;
            content.y += mDecorHeight * mServiceNames.Count;

            var pos = new Rect();
            pos.size = content + SLOT_SIZE;
            pos.width = Mathf.Max(pos.width, 100);
            Vector2 rootpos;
            if (editor.TargetTree == null)
                rootpos = Vector2.zero;
            else
                rootpos = editor.TargetTree.m_RootPosition;
            pos.position = rootpos - 0.5f * Vector2.right * pos.width;
            LocalRect = pos;
        }

        protected override void OnContentLayer(Rect rect)
        {
            if (mResize)
                DoResize();
            mContext = null;
            var space = SPACE * GlobalScale;
            var icon = ICON_SIZE * GlobalScale * 1.5f;
            var tsize = mTitleSize * GlobalScale;
            var dsize = mDetailSize * GlobalScale;
            var decorh = mDecorHeight * GlobalScale;

            var pos = rect;

            QuickGUI.DrawBox(rect, color, selected ? SELECTED_COLOR : BOARDER_COLOR, 0, true);
            if (editor.Binder.IsRunning())
            {
                pos.width *= Mathf.PingPong(Time.realtimeSinceStartup * 0.5f, 1f);
                GUI.Label(pos, DevilEditorUtility.EmptyContent, "MeLivePlayBar");
            }

            pos.size = icon;
            pos.position = rect.position + space;
            GUI.DrawTexture(pos, AIModules.icon);
            pos.size = tsize;
            pos.x += space.x + icon.x;
            GUITitle(pos, title, TITLE_COLOR, TITLE_SIZE + 4, FontStyle.Bold, TextAnchor.MiddleLeft);
            pos.size = dsize;
            pos.position = rect.position + new Vector2(space.x, tsize.y + space.y);
            GUITitle(pos, detail, Color.white, FontStyle.Normal, TextAnchor.MiddleLeft);

            var raycast = false;
            float h = pos.yMax + space.y;
            for (int i = 0; i < mServiceNames.Count; i++)
            {
                pos.position = new Vector2(rect.x, h + i * decorh);
                pos.size = new Vector2(rect.width, decorh);
                raycast = pos.Contains(editor.GlobalMousePosition);
                if (raycast)
                    mContext = mServiceNodes[i];
                QuickGUI.DrawBox(pos, raycast ? SELECTED_COLOR : mServiceModules[i].color, SELECTED_COLOR, 0);

                pos.size = Vector2.one * decorh * 0.9f;
                GUI.DrawTexture(pos, mServiceModules[i].icon);
                pos.x += decorh;
                GUISubtitle(pos, mServiceNames[i], raycast ? Color.black : SUBTITLE_COLOR, FontStyle.Normal, TextAnchor.MiddleLeft);
                if (GetServiceName(mServiceNodes[i], mServiceModules[i]) != mServiceNames[i])
                    Resize();
            }

        }

        public override void DrawComment(bool raycast)
        {
            if (mAsset == null)
                return;
            var com = mAsset.m_Comment;
            if (string.IsNullOrEmpty(com))
                return;
            if (com != mComment)
            {
                mComment = com;
                mCommentSize = CalculateSubtitleSize(mComment, SUB_SIZE + 2);
            }
            var rect = new Rect();
            var size = mCommentSize * GlobalScale;
            rect.size = size + Vector2.one * 4;
            rect.x = GlobalRect.x;
            rect.y = GlobalRect.y - size.y - 8;
            QuickGUI.DrawBox(rect, Color.gray * (raycast ? 0.7f : 0.5f), Color.black, raycast ? 1 : 0);
            rect.size = size;
            rect.x = GlobalRect.x + 2;
            rect.y = GlobalRect.y - size.y - 6;
            GUISubtitle(rect, mComment, Color.white * (raycast ? 1f : 0.7f), SUB_SIZE + 2, raycast ? FontStyle.Normal : FontStyle.Italic, TextAnchor.MiddleLeft);
        }

        public override bool EnableParentAs(BehaviourNode node)
        {
            return false;
        }

        protected override void MoveDelta(Vector2 delta)
        {
            base.MoveDelta(delta);
            if (editor.TargetTree != null)
            {
                var pos = LocalPosition;
                if (Vector2.SqrMagnitude(editor.TargetTree.m_RootPosition - pos) >= 1f)
                {
                    editor.TargetTree.m_RootPosition = pos;
                }
            }
        }
    }
}