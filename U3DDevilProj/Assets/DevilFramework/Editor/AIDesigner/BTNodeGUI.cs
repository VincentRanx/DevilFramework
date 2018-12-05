using Devil.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class BTNodeGUI : BehaviourNode
    {
        BTNode mNode;
        BTNodeAsset mAsset;
        AIModules.Module mMod;
        List<BTNode> mConditionNodes = new List<BTNode>();
        List<AIModules.Module> mConditionMods = new List<AIModules.Module>();
        List<string> mConditionNames = new List<string>();

        Vector2 mTitleSize; // 标题大小
        Vector2 mDetailSize;
        float mDecorHeight; // 修饰高度
        Vector2 mCommentSize;
        string mComment;

        BTNode mContext;
        bool mResize;
        bool mResized;
        string mDetail = "";
        // 运行时
        BTNode mRuntime;

        public BTNodeGUI(BehaviourTreeEditor editor, BTNode asset) : base(editor)
        {
            mNode = asset;
            mAsset = mNode.Asset as BTNodeAsset;
            mMod = AIModules.Get(asset);
            color = mMod.color;
            title = mMod.Title;
            enableChild = mAsset != null && mAsset.EnableChild;
            enableParent = mAsset != null;
            mContext = mNode;
            mDetail = GetDetail();
            Resize();
        }
        
        string GetDetail()
        {
            var s = mAsset.DisplayContent;
            if (string.IsNullOrEmpty(s))
                s = mMod.Detail;
            return s;
        }

        string GetConditionName(BTNode node, AIModules.Module mod)
        {
            var n = node.Asset as BTConditionAsset;
            if (n == null)
                return mod.Title;
            var s = n.DisplayName;
            if (string.IsNullOrEmpty(s))
                s = mod.Title;
            return s;
        }

        // get conditions or services
        void GetDecoratorModules()
        {
            mConditionNodes.Clear();
            mConditionMods.Clear();
            mConditionNames.Clear();
            for (int i = 0; i < mAsset.ConditionCount; i++)
            {
                var id = mAsset.GetConditionId(i);
                var node = editor.TargetTree.GetNodeById(id);
                if (node != null)
                {
                    var mod = AIModules.Get(node);
                    mConditionNodes.Add(node);
                    mConditionNames.Add(GetConditionName(node, mod));
                    mConditionMods.Add(mod);
                }
            }
        }

        public override void Resize()
        {
            mResize = true;
            DontClip = true;
        }

        void DoResize()
        {
            mResize = false;
            mResized = true;
            GetDecoratorModules();
            Vector2 content = new Vector2();
            mTitleSize = CalculateTitleSize(title);
            mTitleSize.x = Mathf.Max(mTitleSize.x, 70);
            mTitleSize.y = Mathf.Max(ICON_SIZE.y, mTitleSize.y);
            if (string.IsNullOrEmpty(mDetail))
                mDetailSize = Vector2.zero;
            else
            {
                mDetailSize = CalculateSubtitleSize(mDetail);
                mDetailSize.y += SPACE.y;
            }
            content.x = Mathf.Max(mTitleSize.x + ICON_SIZE.x + SPACE.x * 2.5f, mDetailSize.x + SPACE.x * 2);
            content.y += mTitleSize.y + SPACE.y * 2f + mDetailSize.y;

            Vector2 size = Vector2.zero;
            for (int i = 0; i < mConditionNames.Count; i++)
            {
                size = CalculateSubtitleSize(mConditionNames[i]);
                size.x += size.y;
                content.x = Mathf.Max(content.x, size.x + SPACE.x * 2);
            }
            mDecorHeight = size.y + SPACE.y;
            content.y += mDecorHeight * mConditionNames.Count;

            var pos = new Rect();
            float sy = 0;
            if (enableParent)
                sy += SLOT_SIZE.y;
            if (enableChild)
                sy += SLOT_SIZE.y;
            pos.size = content + new Vector2(SLOT_SIZE.x, sy);
            pos.width = Mathf.Max(pos.width, 100);
            pos.position = mNode.position - 0.5f * Vector2.right * pos.width;
            LocalRect = pos;
            DontClip = false;
        }

        protected override bool IsHighlight
        {
            get
            {
                var n = GetRuntimeNode();
                if (n == null)
                    n = mNode;
                return Selection.activeObject == n.Asset;
            }
        }

        protected override void MoveDelta(Vector2 delta)
        {
            base.MoveDelta(delta);
            var pos = LocalPosition;
            if (Vector2.SqrMagnitude(mNode.position - pos) >= 1f)
            {
                mNode.position = pos;
            }
        }

        public override BTNode GetNode()
        {
            return mNode;
        }

        public override BTNode GetContext()
        {
            return mContext;
        }

        public override BTNode GetRuntimeNode()
        {
            return mRuntime;
        }

        public override void OnGUI(Rect clipRect)
        {
            if (mResize)
            {
                var res = mResized;
                DoResize();
                if (!res)
                    return;
            }
            base.OnGUI(clipRect);
        }

        void DebugRuntimeState(Rect pos)
        {
            var nd = GetRuntimeNode();
            var looper = editor.Binder.looper;
            var runtime = nd == null ? null : nd.Asset as BTNodeAsset;
            if (runtime == null || looper == null)
                return;
            //var access = looper.EditorAccessed.Contains(runtime);
            //if (!access)
            //    return;
            if(runtime.State == EBTState.running)
            {
                var size = SLOT_SIZE * 1.5f * GlobalScale;
                GUI.DrawTexture(new Rect(GlobalRect.xMax - size.x, GlobalRect.y - size.y, size.x, size.y), AIModules.RunIcon);
            }
            else if(runtime.State == EBTState.success)
            {
                var size = SLOT_SIZE * 1.5f * GlobalScale;
                GUI.DrawTexture(new Rect(GlobalRect.xMax - size.x, GlobalRect.y - size.y, size.x, size.y), AIModules.GoodIcon);
            }
            else if(runtime.State == EBTState.failed)
            {
                var size = SLOT_SIZE * 1.5f * GlobalScale;
                GUI.DrawTexture(new Rect(GlobalRect.xMax - size.x, GlobalRect.y - size.y, size.x, size.y), AIModules.BadIcon);
            }
            if(runtime.State >= EBTState.running)
            {
                var h = mDecorHeight * GlobalScale;
                var dh = Mathf.Min(15, h);
                pos = new Rect(pos.xMax - dh, pos.y, dh, dh);
                for(int i= 0; i < runtime.ConditionCount; i++)
                {
                    var ok = runtime.editor_conditionCache[i];
                    GUI.DrawTexture(pos, ok ? AIModules.GoodIcon : AIModules.BadIcon);
                    pos.y += h;
                }
            }
        }

        protected override void OnContentLayer(Rect rect)
        {
            if (mNode == null || mNode.Asset == null)
            {
                editor.AIGraph.RemoveElement(this);
                return;
            }
            mContext = mNode;
            var space = SPACE * GlobalScale;
            var icon = ICON_SIZE * GlobalScale;
            var decorh = mDecorHeight * GlobalScale;
            var tsize = mTitleSize * GlobalScale;
            var dsize = mDetailSize * GlobalScale;
            var pos = rect;
            QuickGUI.DrawBox(rect, color, selected ? SELECTED_COLOR : BOARDER_COLOR, selected ? 2 : 1, true);
            
            bool raycast;
            float h = rect.y;
            for (int i = 0; i < mConditionNames.Count; i++)
            {
                pos.position = new Vector2(rect.x, h + i * decorh);
                pos.size = new Vector2(rect.width, decorh);
                raycast = pos.Contains(editor.GlobalMousePosition);
                if (raycast)
                    mContext = editor.TargetTree.GetNodeById(mAsset.GetConditionId(i));
                QuickGUI.DrawBox(pos, raycast ? SELECTED_COLOR : mConditionMods[i].color, SELECTED_COLOR, 0);

                pos.size = Vector2.one * decorh * 0.9f;
                GUI.DrawTexture(pos, mConditionMods[i].icon);
                pos.x += decorh;
                GUISubtitle(pos, mConditionNames[i], raycast ? Color.black : SUBTITLE_COLOR, FontStyle.Normal, TextAnchor.MiddleLeft);
                if (mConditionNames[i] != GetConditionName(mConditionNodes[i], mConditionMods[i]))
                    Resize();
            }

            pos = new Rect(rect.x, rect.y + decorh * mConditionNames.Count, rect.width, tsize.y + dsize.y + space.y * 2);
            pos.size = icon;
            pos.position += space;
            GUI.DrawTexture(pos, mMod.icon);
            pos.size = tsize;
            pos.position += Vector2.right * (icon.x + space.x * 0.5f);
            GUITitle(pos, title, FontStyle.Bold, TextAnchor.UpperLeft);
            if(!string.IsNullOrEmpty(mDetail))
            {
                pos.size = dsize;
                pos.position = new Vector2(rect.x + space.x, pos.y + tsize.y + 0.5f * space.y);
                GUISubtitle(pos, mDetail, FontStyle.Normal, TextAnchor.MiddleLeft);
            }
            var del = GetDetail();
            if(del != mDetail)
            {
                mDetail = del;
                Resize();
            }
            if (editor.Binder.IsRunning())
            {
                var runtime = editor.Binder.runtime;
                mRuntime = runtime == null ? null : runtime.GetNodeById(mNode.Identify);
                DebugRuntimeState(rect);
            }
            else
            {
                mRuntime = null;
            }
        }
        
        public override void DrawComment(bool raycast)
        {
            var com = mAsset.m_Comment;
            if (string.IsNullOrEmpty(com))
                return;
            if (com != mComment)
            {
                mComment = com;
                mCommentSize = CalculateSubtitleSize(mComment);
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
            GUISubtitle(rect, mComment, Color.white * (raycast ? 1f : 0.7f), raycast ? FontStyle.Normal : FontStyle.Italic, TextAnchor.MiddleLeft);
        }

        public override bool EnableParentAs(BehaviourNode node)
        {
            if (base.EnableParentAs(node))
            {
                if (node == null || node == editor.RootNode)
                    return true;
                var p = node.GetNode();
                return p != null && !p.IsChildOf(mNode);
            }
            return false;
        }

        public override void ModifyParentAs(BehaviourNode node)
        {
            base.ModifyParentAs(node);
            var todo = BTEditableDO.New<ModifyParent>(editor);
            todo.isParentRoot = node == editor.RootNode;
            todo.child = mNode;
            todo.parent = node == null ? null : node.GetNode();
            editor.DoEdit(todo);
        }
        
    }
}