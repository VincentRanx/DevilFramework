using UnityEngine;

namespace DevilEditor
{
    public class EditorSelection : PaintElement
    {
        string style = "SelectionRect";

        public EditorSelection() : base()
        {
            DontClip = true;
        }

        public override void OnGUI(Rect clipRect)
        {
            GUI.Label(GlobalRect, DevilEditorUtility.EmptyContent, style);
        }
    }
}