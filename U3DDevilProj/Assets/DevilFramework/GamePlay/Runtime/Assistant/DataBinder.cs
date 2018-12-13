namespace Devil.GamePlay.Assistant
{
    public sealed class DataBinder<T> : IDataBinder<T> where T : class
    {
        public System.Action<T> OnValidateData;
        public System.Action<T> OnBindData;
        public System.Action<T> OnUnbindData;

        IDataBinder<T> self;
        bool mActive;
        bool mRefresh;

        public int Index { get; set; }

        T mData;
        public T Data
        {
            get { return mData; }
            set
            {
                if (mData != value)
                {
                    mRefresh = false;
                    if (mData != null && mActive)
                        self.OnUnbindDataEvents(mData);
                    mData = value;
                    if (mData == null)
                    {
                        self.OnClearData();
                    }
                    else if (mActive)
                    {
                        self.OnBindDataEvents(mData);
                        self.OnRefreshData(mData);
                    }
                };
            }
        }

        public bool IsActive
        {
            get { return mActive; }
            set
            {
                if (mActive != value)
                {
                    mActive = value;
                    if (mData != null)
                    {
                        if (value)
                        {
                            self.OnBindDataEvents(mData);
                            self.OnRefreshData(mData);
                            mRefresh = false;
                        }
                        else
                        {
                            self.OnUnbindDataEvents(mData);
                        }
                    }
                }
            }
        }

        public DataBinder()
        {
            self = this;
        }

        public DataBinder(System.Action<T> validateCallback, System.Action<T> bindCallback, System.Action<T> unbindCallback)
        {
            self = this;
            OnValidateData = validateCallback;
            OnBindData = bindCallback;
            OnUnbindData = unbindCallback;
        }

        public void SetData(object data)
        {
            Data = data as T;
        }

        public object GetData()
        {
            return Data;
        }

        public void ApplyUpdate()
        {
            if (mData != null)
                mRefresh = true;
        }

        public void Update()
        {
            if (mRefresh)
            {
                mRefresh = false;
                if (mData == null)
                    self.OnClearData();
                else
                    self.OnRefreshData(mData);
            }
        }

        void IDataBinder<T>.OnRefreshData(T data) { if (OnValidateData != null) OnValidateData(data); }
        void IDataBinder<T>.OnBindDataEvents(T data) { if (OnBindData != null) OnBindData(data); }
        void IDataBinder<T>.OnUnbindDataEvents(T data) { if (OnUnbindData != null) OnUnbindData(data); }
        void IDataBinder<T>.OnClearData() { if (OnValidateData != null) OnValidateData(null); }
    }
}