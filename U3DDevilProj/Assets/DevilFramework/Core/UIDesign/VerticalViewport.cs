using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devil.UI
{
    using LayoutInfo = LayoutData.LayoutInfo;
    public delegate int DataComparesion(object a, object b);

    public class VerticalViewport : UIBehaviour, List, ILayoutSelfController
    {
        public enum Align
        {
            UpperLeft,
            UpperCenter,
            UpperRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            LowerLeft,
            LowerCenter,
            LowerRight,
        }
        
        class Element : IIdentified
        {
            static int _idcount;
            public int Identify { get; private set; }
            int prototype;
            public object data;
            public LayoutData dataEntity;
            public Vector2 size;
            public Vector2 position;
            public Element next;
            public Element previous;
            public Row row;
            bool mVisible;

            public LayoutInfo info
            {
                get
                {
                    LayoutInfo lin;
                    lin.id = Identify;
                    lin.data = data;
                    return lin;
                }
            }
            
            public Element(int prototype)
            {
                Identify = ++_idcount;
                this.prototype = prototype;
            }

            public void AddAfter(Element data)
            {
                var n = next;
                next = data;
                data.previous = this;
                if(n != null)
                {
                    data.next = n;
                    n.previous = data;
                }
            }

            public void AddBefore(Element data)
            {
                var n = previous;
                previous = data;
                data.next = this;
                if(n != null)
                {
                    n.next = data;
                    data.previous = n;
                }
            }

            public void Remove()
            {
                var prev = previous;
                previous = null;
                var nex = next;
                next = null;
                if(prev != null)
                    prev.next = nex;
                if (nex != null)
                    nex.previous = prev;
            }

            public void SetVisible(VerticalViewport viewport, bool visible, bool forceValidate)
            {
                if (visible == mVisible && !forceValidate)
                    return;
                mVisible = visible;
                if (visible)
                {
                    if (dataEntity == null)
                    {
                        dataEntity = viewport.mObjectBuffer.GetAnyTarget(prototype);
                        dataEntity.OnBindData(info);
                    }
                    if (!dataEntity.gameObject.activeSelf)
                        dataEntity.gameObject.SetActive(true);
                    var trans = dataEntity.rectTransform;
                    Rect localrect = new Rect(position.x, position.y - size.y, size.x, size.y);
                    Vector3 pos = Vector3.zero;
                    pos.x = Mathf.Lerp(localrect.xMin, localrect.xMax, trans.pivot.x);
                    pos.y = Mathf.Lerp(localrect.yMin, localrect.yMax, trans.pivot.y);
                    dataEntity.SetLocalPosition(pos);
                }
                else if (dataEntity != null)
                {
                    dataEntity.OnUnbindData();
                    viewport.mObjectBuffer.SaveBuffer(prototype, dataEntity);
                    if (viewport.m_MoveCachedItemOutOfBounds && viewport.m_ClipRect != null)
                    {
                        var rect = _2DUtil.CalculateRelativeRect(viewport.transform, viewport.m_ClipRect);
                        var p = dataEntity.transform.localPosition;
                        float l = Mathf.Abs(p.x - rect.xMin);
                        float r = Mathf.Abs(p.x - rect.xMax);

                        if (l < r)
                            p.x = rect.xMin - rect.width;
                        else
                            p.x = rect.xMax + rect.width;
                        dataEntity.transform.localPosition = p;

                    }
                    else if (dataEntity.gameObject.activeSelf)
                    {
                        dataEntity.gameObject.SetActive(false);
                    }
                    dataEntity = null;
                }
            }
            
            public class Enumerator: IEnumerator
            {
                Element mFirst;
                Element mCurrent;
                bool mBegin;
                public Enumerator(Element first)
                {
                    mFirst = first;
                }

                public object Current { get { return mCurrent == null ? null : mCurrent.data; } }

                public bool MoveNext()
                {
                    if(!mBegin)
                    {
                        mBegin = true;
                        mCurrent = mFirst;
                    }
                    else if(mCurrent != null)
                    {
                        mCurrent = mCurrent.next;
                    }
                    return mCurrent != null;
                }

                public void Reset()
                {
                    mBegin = false;
                }
            }
        }

        class Row
        {
            public int index;
            public float y;
            public float fixedWidth; // 固定宽度
            public float height;
            public float width;
            public Element firstData;
            public Element lastData;
            public Row next;
            public Row previous;
            
            public Row(float fixedwidth)
            {
                fixedWidth = fixedwidth;
                index = 0;
            }

            public Row AppendLine()
            {
                Row row = new Row(fixedWidth);
                row.previous = this;
                row.index = index + 1;
                next = row;
                return row;
            }
            
            public bool AppendNode(Element node, float space)
            {
                if (firstData == null)
                {
                    firstData = node;
                    lastData = node;
                    width = node.size.x + space * 2;
                    height = node.size.y;
                    node.row = this;
                    return true;
                }
                else if (width + node.size.x > fixedWidth)
                {
                    return false;
                }
                else
                {
                    width += space + node.size.x;
                    height = Mathf.Max(height, node.size.y);
                    lastData = node;
                    node.row = this;
                    return true;
                }
            }

            public void Reposition(float hoffset, float voffset, Vector2 space)
            {
                float xoffst = space.x + (fixedWidth - width) * hoffset;
                float yoffset;
                y = previous == null ? -space.y : previous.y - previous.height - space.y;
                var node = firstData;
                while(node != null)
                {
                    yoffset = (height - node.size.y) * voffset;
                    node.position = new Vector2(xoffst, y - yoffset);
                    xoffst += space.x + node.size.x;
                    if (node == lastData)
                        break;
                    node = node.next;
                }
            }

            public void SetVisible(VerticalViewport viewport, bool visible, bool forceValidate)
            {
                var n = firstData;
                while (n != null)
                {
                    n.SetVisible(viewport, visible, forceValidate);
                    if (n == lastData)
                        break;
                    n = n.next;
                }
            }
        }

        public RectTransform m_ClipRect;

        [SerializeField]
        Align m_Alignment;
        [SerializeField]
        bool m_SortData;
        // 试图区域扩充范围
        public Vector2 m_ViewportExpend;
        public Vector2 m_Space = new Vector2(5, 5);
        public int m_InitCacheSize = 32;
        public bool m_FillEmptyData; // 使用空数据填充

        [HideInInspector]
        [SerializeField]
        RectTransform mRectTrans;

        // 将缓存的对象移出视图区域
        [SerializeField]
        bool m_MoveCachedItemOutOfBounds;

        public RectTransform SelfRect
        {
            get
            {
                if (mRectTrans == null)
                    mRectTrans = GetComponent<RectTransform>();
                return mRectTrans;
            }
        }

        float mFixedWidth; // 宽度
        LayoutData[] mPrototypes; // 预设原型
        ObjectBuffers<LayoutData> mObjectBuffer;

        Element mFirst;
        Element mLast;
        Row mFirstRow;
        Row mLastRow;

        float mHOffset;
        float mVOffset;
        private DrivenRectTransformTracker m_Tracker;
        public DataComparesion Sorter { get; set; }

        public int DataCount
        {
            get
            {
                int num = 0;
                var node = mFirst;
                while(node != null)
                {
                    num++;
                    node = node.next;
                }
                return num;
            }
        }

        bool mResort;
        bool mResize;
        Row mResizeRow; // 重新计算大小的行

        Vector2 SizeOfPrototype(int prototype)
        {
            var p = mPrototypes[prototype];
            return p.rectTransform.rect.size;
        }

        void InitPrototypes()
        {
            if (mPrototypes == null || mObjectBuffer == null)
            {
                Transform trans = transform;
                mPrototypes = new LayoutData[trans.childCount];
                mObjectBuffer = new ObjectBuffers<LayoutData>(mPrototypes.Length, m_InitCacheSize, InstantiateData, DestroyData);
                Vector2 anchor = new Vector2(0, 1);
                for (int i = 0; i < mPrototypes.Length; i++)
                {
                    GameObject go = trans.GetChild(i).gameObject;
                    LayoutData data = go.GetComponent<LayoutData>();
                    if (data == null)
                        data = go.AddComponent<LayoutData>();
                    mPrototypes[i] = data;
                    data.rectTransform.anchorMin = anchor;
                    data.rectTransform.anchorMax = anchor;
                    if (go.activeSelf)
                        go.SetActive(false);
                    mObjectBuffer.SaveBuffer(i, data);
                }
            }
        }

        void InitAxisData()
        {
            RectTransform trans = SelfRect;
            trans.pivot = new Vector2(0, 1);
            trans.anchorMin = new Vector2(0, 1);
            trans.anchorMax = new Vector2(0, 1);
            Rect rect = m_ClipRect == null ? trans.rect : m_ClipRect.rect;
            mFixedWidth = rect.width;
            switch (m_Alignment)
            {
                case Align.LowerLeft:
                    mHOffset = 0;
                    mVOffset = 1;
                    break;
                case Align.MiddleLeft:
                    mHOffset = 0;
                    mVOffset = 0.5f;
                    break;
                case Align.UpperLeft:
                    mHOffset = 0;
                    mVOffset = 0;
                    break;
                case Align.LowerCenter:
                    mHOffset = 0.5f;
                    mVOffset = 1;
                    break;
                case Align.MiddleCenter:
                    mHOffset = 0.5f;
                    mVOffset = 0.5f;
                    break;
                case Align.UpperCenter:
                    mHOffset = 0.5f;
                    mVOffset = 0;
                    break;
                case Align.LowerRight:
                    mHOffset = 1f;
                    mVOffset = 1;
                    break;
                case Align.MiddleRight:
                    mHOffset = 1f;
                    mVOffset = 0.5f;
                    break;
                case Align.UpperRight:
                    mHOffset = 1f;
                    mVOffset = 0;
                    break;
                default: break;
            }
        }

        int GetPrototype<T>() where T : class
        {
            for (int i = 0; i < mPrototypes.Length; i++)
            {
                if (mPrototypes[i] is IDataBinder<T>)
                    return i;
            }
            return -1;
        }

        // 实例化对象
        LayoutData InstantiateData(int prototype)
        {
            LayoutData proto = mPrototypes[prototype];
            if (proto == null)
                return null;
            GameObject obj = Instantiate(proto.gameObject, transform, false);
            return obj.GetComponent<LayoutData>();
        }

        bool DestroyData(int prototype, LayoutData data)
        {
            if (data != null && data != mPrototypes[prototype])
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(data.gameObject);
                else
#endif
                    Destroy(data.gameObject);
                return true;
            }
            else
            {
                return false;
            }
        }

        int CompareData(Element a, Element b)
        {
            if (Sorter == null)
                return a.Identify <= b.Identify ? -1 : 1;
            int comp = Sorter(a.data, b.data);
            if (comp == 0)
                return a.Identify <= b.Identify ? -1 : 1;
            else
                return comp;
        }

        [ContextMenu("Clear Data")]
        public void ClearData()
        {
            mResize = false;
            mResizeRow = null;
            mFirstRow = null;
            mLastRow = null;
            var node = mFirst;
            while(node != null)
            {
                node.SetVisible(this, false, true);
                node = node.next;
            }
            mFirst = null;
            mLast = null;
            if (!Application.isPlaying && mPrototypes != null)
            {
                for(int i = 0; i < mPrototypes.Length; i++)
                {
                    mPrototypes[i].gameObject.SetActive(true);
                }
            }
            if (!Application.isPlaying && mObjectBuffer != null)
                mObjectBuffer.Clear();
            SetDirty();
            //SelfRect.offsetMin = new Vector2(0, -m_Space.y * 2f);
            //SelfRect.offsetMax = new Vector2(mFixedWidth, 0);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(mObjectBuffer != null)
                mObjectBuffer.Clear();
        }

        [ContextMenu("Resposition")]
        public void Reposition()
        {
            InitPrototypes();
            InitAxisData();
            mResize = false;
            mFirstRow = new Row(mFixedWidth);
            mLastRow = mFirstRow;
            Resize(mFirstRow);
        }

        // 重新计算大小和位置
        void Resize(Row startRow)
        {
            var node = startRow.firstData;
            if(node == null)
            {
                var row = startRow.previous;
                if (row == null)
                    node = mFirst;
                else if (row.lastData == null)
                    node = null;
                else
                    node = row.lastData.next;
            }
            startRow.firstData = null;
            startRow.lastData = null;
            while(node != null)
            {
                if (!startRow.AppendNode(node, m_Space.x))
                {
                    startRow.Reposition(mHOffset, mVOffset, m_Space);
                    startRow.SetVisible(this, true, true);
                    startRow = startRow.AppendLine();
                }
                else
                {
                    node = node.next;
                }
            }
            startRow.Reposition(mHOffset, mVOffset, m_Space);
            startRow.SetVisible(this, true, true);
            mLastRow = startRow;
            SetDirty();
        }
        
        public IEnumerator GetEnumerator()
        {
            return new Element.Enumerator(mFirst);
        }

        private void Update()
        {
            if (mResort)
            {
                ResortImmediate();
            }
            if (mResize)
            {
                mResize = false;
                if (mResizeRow == null)
                {
                    mFirstRow = new Row(mFixedWidth);
                    mLastRow = mFirstRow;
                    mResizeRow = mFirstRow;
                }
                Resize(mResizeRow);
                mResizeRow = null;
            }
        }

        protected override void Start()
        {
            base.Start();
            InitAxisData();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(SelfRect);
            base.OnDisable();
        }
        
        public virtual void SetLayoutHorizontal()
        {
            mFixedWidth = m_ClipRect == null ? 1 : m_ClipRect.rect.width;
            m_Tracker.Clear();
            m_Tracker.Add(this, SelfRect, DrivenTransformProperties.SizeDeltaX);
            SelfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mFixedWidth);
        }

        public virtual void SetLayoutVertical()
        {
            m_Tracker.Add(this, SelfRect, DrivenTransformProperties.SizeDeltaY);
            float h = mLastRow == null ? -m_Space.y : mLastRow.y - mLastRow.height - m_Space.y;
            if(m_ClipRect != null)
                h = Mathf.Min(h, -m_ClipRect.rect.height);

            SelfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, -h);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(SelfRect);
        }

        public void AddData(int prototype, object data)
        {
            InitPrototypes();
            Element ele = new Element(prototype);
            ele.size = SizeOfPrototype(prototype);
            ele.data = data;
            if (mFirst == null)
            {
                mFirst = ele;
                mResize = true;
                mResizeRow = mFirstRow;
                mLast = ele;
                return;
            }
            if (m_SortData)
            {
                var node = mLast;
                while (node != null)
                {
                    if (CompareData(node, ele) < 0)
                    {
                        break;
                    }
                    node = node.previous;
                }
                if (node == null)
                {
                    mFirst.AddBefore(ele);
                    mResize = true;
                    mResizeRow = null;
                    mFirst = ele;
                }
                else
                {
                    node.AddAfter(ele);
                    if (node == mLast)
                    {
                        mLast = ele;
                    }
                    mResize = true;
                    if (node.row == null)
                        mResizeRow = null;
                    else if (mResizeRow == null || mResizeRow.index > node.row.index)
                        mResizeRow = node.row;
                }
            }
            else
            {
                mLast.AddAfter(ele);
                mLast = ele;
                mResize = true;
                mResizeRow = mLastRow;
            }
        }

        public object RemoveData(int id)
        {
            var node = mFirst;
            while(node != null)
            {
                if(node.Identify == id)
                {
                    var p = node.previous;
                    node.Remove();
                    var r0 = mResize ? mResizeRow : mLastRow;
                    int i0 = r0 == null ? 0 : r0.index;
                    mResize = true;
                    var r1 = p == null ? null : p.row;
                    int i1 = r1 == null ? 0 : r1.index;
                    if (i1 <= i0)
                        mResizeRow = r1;
                    return node.data;
                }
            }
            return null;
        }

        public T RemoveData<T>(FilterDelegate<T> filter) where T : class
        {
            var node = mFirst;
            while (node != null)
            {
                T v = node.data as T;
                if (filter(v))
                {
                    var p = node.previous;
                    node.Remove();
                    var r0 = mResize ? mResizeRow : mLastRow;
                    int i0 = r0 == null ? 0 : r0.index;
                    mResize = true;
                    var r1 = p == null ? null : p.row;
                    int i1 = r1 == null ? 0 : r1.index;
                    if (i1 <= i0)
                        mResizeRow = r1;
                    return v;
                }
            }
            return null;
        }


        public void BindData<T>(IEnumerable<T> iter) where T : class
        {
            InitPrototypes();
            int p = GetPrototype<T>();
            if (p == -1)
                return;
            Vector2 size = SizeOfPrototype(p);
            ClearData();
            var it = iter.GetEnumerator();
            //Element node = null;
            if (m_SortData)
            {
                while (it.MoveNext())
                {
                    var ele = new Element(p);
                    ele.data = it.Current;
                    ele.size = size;
                    if (mLast == null)
                    {
                        mFirst = ele;
                        mLast = ele;
                    }
                    else
                    {
                        var prev = mLast;
                        while (prev != null)
                        {
                            if (CompareData(prev, ele) <= 0)
                                break;
                            prev = prev.previous;
                        }
                        if (prev == null)
                        {
                            ele.AddAfter(mFirst);
                            mFirst = ele;
                        }
                        else
                        {
                            prev.AddAfter(ele);
                            if(prev == mLast)
                                mLast = ele;
                        }
                    }
                }
            }
            else
            {
                while (it.MoveNext())
                {
                    Element ele = new Element(p);
                    ele.data = it.Current;
                    ele.size = size;
                    if (mFirst == null)
                    {
                        mFirst = ele;
                        mLast = ele;
                    }
                    else
                    {
                        mLast.AddAfter(ele);
                        mLast = ele;
                    }
                }
            }
            mResize = true;
            mResizeRow = null;
        }
        
        public void AddData<T>(T data) where T : class
        {
            InitPrototypes();
            int p = GetPrototype<T>();
            if (p == -1)
                return;
            AddData(p, data);
        }

        public T Value<T>(int index) where T : class
        {
            int num = 0;
            var node = mFirst;
            while(num < index && node != null)
            {
                num++;
                node = node.next;
            }
            return node == null ? null : node.data as T;
        }

        public bool ModifyData(int dataid, object data)
        {
            var node = mFirst;
            while(node != null)
            {
                if(node.Identify == dataid)
                {
                    if (node.data != data)
                    {
                        node.data = data;
                        if (node.dataEntity != null)
                        {
                            node.dataEntity.OnUnbindData();
                            node.dataEntity.OnBindData(node.info);
                        }
                    }
                    return true;
                }
                node = node.next;
            }
            return false;
        }

        public bool ModifyData(FilterDelegate<object> datafilter, object data)
        {
            var node = mFirst;
            while (node != null)
            {
                if (datafilter(node.data))
                {
                    if (node.data != data)
                    {
                        node.data = data;
                        if (node.dataEntity != null)
                        {
                            node.dataEntity.OnUnbindData();
                            node.dataEntity.OnBindData(node.info);
                        }
                    }
                    return true;
                }
                node = node.next;
            }
            return false;
        }

        public void Resort()
        {
            mResort = true;
        }

        // 重新排序
        public void ResortImmediate()
        {
            mResort = false;
            if (!m_SortData)
                return;
            var first = mFirst;
            if (first == null)
                return;
            var tmp = first.next;
            bool resize = false;
            while(tmp != null)
            {
                var next = tmp.next;
                var prev = tmp.previous;
                while(prev != null)
                {
                    if (CompareData(prev, tmp) <= 0)
                        break;
                    prev = prev.previous;
                }
                if(prev == null)
                {
                    tmp.Remove();
                    mFirst.AddBefore(tmp);
                    mFirst = tmp;
                    resize = true;
                }
                else if(prev.next != tmp)
                {
                    tmp.Remove();
                    prev.AddAfter(tmp);
                    resize = true;
                }
                tmp = next;
            }
            while (mLast.next != null)
                mLast = mLast.next;
            if (resize)
            {
                // 重置位置
                mResize = true;
                mResizeRow = null;
            }
        }

        public int FindDataID<T>(FilterDelegate<object> dataFilter)
        {
            var node = mFirst;
            while (node != null)
            {
                if (dataFilter(node.data))
                {
                    return node.Identify;
                }
                node = node.next;
            }
            return 0;
        }

        public T GetDataComponent<T>(int index) where T : Component
        {
            int num = 0;
            var node = mFirst;
            while (num < index && node != null)
            {
                num++;
                node = node.next;
            }
            return node == null ? null : node.dataEntity as T;
        }

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
        
        public int num = 30;
        [ContextMenu("Test")]
        void Test()
        {
            InitPrototypes();
            InitAxisData();
            if (mPrototypes.Length == 0)
                return;
            ClearData();
            mFirst = null;
            Element current = null;
            for (int i = 0; i < num; i++)
            {
                int proto = (i % 23) % mPrototypes.Length;
                current = new Element(proto);
                current.size = SizeOfPrototype(proto);
                if (mFirst == null)
                {
                    mFirst = current;
                    mLast = mFirst;
                }
                else
                {
                    mLast.AddAfter(current);
                    mLast = current;
                }
            }
            mFirstRow = new Row(mFixedWidth);
            Resize(mFirstRow);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (m_ClipRect != null)
            {
                Rect rect = _2DUtil.CalculateRelativeRect(SelfRect, m_ClipRect);
                Gizmos.color = new Color(0.7f, 0.7f, 0.4f, 0.5f);
                Gizmos.DrawCube(rect.center, rect.size);
            }
            Gizmos.color = Color.green;
            var node = mFirst;
            while (node != null)
            {
                Rect rect = new Rect(Vector2.zero, node.size);
                rect.position = new Vector2(node.position.x, node.position.y - node.size.y);
                Gizmos.DrawWireCube(rect.center, rect.size);
                node = node.next;
            }
        }

#endif
        
    }
}