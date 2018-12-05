using UnityEngine;

namespace DevilEditor
{
    public class TipBox : PaintElement
    {
        GUIContent content;
        Vector2 size;
        long time;
        long tick;

        public TipBox() :base()
        {
            content = new GUIContent();
            DontClip = true;
            Visible = false;
        }
        
        public void Show(string text, float time)
        {
            this.content.text = text;
            Visible = true;
            double t = time;
            this.time = (long)(t * 10000000d);
            tick = System.DateTime.Now.Ticks;
        }

        public override void OnGUI(Rect clipRect)
        {
            var style = (GUIStyle)"helpbox";
            if(size.x < 1)
            {
                size = style.CalcSize(content) + Vector2.one * 10f;
            }
            GUI.Label(new Rect(clipRect.x, clipRect.yMax - size.y, size.x, size.y), content, style);
            if (System.DateTime.Now.Ticks - tick > time)
            {
                size = Vector2.zero;
                Visible = false;
            }
        }
    }
}