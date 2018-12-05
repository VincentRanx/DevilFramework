using Devil.AI;
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

    public class AddNode : BTEditableDO
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
                        using (var ser = new SerializedObject(context.Asset))
                        {
                            var pro = ser.FindProperty(BTNodeAsset.P_CONDITION);
                            pro.arraySize++;
                            pro = pro.GetArrayElementAtIndex(pro.arraySize - 1);
                            pro.intValue = asset.Identify;
                            ser.ApplyModifiedPropertiesWithoutUndo();
                        }
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
                        asset.Parent = parent;
                    }
                    else if (editor.PresentParentRequest != null && editor.PresentParentRequest.GetNode() != null)
                    {
                        var child = editor.PresentParentRequest.GetNode();
                        child.Parent = asset;
                    }
                    newNode = asset;
                    editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
                    editor.RequestChild(null);
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

    public class ModifyParent : BTEditableDO
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
                if (child.Parent == parent)
                    return false;
                oldparent = child.Parent;
                child.Parent = parent;
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
                    child.Parent = oldparent;
                }
                editor.AddDelayTask(BehaviourTreeEditor.ACT_UPDATE_WIRES, editor.Wires.UpdateWires);
            }
        }
    }


    public class DeleteNode : BTEditableDO
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
}