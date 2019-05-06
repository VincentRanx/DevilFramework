using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Devil.GamePlay.Assistant
{
    public class DataListView : MonoBehaviour, IDataList
	{
        public Transform m_DataRoot;
        public GameObject m_Prefab;

        public bool m_ModifySize;
        public Vector2 m_ItemSize;

        ObjectPool<GameObject> mPool;


        List<GameObject> mDataComponents = new List<GameObject>();
        List<object> mDatas = new List<object>();
        public int DataCount { get { return mDataComponents.Count; } }

        GameObject NewDataSource()
        {
            var go = Instantiate(m_Prefab, m_DataRoot == null ? transform : m_DataRoot);
            if(m_ModifySize)
            {
                var rect = go.transform as RectTransform;
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_ItemSize.x);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_ItemSize.y);
            }
            return go;
        }

        void DestroySource(GameObject dt)
        {
            if(dt != m_Prefab)
                Destroy(dt);
        }

        private void Awake()
        {
            mPool = new ObjectPool<GameObject>(12, NewDataSource, DestroySource);
            if (m_Prefab != null && m_Prefab.activeInHierarchy)
                m_Prefab.SetActive(false);
        }

        private void OnDestroy()
        {
            ClearData();
            if(mPool != null)
                mPool.Clear();
        }

        public void AddData<T>(T data) where T : class
        {
            var go = mPool.Get();
            go.SetActive(true);
            var dt = go.GetComponent<IDataBinder<T>>();
            dt.Data = data;
            dt.Index = mDataComponents.Count;
            mDatas.Add(dt);
            mDataComponents.Add(go);
        }

        public void BindData<T>(IEnumerable<T> iter) where T : class
        {
            ClearData();
            var it = iter.GetEnumerator();
            while (it.MoveNext())
            {
                AddData(it.Current);
            }
        }

        public void ClearData()
        {
            for (int i = 0; i < mDataComponents.Count; i++)
            {
                var t = mDataComponents[i];
                var dt = t.GetComponent<IDataSource>();
                dt.SetData(null);
                t.SetActive(false);
                mPool.Add(mDataComponents[i]);
            }
            mDataComponents.Clear();
            mDatas.Clear();
        }

        public T GetDataComponent<T>(int index) where T : Component
        {
            return mDataComponents[index].GetComponent<T>();
        }

        public GameObject GetDataInstance(int index)
        {
            return mDataComponents[index];
        }

        public IEnumerator GetEnumerator()
        {
            return mDatas.GetEnumerator();
        }

        public T Value<T>(int index) where T : class
        {
            return mDatas[index] as T;
        }
        
    }
}