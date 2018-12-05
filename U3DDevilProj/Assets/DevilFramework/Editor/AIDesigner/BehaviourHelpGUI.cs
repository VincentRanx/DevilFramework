using UnityEngine;

namespace DevilEditor
{

    public class BehaviourHelpGUI : PaintElement
    {

        string help = @"<b><color=#a0a0a0>
<size=30>AI Designer</size>

<size=13>【鼠标2】 编辑/添加组件

【SHIFT+点击】 多选

【CTRL+点击】 反选

【ALT+点击】 删除连线

【滚轮】 鼠标位置缩放

【CTRL+滚轮】 中心缩放

【1/2/3/4】 呼出 控制节点/任务/条件/服务 列表

【快捷键】 呼出对应节点列表
</size></color></b>";

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