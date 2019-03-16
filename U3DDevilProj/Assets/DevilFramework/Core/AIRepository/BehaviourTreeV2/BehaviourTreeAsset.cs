using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [CreateAssetMenu(fileName = "New Behaviour Asset", menuName = "AI/Behaviour Tree")]
    [System.Serializable]
    public class BehaviourTreeAsset : ScriptableObject, System.IDisposable
    {
        public string m_Comment = "";

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
            asset.m_Comment = m_Comment;
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
        
        public string EditorMissingAssets()
        {
            var buf = StringUtil.GetBuilder();
            foreach (var t in m_BTNodes)
            {
                if (t.Asset == null)
                {
                    buf.Append(StringUtil.Concat(t.ModName, "_", t.Identify, ", "));
                }
            }
            return StringUtil.ReleaseBuilder(buf);
        }

        public void EditorResetParent(int childid, int parentid)
        {
            var child = GetNodeById(childid);
            var parent = GetNodeById(parentid);
            if (child != null)
            {
                var oldp = child.EditorParentId;
                var newp = parent == null ? 0 : parent.Identify;
                if (oldp != newp)
                {
                    child.EditorParentId = newp;
                    var old = oldp == 0 ? null : GetNodeById(oldp);
                    if (old != null)
                        old.EditorChildrenIds.Remove(childid);
                    if (parent != null && !parent.EditorChildrenIds.Contains(childid))
                        parent.EditorChildrenIds.Add(childid);
                }
            }
            else if (parent != null)
            {
                parent.EditorChildrenIds.Remove(childid);
            }
        }

        public bool EditorContainsNodeType(System.Type type)
        {
            var tname = type.Name;
            foreach(var t in m_BTNodes)
            {
                if (t.ModName == tname)
                    return true;
            }
            return false;
        }

        public void EditorResort()
        {
            GlobalUtil.Sort(m_BTNodes, (x, y) => x.Identify <= y.Identify ? -1 : 1);
            foreach (var t in m_BTNodes)
            {
                GlobalUtil.Sort(t.EditorChildrenIds, (x, y) =>
                {
                    var a = GetNodeById(x);
                    var b = GetNodeById(y);
                    return a.position.x <= b.position.x ? -1 : 1;
                });
            }
            m_Sorted = true;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void EditorMergeTo(string path, BehaviourTreeAsset asset)
        {
            if (asset == this || asset == null)
                return;
            if (!string.IsNullOrEmpty(path))
            {
                asset.m_BTNodes.Clear();
                var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var t in assets)
                {
                    if (t != asset)
                        DestroyImmediate(t, true);
                }
            }
            else
            {
                foreach(var t in asset.m_BTNodes)
                {
                    if (t.Asset != null)
                        DestroyImmediate(t.Asset, true);
                }
                asset.m_BTNodes.Clear();
            }
            for(int i = 0; i < m_BTNodes.Count; i++)
            {
                var node = m_BTNodes[i].Clone(asset);
                asset.m_BTNodes.Add(node);
            }
            asset.m_Comment = m_Comment;
            asset.m_RootId = m_RootId;
            asset.m_Sorted = m_Sorted;
            asset.m_RootPosition = m_RootPosition;
            UnityEditor.EditorUtility.SetDirty(asset);
        }
        
        public BTNode[] EditorPaste(List<BTNode> selection, Vector2 deltaPos)
        {
            HashSet<BTNode> toCopy = new HashSet<BTNode>();
            foreach(var t in selection)
            {
                if (toCopy.Add(t))
                {
                    var nasset = t.Asset as BTNodeAsset;
                    if(nasset != null)
                    {
                        foreach(var c in nasset.EditorConditionIds)
                        {
                            var cond = GetNodeById(c);
                            toCopy.Add(cond);
                        }
                    }
                }
            }
            
            BTNode[] src = new BTNode[toCopy.Count];
            BTNode[] copies = new BTNode[src.Length];
            toCopy.CopyTo(src);
            int id0 = m_BTNodes.Count > 0 ? (m_BTNodes[m_BTNodes.Count - 1].Identify + 1) : 1;
            for (int i = 0; i < src.Length; i++)
            {
                copies[i] = src[i].Copy(this, id0 + i, deltaPos);
            }
            m_BTNodes.AddRange(copies);
            for (int i = 0; i < copies.Length; i++)
            {
                var p = src[i].Parent;
                // 关联父节点
                var index = GlobalUtil.FindIndex(src, (x) => x == p);
                if (index == -1)
                    EditorResetParent(copies[i].Identify, 0);
                else
                    EditorResetParent(copies[i].Identify, copies[index].Identify);
                // 关联条件节点
                var nasset = src[i].Asset as BTNodeAsset;
                if (nasset == null)
                    continue;
                for (int n = 0; n < nasset.ConditionCount; n++)
                {
                    var cindex = GlobalUtil.FindIndex(src, (x) => x.Identify == nasset.GetConditionId(n));
                    ((BTNodeAsset)copies[i].Asset).EditorSetCondition(n, copies[cindex].Identify);
                }
            }
            return copies;
        }
        
        public void EditorDeleteNode(BTNode node)
        {
            if (node.Asset != null && node.Asset.TreeAsset != this)
                return;
            EditorResetParent(node.Identify, 0);
            var delId = node.Identify;
            // goto delete
            HashSet<int> removeIds = new HashSet<int>();
            removeIds.Add(node.Identify);
            var cnode = node.Asset as BTNodeAsset;
            if (cnode != null)
            {
                // clean parents and conditions
                foreach (var c in cnode.EditorConditionIds)
                {
                    removeIds.Add(c);
                }
                foreach (var t in m_BTNodes)
                {
                    if (t == node)
                        continue;
                    if (t.EditorParentId == node.Identify)
                        EditorResetParent(t.Identify, 0);
                }
            }
            // do clean condition
            if (node.isCondition)
            {
                foreach (var t in m_BTNodes)
                {
                    var nasset = t.Asset as BTNodeAsset;
                    if (nasset != null)
                        nasset.EditorConditionIds.RemoveAll((x) => removeIds.Contains(x));
                }
            }
            // do delete
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

        // 修复资源引用
        public void EditorResovleAsset()
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
                return;
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] == null)
                {
                    Object.DestroyImmediate(assets[i], true);
                    assets[i] = null;
                }
            }
            List<BTNode> toclean = new List<BTNode>();
            foreach(var t in m_BTNodes)
            {
                if(t.Asset == null)
                {
                    t.EditorResovleAsset(assets);
                }
                if (t.Asset == null)
                    toclean.Add(t);
            }
            foreach(var t in toclean)
            {
                EditorDeleteNode(t);
            }
            UnityEditor.EditorUtility.SetDirty(this);
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