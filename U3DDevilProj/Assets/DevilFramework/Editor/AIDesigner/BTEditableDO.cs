using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{

    public abstract class BTEditableDO
    {
        public BehaviourTreeEditor editor { get; private set; }
        public abstract bool DoEditWithUndo();
        public abstract void UndoEdit();

        public static T New<T>(BehaviourTreeEditor editor) where T: BTEditableDO,new()
        {
            var t = new T();
            t.editor = editor;
            return t;
        }
    }

    public class BTAddNode : BTEditableDO
    {
        public BTNode context;
        public Vector2 position;
        public AIModules.Module mod;

        bool isDo;
        BTNode newNode;
        
        public override bool DoEditWithUndo()
        {
            if (isDo)
                return false;
            isDo = true;
            BTNode asset;
            switch (mod.CategoryId)
            {
                case AIModules.CATE_CONDITION:
                    if (context == null || !(context.Asset is BTNodeAsset))
                    {
                        //EditorUtility.DisplayDialog("Error", string.Format("此处无法添加 \"{0}\"。", mod.Title), "OK");
                        editor.Tip.Show(string.Format("!!!无法添加 \"{0}\"。", mod.Title), 5);
                        return false;
                    }
                    else
                    {
                        asset = editor.TargetTree.EditorCreateNode(mod.ModuleType, position);
                        ((BTNodeAsset)context.Asset).EditorConditionIds.Add(asset.Identify);
                        editor.EditNodes((x) =>
                        {
                            if (x.GetNode() == context)
                                x.Resize();
                        });
                        newNode = asset;
                        editor.RequestChild(null);
                        return true;
                    }
                case AIModules.CATE_COMPOSITE:
                case AIModules.CATE_TASK:
                    if (editor.PresentParentRequest != null && !mod.IsController)
                    {
                        editor.Tip.Show(string.Format("!!!无法添加\"{0}\"", mod.Title), 5);
                        return false;
                    }
                    asset = editor.TargetTree.EditorCreateNode(mod.ModuleType, position);
                    if (asset.Asset is BTNodeAsset)
                    {
                        var node = new BTNodeGUI(editor, asset);
                        editor.AIGraph.AddElement(node);
                    }
                    if (editor.PresentChildRequest == editor.RootNode)
                    {
                        editor.TargetTree.m_RootId = asset.Identify;
                    }
                    else if (editor.PresentChildRequest != null && editor.PresentChildRequest.GetNode() != null)
                    {
                        var parent = editor.PresentChildRequest.GetNode();
                        editor.TargetTree.EditorResetParent(asset.Identify, parent.Identify);
                    }
                    else if (editor.PresentParentRequest != null && editor.PresentParentRequest.GetNode() != null)
                    {
                        var child = editor.PresentParentRequest.GetNode();
                        editor.TargetTree.EditorResetParent(child.Identify, asset.Identify);
                    }
                    newNode = asset;
                    editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
                    editor.RequestChild(null);
                    editor.SetSelections((x) => x.GetNode() == asset);
                    return true;
                case AIModules.CATE_SERVICE:
                    if (editor.TargetTree.EditorContainsNodeType(mod.ModuleType))
                    {
                        editor.Tip.Show("已经存在该服务", 5);
                        return false;
                    }
                    asset = editor.TargetTree.EditorCreateNode(mod.ModuleType, position);
                    editor.RootNode.Resize();
                    return true;
                default:
                    return false;
            }
         
        }

        public override void UndoEdit()
        {
            if (isDo)
            {
                isDo = false;
                editor.TargetTree.EditorDeleteNode(newNode);
                newNode = null;
                editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
            }
        }
    }

    public class BTModifyParent : BTEditableDO
    {
        bool isDo;

        public bool isParentRoot;
        public BTNode child;
        public BTNode parent;

        int oldRoot;
        BTNode oldparent;

        public override bool DoEditWithUndo()
        {
            if (isDo || child == null)
                return false;
            if (isParentRoot)
            {
                oldRoot = editor.TargetTree.m_RootId;
                if (oldRoot == child.Identify)
                    return false;
                editor.TargetTree.m_RootId = child.Identify;
                isDo = true;
            }
            else
            {
                editor.TargetTree.EditorResetParent(child.Identify, parent == null ? 0 : parent.Identify);
                isDo = true;
            }
            editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
            return true;
        }

        public override void UndoEdit()
        {
            if (isDo)
            {
                if (isParentRoot)
                {
                    editor.TargetTree.m_RootId = oldRoot;
                }
                else
                {
                    editor.TargetTree.EditorResetParent(child.Identify, oldparent == null ? 0 : oldparent.Identify);
                }
                editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
            }
        }
    }
    
    public class BTDeleteNode : BTEditableDO
    {
        bool isDo;
        List<BehaviourNode> selections = new List<BehaviourNode>();
        public void SetSelection(IList<BehaviourNode> lst)
        {
            selections.Clear();
            selections.AddRange(lst);
        }

        public override bool DoEditWithUndo()
        {
            if (isDo)
                return false;
            isDo = true;
            for (int i = selections.Count - 1; i >= 0; i--)
            {
                var t = selections[i].GetNode();
                if (t != null)
                {
                    editor.TargetTree.EditorDeleteNode(t);
                    editor.AIGraph.RemoveElement(selections[i]);
                }
            }
            editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
            editor.ClearSelection();
            return true;
        }

        public override void UndoEdit()
        {

        }
    }

    public class BTCopy : BTEditableDO
    {
        public Vector2 deltaPosition;
        List<BTNode> selection;
        bool isDo;

        public int SelectionCount { get { return selection == null ? 0 : selection.Count; } }
        
        public Vector2 GetSelectionPositin()
        {
            var rect = new Rect();
            for (int i = 0; i < selection.Count; i++)
            {
                if (i == 0)
                {
                    rect.position = selection[i].position;
                    rect.size = Vector2.zero;
                }
                else
                {
                    var pos = selection[i].position;
                    rect.xMin = Mathf.Min(pos.x, rect.xMin);
                    rect.xMax = Mathf.Max(pos.x, rect.xMax);
                    rect.yMin = Mathf.Min(pos.y, rect.yMin);
                    rect.yMax = Mathf.Max(pos.y, rect.yMax);
                }
            }
            return new Vector2(rect.center.x, rect.yMin);
        }

        public void SetSelection(List<BehaviourNode> selection)
        {
            if (this.selection == null)
                this.selection = new List<BTNode>();
            else
                this.selection.Clear();
            foreach(var t in selection)
            {
                var node = t.GetNode();
                if (node != null)
                    this.selection.Add(node);
            }
        }

        public override bool DoEditWithUndo()
        {
            if (isDo || SelectionCount == 0)
                return false;
            var nodes = editor.TargetTree.EditorPaste(selection, deltaPosition);
            foreach(var t in nodes)
            {
                if(t.Asset is BTNodeAsset)
                {
                    var newitem = new BTNodeGUI(editor, t);
                    editor.AIGraph.AddElement(newitem);
                }
            }
            editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
            editor.SetSelections((x) =>
            {
                return GlobalUtil.FindIndex(nodes, (y) => y == x.GetNode()) != -1;
            });
            return false;
        }

        public override void UndoEdit()
        {
        }
    }
}