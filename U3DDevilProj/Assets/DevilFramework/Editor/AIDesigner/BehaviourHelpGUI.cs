using UnityEngine;

namespace DevilEditor
{

    public class BehaviourHelpGUI : PaintElement
    {

        string help = @"<color=#808080><size=13>
【鼠标2】 编辑/添加组件

【C】 添加注释块

【CTRL+点击】 多选

【CTRL+D】 重做(复制)选中组件

【滚轮】 鼠标位置缩放

【CTRL+滚轮】 中心缩放
</size></color>";

        public BehaviourHelpGUI()
        {
            SortOrder = -10;
            DontClip = true;
        }
        
        public override void OnGUI(Rect clipRect)
        {
            //int fsize = GUI.skin.textArea.fontSize;
            //int lsize = GUI.skin.label.fontSize;
            //FontStyle fstyle = GUI.skin.textArea.fontStyle;
            //FontStyle lstyle = GUI.skin.label.fontStyle;

            //GUI.skin.label.fontStyle = FontStyle.Normal;
            //GUI.skin.label.fontSize = 13;
            GUI.Label(new Rect(clipRect.xMin + 10, clipRect.yMin + 40, clipRect.width - 20, clipRect.height - 40), help);

        }
    }
}