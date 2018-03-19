using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph<T> where T : class
{
    public delegate bool PathFilter(int from, int to);
    public delegate bool NodeFilter(int nodeIndex);

    List<T> mNodes; // 值
    List<uint>[] mLinks; // 连接
    int mRootNode;

    HashSet<uint> mQuery;
    Stack<uint> mPath;

    public Graph(int layers = 1)
    {
        mNodes = new List<T>();
        mLinks = new List<uint>[layers];
        for(int i = 0; i < mLinks.Length; i++)
        {
            mLinks[i] = new List<uint>();
        }
        mRootNode = 0;
        mQuery = new HashSet<uint>();
        mPath = new Stack<uint>();
    }

    public int NodeLength { get { return mNodes.Count; } }

    public T this[int index]
    {
       get { return index >= 0 && index < mNodes.Count ? mNodes[index] : null; }
    }

    public int Layers { get { return mLinks.Length; } }

    public int PathLength(int layer)
    {
        return mLinks[layer].Count;
    }

    public T Root { get { return mRootNode < mNodes.Count && mRootNode >= 0 ? mNodes[mRootNode] : null; } }

    public int RootIndex { get { return mRootNode; } }

    public void PathAt(int layer, int index, out int from, out int to)
    {
        if (index >= 0 && index < mLinks[layer].Count)
        {
            uint link = mLinks[layer][index];
            from = (int)((link & 0xffff0000u) >> 16);
            to = (int)(link & 0xffffu);
        }
        else
        {
            from = -1;
            to = -1;
        }
    }

    public int IndexOf(T node)
    {
        return mNodes.IndexOf(node);
    }

    public void SetRootNodeIndex(int index)
    {
        mRootNode = index;
    }

    public int AddNode(T node)
    {
        if (node == null || mNodes.Contains(node))
            return -1;
        mNodes.Add(node);
        return mNodes.Count - 1;
    }

    public void RemoveNode(int index)
    {
        if (index >= 0 && index < mNodes.Count)
        {
            mNodes.RemoveAt(index);
            int from, to;
            for (int l = 0; l < mLinks.Length; l++)
            {
                for (int i = mLinks[l].Count - 1; i >= 0; i--)
                {
                    uint link = mLinks[l][i];
                    from = (int)((link & 0xffff0000u) >> 16);
                    to = (int)(link & 0xffff);
                    if (from == index || to == index)
                    {
                        mLinks[l].RemoveAt(i);
                        continue;
                    }
                    if (from > index)
                        from--;
                    if (to > index)
                        to--;
                    link = ((uint)from << 16) | (uint)to;
                    mLinks[l][i] = link;
                }
            }
        }
    }

    public void RemoveNode(NodeFilter filter)
    {
        for(int i = mNodes.Count - 1; i >= 0; i--)
        {
            if (filter(i))
                RemoveNode(i);
        }
    }

    public void RemoveNode(T node)
    {
        int index = mNodes.IndexOf(node);
        RemoveNode(index);
    }

    public void Clear()
    {
        for(int i = 0; i < mLinks.Length; i++)
        {
            mLinks[i].Clear();
        }
        mNodes.Clear();
        mQuery.Clear();
        mPath.Clear();
        mRootNode = 0;
    }

    public void AddPath(int layer, T from, T to)
    {
        int i0 = mNodes.IndexOf(from);
        if (i0 < 0)
            return;
        int i1 = mNodes.IndexOf(to);
        if (i1 < 0)
            return;
        uint link = ((uint)i0 << 16) | (uint)i1;
        if (!mLinks[layer].Contains(link))
            mLinks[layer].Add(link);
    }

    public void AddPath(int layer, int from, int to)
    {
        if (from < 0 || to < 0)
            return;
        uint link = ((uint)from << 16) | (uint)to;
        if (!mLinks[layer].Contains(link))
            mLinks[layer].Add(link);
    }

    public void RemovePath(int layer, T from, T to)
    {
        int i0 = mNodes.IndexOf(from);
        if (i0 < 0)
            return;
        int i1 = mNodes.IndexOf(to);
        if (i1 < 0)
            return;
        uint link = ((uint)i0 << 16) | (uint)i1;
        mLinks[layer].Remove(link);
    }

    public void RemovePath(int layer, int from, int to)
    {
        if (from < 0 || to < 0)
            return;
        uint link = ((uint)from << 16) | (uint)to;
        mLinks[layer].Remove(link);
    }

    public void RemovePath(int layer, PathFilter filter)
    {
        int from, to;
        for (int i = mLinks[layer].Count - 1; i >= 0; i--)
        {
            PathAt(layer, i, out from, out to);
            if (filter(from, to))
                mLinks[layer].RemoveAt(i);
        }
    }

    public int GetChildCount(int layer, int index)
    {
        if (index < 0 || index >= mNodes.Count)
            return 0;
        int ret = 0;
        int from, to;
        for (int i = 0; i < mLinks[layer].Count; i++)
        {
            uint link = mLinks[layer][i];
            from = (int)((link & 0xffff0000u) >> 16);
            to = (int)(link & 0xffffu);
            if (from == index)
            {
                ret++;
            }
        }
        return ret;
    }

    public int GetChildCount(int layer, T node)
    {
        int index = mNodes.IndexOf(node);
        return GetChildCount(layer, index);
    }

    public void GetAllChildren(int layer, int index, ICollection<T> children)
    {
        if (index < 0 || index >= mNodes.Count)
            return;
        int from, to;
        for (int i = 0; i < mLinks[layer].Count; i++)
        {
            uint link = mLinks[layer][i];
            from = (int)((link & 0xffff0000u) >> 16);
            to = (int)(link & 0xffffu);
            if (from == index)
            {
                children.Add(mNodes[to]);
            }
        }
    }

    public void GetAllChildrenIndex(int layer, int index, ICollection<int> children)
    {
        if (index < 0 || index >= mNodes.Count)
            return;
        int from, to;
        for (int i = 0; i < mLinks[layer].Count; i++)
        {
            uint link = mLinks[layer][i];
            from = (int)((link & 0xffff0000u) >> 16);
            to = (int)(link & 0xffffu);
            if (from == index)
            {
                children.Add(to);
            }
        }
    }

    public void GetAllChildren(int layer, T value, ICollection<T> children)
    {
        int index = mNodes.IndexOf(value);
        GetAllChildren(layer, index, children);
    }

    public int GetParentCount(int layer, int index)
    {
        if (index < 0 || index >= mNodes.Count)
            return 0;
        int ret = 0;
        int from, to;
        for (int i = 0; i < mLinks[layer].Count; i++)
        {
            uint link = mLinks[layer][i];
            from = (int)((link & 0xffff0000u) >> 16);
            to = (int)(link & 0xffffu);
            if (to == index)
            {
                ret++;
            }
        }
        return ret;
    }

    public int GetParentCount(int layer, T node)
    {
        int index = mNodes.IndexOf(node);
        return GetParentCount(layer, index);
    }

    public void GetAllParent(int layer, int index, ICollection<T> parents)
    {
        if (index < 0 || index >= mNodes.Count)
            return;
        int from, to;
        for (int i = 0; i < mLinks[layer].Count; i++)
        {
            uint link = mLinks[layer][i];
            from = (int)((link & 0xffff0000u) >> 16);
            to = (int)(link & 0xffffu);
            if (to == index)
            {
                parents.Add(mNodes[from]);
            }
        }
    }

    public void GetAllParentIndex(int layer, int index, ICollection<int> parents)
    {
        if (index < 0 || index >= mNodes.Count)
            return;
        int from, to;
        for (int i = 0; i < mLinks[layer].Count; i++)
        {
            uint link = mLinks[layer][i];
            from = (int)((link & 0xffff0000u) >> 16);
            to = (int)(link & 0xffffu);
            if (to == index)
            {
                parents.Add(from);
            }
        }
    }

    public void GetAllParent(int layer, T node, ICollection<T> parents)
    {
        int index = mNodes.IndexOf(node);
        GetAllParent(layer, index, parents);
    }

    public bool FindPath(int layer, T from, T to)
    {
        int f, t;
        f = IndexOf(from);
        if (f < 0)
            return false;
        t = IndexOf(to);
        if (t < 0)
            return false;
        return FindPath(layer, f, t);
    }

    public bool FindPath(int layer, int from, int to)
    {
        mPath.Clear();
        mQuery.Clear();
        uint f = (uint)from << 16;
        uint link;
        while (true)
        {
            for (int i = 0; i < mLinks[layer].Count; i++)
            {
                link = mLinks[layer][i];
                if ((link & 0xffff0000u) == f && mQuery.Add(link))
                {
                    if ((link & 0xffff) == to)
                        return true;
                    mPath.Push(link);
                }
            }
            if (mPath.Count == 0)
                break;
            link = mPath.Pop();
            f = (link & 0xffffu) << 16;
        }
        return false;
    }

    public bool FindInversePath(int layer, int from, int to)
    {
        mPath.Clear();
        mQuery.Clear();
        uint f = (uint)from;
        uint link;
        uint t = ((uint)to) << 16;
        while (true)
        {
            for (int i = 0; i < mLinks[layer].Count; i++)
            {
                link = mLinks[layer][i];
                if ((link & 0xffffu) == f && mQuery.Add(link))
                {
                    if ((link & 0xffff0000u) == t)
                        return true;
                    mPath.Push(link);
                }
            }
            if (mPath.Count == 0)
                break;
            link = mPath.Pop();
            f = (link & 0xffff0000u) >> 16;
        }
        return false;
    }

}
