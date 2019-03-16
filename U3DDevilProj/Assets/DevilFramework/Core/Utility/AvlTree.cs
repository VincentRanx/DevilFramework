#define TEST
//#define NODE_POOL
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.Utility
{
    /// <summary>
    /// 平衡二叉树，默认通过 Hash 值作为比较 id，也可通过一个 IdentifierDelegate 的委托类型实现比较值的覆盖方法
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public sealed class AvlTree<T> : ICollection<T>
    {
        public int Count { get { return mRoot == null ? 0 : mRoot.count; } }
        public int Deep { get { return mRoot == null ? 0 : mRoot.deep; } }
        public bool IsReadOnly { get { return false; } }
        public NodeInfo Root
        {
            get
            {
                NodeInfo info;
                info.data = mRoot;
                return info;
            }
        }

        public NodeInfo Min
        {
            get
            {
                if (mRoot == null)
                    return default(NodeInfo);
                NodeInfo info;
                info.data = mRoot.Min;
                return info;
            }
        }

        public NodeInfo Max
        {
            get
            {
                if (mRoot == null)
                    return default(NodeInfo);
                NodeInfo info;
                info.data = mRoot.Max;
                return info;
            }
        }

        private IdentifierDelegate<T> mIdentifier;
        private Node mRoot;
        private Stack<Node> mCache;

        public AvlTree(IdentifierDelegate<T> identifier = null)
        {
            if (identifier == null)
                mIdentifier = (x) => x == null ? 0 : x.GetHashCode();
            else
                mIdentifier = identifier;
            mCache = new Stack<Node>(32);
        }

        public int GetDataId(T data)
        {
            return mIdentifier(data);
        }

        public T Add(T item, out bool replaced)
        {
            var id = mIdentifier(item);
            var newnode = Node.GetNode(id, item);
            if (mRoot == null)
            {
                mRoot = newnode;
            }
            else
            {
                var node = mRoot;
                while (node != null)
                {
                    if (node.id == id)
                    {
                        replaced = true;
                        var ret = node.value;
                        node.value = item;
                        return ret;
                    }
                    else if (node.id > id)
                    {
                        if (node.left == null)
                        {
                            node.AddLeftLeaf(newnode);
                            node.UpdateTree();
                            if (node.parent != null)
                                node.parent.FixBalence();
                            break;
                        }
                        else
                        {
                            node = node.left;
                        }
                    }
                    else
                    {
                        if (node.right == null)
                        {
                            node.AddRightLeaf(newnode);
                            node.UpdateTree();
                            if (node.parent != null)
                                node.parent.FixBalence();
                            break;
                        }
                        else
                        {
                            node = node.right;
                        }
                    }
                }
                while (mRoot.parent != null)
                    mRoot = mRoot.parent;
            }
            replaced = false;
            return default(T);
        }

        public void Add(T item)
        {
            bool replaced;
#if TEST
            var ret =
#endif
                Add(item, out replaced);
#if TEST
            if (replaced)
            {
                Debug.LogWarningFormat("AvlTree<{0}> replaced {1} to {2}", typeof(T).Name, ret, item);
            }
#endif
        }

        public void Clear()
        {
            mCache.Clear();
            var node = mRoot;
            while (node != null)
            {
                mCache.Push(node);
                node = node.left;
            }
            while (mCache.Count > 0)
            {
                var v = mCache.Pop();
                var sub = v.right;
                v.Release();
                while (sub != null)
                {
                    mCache.Push(sub);
                    sub = sub.left;
                }
            }
            mRoot = null;
        }

        public void ClearWithCallback(System.Action<T> callback)
        {
            mCache.Clear();
            var node = mRoot;
            while (node != null)
            {
                mCache.Push(node);
                node = node.left;
            }
            while (mCache.Count > 0)
            {
                var v = mCache.Pop();
                var sub = v.right;
                callback(v.value);
                v.Release();
                while (sub != null)
                {
                    mCache.Push(sub);
                    sub = sub.left;
                }
            }
            mRoot = null;
        }

        /// <summary>
        /// 查找节点信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public NodeInfo FindNode(T item)
        {
            return FindNodeById(mIdentifier(item));
        }

        public NodeInfo FindNodeById(int id)
        {
            var node = mRoot;
            while (node != null)
            {
                if (node.id == id)
                {
                    NodeInfo info;
                    info.data = node;
                    return info;
                }
                else if (node.id < id)
                    node = node.right;
                else
                    node = node.left;
            }
            return default(NodeInfo);
        }
        
        /// <summary>
        /// 回传指定 id 的节点数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetData(int id)
        {
            var node = mRoot;
            while (node != null)
            {
                if (node.id == id)
                    return node.value;
                else if (node.id < id)
                    node = node.right;
                else
                    node = node.left;
            }
            return default(T);
        }

        public bool Contains(T item)
        {
            return ContainsId(mIdentifier(item));
        }

        public bool ContainsId(int id)
        {
            var node = mRoot;
            while (node != null)
            {
                if (node.id == id)
                    return true;
                else if (node.id < id)
                    node = node.right;
                else
                    node = node.left;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            mCache.Clear();
            var node = mRoot;
            while (node != null)
            {
                mCache.Push(node);
                node = node.left;
            }
            for (int i = arrayIndex; i < array.Length; i++)
            {
                if (mCache.Count > 0)
                {
                    var v = mCache.Pop();
                    array[i] = v.value;
                    v = v.right;
                    while(v != null)
                    {
                        mCache.Push(v);
                        v = v.left;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public bool RemoveById(int id)
        {
            var node = mRoot;
            while(node != null)
            {
                if(node.id == id)
                {
                    mRoot = node.RemoveAndReturnDirty();
                    if(mRoot != null)
                    {
                        mRoot.UpdateTree();
                        mRoot.FixBalence();
                    }
                    while (mRoot != null && mRoot.parent != null)
                        mRoot = mRoot.parent;
                    Node.CacheNode(node);
                    return true;
                }
                else if(node.id < id)
                {
                    node = node.right;
                }
                else
                {
                    node = node.left;
                }
            }
            return false;
        }
        
        public bool Remove(T item)
        {
            return RemoveById(mIdentifier(item));
        }

        public IEnumerator<T> GetEnumerator(int startId, int endId)
        {
            return new RangeEnumerator(this, startId, endId);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string GetInfo(int start, int end)
        {
            var buff = StringUtil.GetBuilder();
            buff.Append("AVL[Count:").Append(Count).Append(", Deep:").Append(Deep).Append(", Root:")
                .Append(mRoot == null ? " " : mRoot.id.ToString()).Append("]");
            mCache.Clear();
            var node = mRoot;
            while (node != null)
            {
                if(node.id < start)
                {
                    node = node.right;
                }
                else if(node.id > start)
                {
                    mCache.Push(node);
                    node = node.left;
                }
                else
                {
                    mCache.Push(node);
                    break;
                }
            }
            int n = -1;
            while (mCache.Count > 0)
            {
                node = mCache.Pop();
                if (node.id > end)
                    break;
                if (node.id >= start)
                {
                    if (n == -1)
                        n = node.Index;
                    buff.Append("\n [").Append(n++).Append("] ").Append(node);
                }
                node = node.right;
                while (node != null)
                {
                    mCache.Push(node);
                    node = node.left;
                }
            }
            return StringUtil.ReleaseBuilder(buff);
        }

#if TEST
        public override string ToString()
        {
            return GetInfo(0, 128);
        }
#endif

        /// <summary>
        /// 节点属性
        /// </summary>
        public struct NodeInfo
        {
            internal Node data;
            public bool isExists { get { return data != null && data.count > 0; } }
            public T value { get { return data == null ? default(T) : data.value; } }
            public int deep { get { return data == null ? 0 : data.deep; } }
            public int count { get { return data == null ? 0 : data.count; } }
            public int id { get { return isExists ? data.id : 0; } }
            public int index { get { return isExists ? data.Index : -1; } }

            public NodeInfo parent
            {
                get
                {
                    if (data == null)
                        return default(NodeInfo);
                    NodeInfo info;
                    info.data = data.parent;
                    return info;
                }
            }

            public NodeInfo leftChild
            {
                get
                {
                    if (data == null)
                        return default(NodeInfo);
                    NodeInfo info;
                    info.data = data.left;
                    return info;
                }
            }

            public NodeInfo rightChild
            {
                get
                {
                    if (data == null)
                        return default(NodeInfo);
                    NodeInfo info;
                    info.data = data.right;
                    return info;
                }
            }

            public NodeInfo min
            {
                get
                {
                    if (data == null)
                        return default(NodeInfo);
                    NodeInfo info;
                    info.data = data.Min;
                    return info;
                }
            }

            public NodeInfo max
            {
                get
                {
                    if (data == null)
                        return default(NodeInfo);
                    NodeInfo info;
                    info.data = data.Max;
                    return info;
                }
            }

            public override string ToString()
            {
                if (data == null)
                    return "Not exists Avl node.";
                else
                    return data.ToString();
            }

            public override int GetHashCode()
            {
                return id;
            }

            public override bool Equals(object obj)
            {
                if (obj is NodeInfo)
                    return ((NodeInfo)obj).id == this.id;
                else
                    return false;
            }

            public static bool operator ==(NodeInfo a, NodeInfo b)
            {
                return a.id == b.id;
            }

            public static bool operator !=(NodeInfo a, NodeInfo b)
            {
                return a.id != b.id;
            }
        }

        // 平衡树节点实例
        internal class Node
        {
#if NODE_POOL
            private static Node cachedNode = null; // cache0 <-- cache1 <-- cache2 <-- cache3 <-- cachedNode
#endif
            public static Node GetNode(int id, T value)
            {
#if NODE_POOL
                if(cachedNode != null)
                {
                    var node = cachedNode;
                    cachedNode = cachedNode.parent;
                    node.id = id;
                    node.value = value;
                    node.parent = null;
                    return node;
                }
                else
#endif
                    return new Node(id, value);
            }

            public static void CacheNode(Node node)
            {
#if NODE_POOL
                node.Release();
                node.parent = cachedNode;
                cachedNode = node;
#endif
            }

            public T value;
            public Node left;
            public Node right;
            public Node parent;

            public int id; // id
            public int deep; // 深度
            public int count; // 节点数

            private Node(int id, T value)
            {
                this.value = value;
                this.id = id;
                deep = 1;
                count = 1;
            }

            public int BalenceFactor
            {
                get
                {
                    int l = left == null ? 0 : left.deep;
                    int r = right == null ? 0 : right.deep;
                    return l - r;
                }
            }

            public int Index
            {
                get
                {
                    var index = left == null ? 0 : left.count;
                    var node = this;
                    var par = parent;
                    while(par != null)
                    {
                        if (par.right == node)
                        {
                            index += 1;
                            if (par.left != null)
                                index += par.left.count;
                        }
                        node = par;
                        par = node.parent;
                    }
                    return index;
                }
            }

            public Node Max
            {
                get
                {
                    var node = this;
                    while (node.right != null)
                        node = node.right;
                    return node;
                }
            }

            public Node Min
            {
                get
                {
                    var node = this;
                    while (node.left != null)
                        node = node.left;
                    return node;
                }
            }

            public void Release()
            {
                left = null;
                right = null;
                parent = null;
                id = 0;
                value = default(T);
                deep = 0;
                count = 0;
            }

            public void FixBalence()
            {
                var node = this;
                while (node != null)
                {
                    var balence = node.BalenceFactor;
                    // 左旋转
                    if (balence < -1)
                    {
                        var r = node.right;
                        if (r.BalenceFactor > 0)
                        {
                            r.RotateRight();
                            r.UpdateSelf();
                        }
                        node.RotateLeft();
                        node.UpdateTree();
                        break;
                    }
                    else if (balence > 1)
                    {
                        var l = node.left;
                        if (l.BalenceFactor < 0)
                        {
                            l.RotateLeft();
                            l.UpdateSelf();
                        }
                        node.RotateRight();
                        node.UpdateTree();
                        break;
                    }
                    else
                    {
                        node = node.parent;
                    }
                }
            }
            
            public void AddLeftLeaf(Node newnode)
            {
                left = newnode;
                newnode.parent = this;
            }
            
            public void AddRightLeaf(Node newnode)
            {
                right = newnode;
                newnode.parent = this;
            }

            public void UpdateSelf()
            {
                deep = Mathf.Max(left == null ? 0 : left.deep, right == null ? 0 : right.deep) + 1;
                count = (left == null ? 0 : left.count) +
                    (right == null ? 0 : right.count) + 1;
            }

            public void UpdateTree()
            {
                var p = this;
                while (p != null)
                {
                    p.UpdateSelf();
                    p = p.parent;
                }
            }

            // 左旋转
            public void RotateLeft()
            {
                var p = parent;
                var rl = right.left;
                right.parent = p;
                if (p != null)
                {
                    if (p.left == this)
                        p.left = right;
                    else
                        p.right = right;
                }
                parent = right;
                right.left = this;
                right = rl;
                if (rl != null)
                    rl.parent = this;
            }

            // 右旋转
            public void RotateRight()
            {
                var p = parent;
                var lr = left.right;
                left.parent = p;
                if (p != null)
                {
                    if (p.left == this)
                        p.left = left;
                    else
                        p.right = left;
                }
                parent = left;
                left.right = this;
                left = lr;
                if (lr != null)
                    lr.parent = this;
            }

            public Node RemoveAndReturnDirty()
            {
                var par = parent;
                var l = left;
                var r = right;
                bool isleft = false;
                if(par != null)
                {
                    isleft = par.left == this;
                    if (isleft)
                        par.left = null;
                    else
                        par.right = null;
                }
                if (left != null)
                    left.parent = null;
                if (right != null)
                    right.parent = null;
                parent = null;
                left = null;
                right = null;
                if (l == null && r == null)
                    return par;
                if(l == null)
                {
                    UseParent(r, par, isleft);
                    return par == null ? r : par;
                }
                if(r == null)
                {
                    UseParent(l, par, isleft);
                    return par == null ? l : par;
                }
                if(l.deep < r.deep)
                {
                    var t = r.Min;
                    if(t == r)
                    {
                        r.left = l;
                        l.parent = r;
                        UseParent(r, par, isleft);
                        return r;
                    }
                    var su = t.parent;
                    var tr = t.right;
                    su.left = tr;
                    if (tr != null)
                        tr.parent = su;
                    t.left = l;
                    l.parent = t;
                    t.right = r;
                    r.parent = t;
                    UseParent(t, par, isleft);
                    return su;
                }
                else
                {
                    var t = l.Max;
                    if(t == l)
                    {
                        l.right = r;
                        r.parent = l;
                        UseParent(l, par, isleft);
                        return l;
                    }
                    var su = t.parent;
                    var tl = t.left;
                    su.right = tl;
                    if (tl != null)
                        tl.parent = su;
                    t.left = l;
                    l.parent = t;
                    t.right = r;
                    r.parent = t;
                    UseParent(t, par, isleft);
                    return su;
                }
            }

            private void UseParent(Node asChild, Node asParent, bool isLeft)
            {
                asChild.parent = asParent;
                if (asParent != null)
                {
                    if (isLeft)
                        asParent.left = asChild;
                    else
                        asParent.right = asChild;
                }
            }

#if TEST
            public override string ToString()
            {
                return string.Format("Avl[P:{0}\t L:{1}\t R:{2}\t Dep:{3}\t Bal:{4}\t Num:{5}]\t  \"{6}\"",
                    parent == null ? "-" : parent.id.ToString(),
                    left == null ? "-" : left.id.ToString(),
                    right == null ? "-" : right.id.ToString(),
                    deep.ToString(),
                    BalenceFactor.ToString(),
                    count.ToString(),
                    value.ToString());
            }
#endif

        }

        // 区间迭代器
        internal class RangeEnumerator : IEnumerator<T>
        {
            AvlTree<T> avl;
            Node current;
            Stack<Node> visitStack;
            int startId;
            int endId;

            public RangeEnumerator(AvlTree<T> avl, int startId, int endId)
            {
                visitStack = new Stack<Node>(avl.Deep);
                this.startId = startId;
                this.endId = endId;
                this.avl = avl;
                Reset();
            }

            public T Current { get { return current == null ? default(T) : current.value; } }

            object IEnumerator.Current { get { return current == null ? default(T) : current.value; } }

            public void Dispose()
            {
                visitStack.Clear();
                current = null;
                avl = null;
            }

            public bool MoveNext()
            {
                while (visitStack.Count > 0)
                {
                    var node = visitStack.Pop();
                    if (node.id > endId)
                        return false;
                    var c = node;
                    node = node.right;
                    while (node != null)
                    {
                        visitStack.Push(node);
                        node = node.left;
                    }
                    if (c.id >= startId)
                    {
                        current = c;
                        return true;
                    }
                }
                current = null;
                return false;
            }

            public void Reset()
            {
                visitStack.Clear();
                var node = avl.mRoot;
                while(node != null)
                {
                    if(node.id < startId)
                    {
                        node = node.right;
                    }
                    else if(node.id > startId)
                    {
                        visitStack.Push(node);
                        node = node.left;
                    }
                    else
                    {
                        visitStack.Push(node);
                        break;
                    }
                }
            }
        }

        // 迭代器
        internal class Enumerator : IEnumerator<T>
        {
            AvlTree<T> avl;
            Node current;
            Stack<Node> visitStack; //

            public T Current { get { return current == null ? default(T) : current.value; } }

            object IEnumerator.Current { get { return Current; } }

            public Enumerator(AvlTree<T> avl)
            {
                this.avl = avl;
                if (avl != null)
                    visitStack = new Stack<Node>(avl.Deep);
                Reset();
            }

            public void Dispose()
            {
                avl = null;
                current = null;
            }

            public bool MoveNext()
            {
                if (visitStack == null || visitStack.Count == 0)
                    return false;
                current = visitStack.Pop();
                if (current != null)
                {
                    var node = current.right;
                    while (node != null)
                    {
                        visitStack.Push(node);
                        node = node.left;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                if (visitStack != null)
                {
                    visitStack.Clear();
                    var node = avl.mRoot;
                    while (node != null)
                    {
                        visitStack.Push(node);
                        node = node.left;
                    }
                }
            }
        }
    }
}