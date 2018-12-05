using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [CreateAssetMenu(fileName = "New Behaviour Asset", menuName = "AI/Behaviour Tree")]
    [System.Serializable]
    public class BehaviourTreeAsset : ScriptableObject, System.IDisposable
    {
        [HideInInspector]
        [SerializeField]
        public int m_RootId;

        [HideInInspector]
        [SerializeField]
        List<BTNode> m_BTNodes = new List<BTNode>();

        [HideInInspector]
        [SerializeField]
        public Vector2 m_RootPosition;

        [HideInInspector]
        [SerializeField]
        bool m_Sorted;

        List<IBTService> mServices = new List<IBTService>();
        public List<IBTService> Services { get { return mServices; } }

        public int RootId { get { return m_RootId; } }
        public IBTNode Root
        {
            get
            {
                var t = m_RootId == 0 ? null : GetNodeById(m_RootId);
                return t == null ? null : t.Asset as IBTNode;
            }
        }

        public BTNode GetNodeById(int id)
        {
            if (m_Sorted)
                return GlobalUtil.Binsearch(m_BTNodes, id);
            else
                return GlobalUtil.Find(m_BTNodes, (x) => x.Identify == id);
        }

        public int GetNodeIndex(int id)
        {
            if (m_Sorted)
                return GlobalUtil.BinsearchIndex(m_BTNodes, id);
            else
                return GlobalUtil.FindIndex(m_BTNodes, (x) => x.Identify == id);
        }

        public void GetAllNodes(ICollection<BTNode> collection, FilterDelegate<BTNode> filter = null)
        {
            if (filter == null)
            {
                for (int i = 0; i < m_BTNodes.Count; i++)
                {
                    collection.Add(m_BTNodes[i]);
                }
            }
            else
            {
                for (int i = 0; i < m_BTNodes.Count; i++)
                {
                    if (filter(m_BTNodes[i]))
                        collection.Add(m_BTNodes[i]);
                }
            }
        }

        public BehaviourTreeAsset Clone()
        {
            var asset = CreateInstance<BehaviourTreeAsset>();
            asset.name = string.Format("{0}_{1}", name, asset.GetInstanceID().ToString("x"));
            asset.m_RootId = m_RootId;
            asset.m_Sorted = m_Sorted;
            asset.m_RootPosition = m_RootPosition;
            for (int i = 0; i < m_BTNodes.Count; i++)
            {
                var node = m_BTNodes[i].Clone(asset);
                asset.m_BTNodes.Add(node);
            }
            return asset;
        }

        public void Prepare(BehaviourTreeRunner.AssetBinder binder)
        {
            mServices.Clear();
            for(int i= 0; i < m_BTNodes.Count; i++)
            {
                m_BTNodes[i].Asset.OnPrepare(binder, m_BTNodes[i]);
                if (m_BTNodes[i].Asset is IBTService)
                    mServices.Add((IBTService)m_BTNodes[i].Asset);
            }
        }
        
        public void Dispose()
        {
            mServices.Clear();
        }

        public static void DestroyAsset(BehaviourTreeAsset btree, bool immediate = false)
        {
            if (btree == null)
                return;
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(btree);
#endif
            if(immediate)
            {
                foreach(var t in btree.m_BTNodes)
                {
                    if (t.Asset != null)
                        Object.DestroyImmediate(t.Asset, true);
                }
                DestroyImmediate(btree, true);
            }
            else
            {
                foreach (var t in btree.m_BTNodes)
                {
                    if (t.Asset != null)
                        Object.Destroy(t.Asset);
                }
                Destroy(btree);
            }
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(path))
                UnityEditor.AssetDatabase.Refresh();
#endif
        }

