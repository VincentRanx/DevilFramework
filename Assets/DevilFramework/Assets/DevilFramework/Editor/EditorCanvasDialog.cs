using UnityEngine;
using Devil.Utility;

namespace DevilEditor
{
    public class EditorCanvasTip: PaintElement
    {
        //flow overlay box
        static ObjectBuffer<EditorCanvasTip> mBuffer = new ObjectBuffer<EditorCanvasTip>(10, () => new EditorCanvasTip());
        static EditorCanvasTip mLastTip;

        public static EditorCanvasTip NewTip(string text, float time)
        {
            if (mLastTip != null && mLastTip.mText == text)
            {
                mLastTip.mTick = JDateTime.NowMillies;
                return mLastTip;
            }
            EditorCanvasTip tip = mBuffer.AnyTarget;
            tip.mDuration = (long)(time * 1000);
            tip.mText = text;
            mLastTip = tip;
            return tip;
        }

        string mText;
        long mDuration;
        long mTick;

        private EditorCanvasTip()
        {
        }

        public void Show(EditorGUICanvas canvas, Vector2 localPos)
        {
            if (Parent != null || canvas == null)
                return;
            Installizer.contentStyle.fontSize = 12;
            Vector2 size = Installizer.SizeOfContent(mText ?? "") + new Vector2(10, 10);
            Rect r = new Rect();
            r.size = size;
            r.position = localPos - r.size * 0.5f;
            LocalRect = r;
            mTick = JDateTime.NowMillies;
            canvas.AddElement(this);
        }

        public override void OnGUI(Rect clipRect)
        {
            GUI.Label(GlobalRect, "", "flow overlay box");
            Installizer.contentContent.text = mText;
            Installizer.contentStyle.fontSize = 12;
            Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(GlobalRect, Installizer.contentContent, Installizer.contentStyle);
            if(JDateTime.NowMillies-mTick > mDuration)
            {
                ((EditorGUICanvas)Parent).RemoveElement(this);
            }
        }

        public override void OnRemoved()
        {
            mBuffer.SaveBuffer(this);
            if (mLastTip == this)
                mLastTip = null;
        }
    }

    public class EditorCanvasDialog : PaintElement
    {

        public override void OnGUI(Rect clipRect)
        {
            
        }
    }
}