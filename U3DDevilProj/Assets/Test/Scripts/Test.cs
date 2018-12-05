using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [TextArea(3, 7)]
    public string m_Text = "";

    public int valueCount = 100000;

    AvlTree<int> tree;

    void InitTree()
    {
        if (tree == null)
        {
            tree = new AvlTree<int>((x) => x);
        }
    }

    [ContextMenu("Clear AVL")]
    void Clear()
    {
        if (tree != null)
            tree.Clear();
    }

    [ContextMenu("Test AVL")]
    void TestAVL()
    {
        InitTree();
        Debug.Log(tree);
        int[] arr = StringUtil.ParseArray(m_Text, ' ');
        if (arr != null)
        {
            foreach (var v in arr)
            {
                Debug.Log("Contains " + v + ": " + tree.ContainsId(v));
            }
        }
    }

    [ContextMenu("Remove AVL")]
    void RemoveAVL()
    {
        InitTree();
        int[] arr = StringUtil.ParseArray(m_Text, ' ');
        if (arr != null)
        {
            foreach (var v in arr)
            {
                tree.Remove(v);
            }
        }
        Debug.Log(tree);
    }

    [ContextMenu("Add AVL")]
    void AddAVL()
    {
        InitTree();
        var c = tree.Count + 1;
        for (int i = 0; i < valueCount; i++)
        {
            tree.Add(c + i);
        }
        Debug.Log(tree);
    }

    [ContextMenu("Print AVL")]
    void PrintAVL()
    {
        InitTree();
        Debug.Log(tree);
        var buf = StringUtil.GetBuilder();
        int num = 0;
        foreach (var t in tree)
        {
            buf.Append(t).Append(" ");
            num++;
            if (num == 20)
            {
                Debug.Log(buf.ToString());
                num = 0;
                buf.Remove(0, buf.Length);
            }
        }
        Debug.Log(StringUtil.ReleaseBuilder(buf));
    }
}