#if UNITY_EDITOR
        
        public bool EditorContainsNodeType(System.Type type)
        {
            foreach(var t in m_BTNodes)
            {
                if (t.Asset.GetType() == type)
                    return true;
            }
            return false;
        }

        public void EditorResort()
        {
            GlobalUtil.Sort(m_BTNodes, (x, y) => x.Identify <= y.Identify ? -1 : 1);
            foreach (var t in m_BTNodes)
            {
                GlobalUtil.Sort(t.ChildrenIds, (x, y) =>
                {
                    var a = GetNodeById(x);
                    var b = GetNodeById(y);
                    return a.position.x <= b.position.x ? -1 : 1;
                });
            }
            m_Sorted = true;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void EditorMergeTo(BehaviourTreeAsset asset)
        {
            if (asset == this || asset == null)
                return;
            foreach(var t in asset.m_BTNodes)
            {
                if (t.Asset != null)
                    DestroyImmediate(t.Asset, true);
            }
            asset.m_BTNodes.Clear();
            for(int i = 0; i < m_BTNodes.Count; i++)
            {
                var node = m_BTNodes[i].Clone(asset);
                asset.m_BTNodes.Add(node);
            }
            asset.m_RootId = m_RootId;
            asset.m_Sorted = m_Sorted;
            asset.m_RootPosition = m_RootPosition;
            UnityEditor.EditorUtility.SetDirty(asset);
        }
        
        public void EditorCleanup()
        {
            HashSet<int> removeIds = new HashSet<int>();
            for (int i = m_BTNodes.Count - 1; i >= 0; i--)
            {
                if (m_BTNodes[i].Asset == null)
                {
                    removeIds.Add(m_BTNodes[i].Identify);
                    m_BTNodes.RemoveAt(i);
                }
                else
                {
                    m_BTNodes[i].Asset.TreeAsset = this;
                }
            }
            if (removeIds.Count > 0)
            {
                foreach (var t in m_BTNodes)
                {
                    t.Asset.EditorNodeRemoved(removeIds);
                }
            }
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(path))
            {
                bool reimport = false;
                var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets)
                {
                    if (asset is BTAsset && GlobalUtil.FindIndex(m_BTNodes, (x) => x.Asset == asset) == -1)
                    {
                        DestroyImmediate(asset, true);
                        reimport = true;
                    }
                }
                if (reimport)
                    UnityEditor.AssetDatabase.ImportAsset(path);
            }
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void EditorDeleteNode(BTNode node)
        {
            if (node.Asset != null && node.Asset.TreeAsset != this)
                return;
            var delId = node.Identify;
            // goto delete
            HashSet<int> removeIds = new HashSet<int>();
            removeIds.Add(node.Identify);
            Stack<int> toRemove = new Stack<int>();
            toRemove.Push(node.Identify);
            // collect dependent nodes
            HashSet<int> tmpIds = new HashSet<int>();
            while (toRemove.Count > 0)
            {
                var id = toRemove.Pop();
                var other = GetNodeById(id);
                if (other != null)
                {
                    other.Asset.EditorGetDependentIds(tmpIds);
                    foreach (var tmp in tmpIds)
                    {
                        if (removeIds.Add(tmp))
                        {
                            toRemove.Push(tmp);
                        }
                    }
                    tmpIds.Clear();
                }
            }
            for (int i = m_BTNodes.Count - 1; i >= 0; i--)
            {
                node = m_BTNodes[i];
                if (removeIds.Contains(node.Identify))
                {
                    if (node.Asset != null)
                    {
                        DestroyImmediate(node.Asset);
                    }
                    m_BTNodes.RemoveAt(i);
                }
                else
                {
                    node.Asset.EditorNodeRemoved(removeIds);
                    if (node.ChildrenIds != null)
                        node.ChildrenIds.RemoveAll((x) => removeIds.Contains(x));
                }
            }
            if (delId == m_RootId)
            {
                m_RootId = 0;
                for (int i = 0; i < m_BTNodes.Count; i++)
                {
                    if (m_BTNodes[i].isController)
                    {
                        m_RootId = m_BTNodes[i].Identify;
                        break;
                    }
                    if (m_BTNodes[i].isTask)
                        m_RootId = m_BTNodes[i].Identify;
                }
            }
        }

        public void EditorDeleteAsset(BTAsset asset)
        {
            var node = GlobalUtil.Find(m_BTNodes, (x) => x.Asset == asset);
            if(node != null)
                EditorDeleteNode(node);
        }

        int QueryId()
        {
            if (m_BTNodes.Count == 0)
                return 1;
            for (int i = 0; i < m_BTNodes.Count; i++)
            {
                if (m_BTNodes[i].Identify > i + 1)
                    return i + 1;
            }
            var last = m_BTNodes[m_BTNodes.Count - 1];
            return last.Identify + 1;
        }

        public BTNode EditorCreateNode(System.Type type, Vector2 pos)
        {
            if (Ref.IsTypeInheritedFrom(type, typeof(BTAsset), false))
            {
                var id = QueryId();
                var t = CreateInstance(type) as BTAsset;
                if (t != null)
                {
                    t.name = string.Format("{0}_{1}", type.Name, id.ToString("x"));
                    t.TreeAsset = this;
                    BTNode node = new BTNode(id, t);
                    node.position = pos;
                    m_BTNodes.Insert(id - 1, node);
                    if (m_RootId == 0 && (node.isController || node.isTask))
                        m_RootId = id;
                    return node;
                }
            }
            return null;
        }

#endif
    }
}