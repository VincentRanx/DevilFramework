using Devil.UI;

namespace Devil.GamePlay.Assistant
{
    public class DataViewBase<T> : LayoutData, IDataBinder<T> where T : class
    {
        bool mActive;
        bool mRefresh;
        public int Index { get; set; }
        protected T mData;
        public T Data
        {
            get { return mData; }
            set
            {
                if (mData != value)
                {
                    mRefresh = false;
                    if (mData != null && mActive)
                        OnUnbindDataEvents(mData);
                    mData = value;
                    if (mData == null)
                    {
                        OnClearData();
                    }
                    else if (mActive)
                    {
                        OnRefreshData(mData);
                        OnBindDataEvents(mData);
                    }
                };
            }
        }
        public int LayoutID { get; private set; }
        public virtual void SetData(object data)
        {
            Data = data as T;
        }
        public object GetData()
        {
            return Data;
        }

        public override void OnBindData(LayoutInfo info)
        {
            base.OnBindData(info);
            LayoutID = info.id;
            Data = info.data as T;
        }

        public override void OnUnbindData()
        {
            base.OnUnbindData();
            LayoutID = 0;
            Data = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mActive = true;
            mRefresh = false;
            if (mData != null)
            {
                OnRefreshData(mData);
                OnBindDataEvents(mData);
            }
        }

        protected override void OnDisable()
        {
            base.OnEnable();
            mActive = false;
            if (mData != null)
            {
                OnUnbindDataEvents(mData);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mData = null;
            mRefresh = false;
        }

        public void ApplyUpdate()
        {
            if (mData != null)
                mRefresh = true;
        }

        protected override void Update()
        {
            base.Update();
            if (mRefresh)
            {
                mRefresh = false;
                if (mData == null)
                    OnClearData();
                else
                    OnRefreshData(mData);
            }
        }

        public virtual void OnRefreshData(T data) { }
        public virtual void OnClearData() { }
        public virtual void OnBindDataEvents(T data) { }
        public virtual void OnUnbindDataEvents(T data) { }

    }
}