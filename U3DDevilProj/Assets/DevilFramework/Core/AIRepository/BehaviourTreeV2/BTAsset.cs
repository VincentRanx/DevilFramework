using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [System.Serializable]
    public class BTNode : IIdentified
    {
        [SerializeField]
        int id;
        [SerializeField]
        string moduleName;
        public string ModName
        {
            get
            {
                if (asset != null)
                    moduleName = asset.GetType().FullName;
                return moduleName;
            }
        }
        [SerializeField]
        int parentId;
        public BTNode Parent
        {
            get
            {
                var p = parentId == 0 ? null : asset.TreeAsset.GetNodeById(parentId);
                if (p == null)
                    parentId = 0;
                return p;
            }
            set
            {
                var p = parentId == 0 ? null : asset.TreeAsset.GetNodeById(parentId);
                if (p == value)
                    return;
                if (value == null)
                {
                    parentId = 0;
                }
                else
                {
                    parentId = value.id;
                    if (!value.children.Contains(id))
                        value.children.Add(id);
                }
                if (p != null)
                    p.children.Remove(id);
            }
        }
        [SerializeField]
        List<int> children = new List<int>();
        public List<int> ChildrenIds { get { return children; } }
        public int ChildrenCount { get { return children.Count; } }
        public BTNode ChildAt(int index)
        {
            return asset.TreeAsset.GetNodeById(children[index]);
        }
        public Vector2 position;
        [SerializeField]
        BTAsset asset;
        public BTAsset Asset { get { return asset; } }

        public int Identify { get { return id; } }

        public bool isController { get { return asset is BTControllerAsset; } }

        public bool isTask { get { return asset is BTTaskAsset; } }

        public bool isService { get { return asset is BTServiceAsset; } }

        public bool isCondition { get { return asset is BTConditionAsset; } }
        
        public BTNode() { }

        public BTNode(int id, BTAsset asset)
        {
            this.id = id;
            this.asset = asset;
            moduleName = asset.GetType().Name;
        }

        public bool IsChildOf(BTNode node)
        {
            if (id == 0 || node.id == 0 || node.children.Count == 0)
                return false;
            if (asset == null || node.asset == null)
                return false;
            var tree = asset.TreeAsset;
            if (tree == null || tree != node.asset.TreeAsset)
                return false;
            var p = Parent;
            while (p != null)
            {
                if (p == node)
                    return true;
                p = p.Parent;
            }
            return false;
        }

        public BTNode Clone(BehaviourTreeAsset tree)
        {
            BTNode newnode = new BTNode();
            newnode.id = this.id;
            newnode.moduleName = moduleName;
            newnode.parentId = this.parentId;
            if (children.Count > 0)
                newnode.children.AddRange(children);
            newnode.position = position;
            if (asset != null)
            {
                newnode.asset = Object.Instantiate(asset);
                newnode.asset.name = string.Format("{0}_{1}", asset.GetType().Name, id);
                newnode.asset.TreeAsset = tree;
            }
            else
            {
                newnode.asset = null;
            }
            return newnode;
        }

    }

    public abstract class BTAsset : ScriptableObject 
	{
        [HideInInspector]
        [SerializeField]
        BehaviourTreeAsset m_TreeAsset;
        public BehaviourTreeAsset TreeAsset { get { return m_TreeAsset; } set { m_TreeAsset = value; } }
        public abstract bool EnableChild { get; }
        public abstract void OnPrepare(BehaviourTreeRunner.AssetBinder assetBinder, BTNode node);

#if UNITY_EDITOR
        public virtual void EditorNodeRemoved(ICollection<int> ids) {}
        public virtual void EditorGetDependentIds(ICollection<int> ids) { }
#endif
    }
}