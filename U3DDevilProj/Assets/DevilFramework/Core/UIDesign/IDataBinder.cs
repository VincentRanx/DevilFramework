using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil
{
    public interface IDataSource
    {
        int Index { get; set; }
        void SetData(object data);
        object GetData();
        void ApplyUpdate();
    }

    public interface IDataBinder<T> : IDataSource where T : class
    {
        T Data { get; set; }
        void OnClearData();
        void OnRefreshData(T data);
        void OnBindDataEvents(T data);
        void OnUnbindDataEvents(T data);
    }

    public interface List : IEnumerable
    {
        int DataCount { get; }
        void ClearData();
        void BindData<T>(IEnumerable<T> iter) where T : class;
        void AddData<T>(T data) where T : class;
        T Value<T>(int index) where T : class;
        T GetDataComponent<T>(int index) where T : Component;
    }
}