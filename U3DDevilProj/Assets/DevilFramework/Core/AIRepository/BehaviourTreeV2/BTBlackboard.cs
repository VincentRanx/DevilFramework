using Devil.Utility;
using System;
using System.Collections.Generic;

namespace Devil.AI
{
    public interface IBlackboardProperty
    {
        bool IsList { get; }
        System.Type TypeDefine { get; }
        void GatherData(ICollection<object> collection);
    }

    public interface IBlackboardValue<T>
    {
        bool IsSet { get; }
        T Value { get; }
        void Set(T value);
        void Unset();
    }

    public interface IBlackboardList<T> : IList<T> { }
    
    public class BTBlackboard
    {
        interface ICopy : IBlackboardProperty
        {
            bool IsEmpty { get; }
            ICopy CopyThis();
            void CopyValue(ICopy copyTo);
            void ClearAll();
        }

        class PValue<T> : IBlackboardValue<T>, ICopy
        {
            bool mIsSet;
            T mValue;
            public bool IsSet { get { return mIsSet; } }

            public T Value { get { return mValue; } }

            public bool IsEmpty { get { return !mIsSet; } }

            public bool IsList { get { return false; } }

            public Type TypeDefine { get { return typeof(T); } }
            
            public ICopy CopyThis()
            {
                var v = new PValue<T>();
                v.mValue = mValue;
                v.mIsSet = mIsSet;
                return v;
            }

            public void CopyValue(ICopy copyTo)
            {
                var p = copyTo as PValue<T>;
                if (p == null)
                    return;
                p.mValue = mValue;
                p.mIsSet = mIsSet;
            }

            public void GatherData(ICollection<object> collection)
            {
                if (mIsSet)
                    collection.Add(mValue);
            }

            public void Set(T value)
            {
                mValue = value;
                mIsSet = true;
            }

            public void Unset()
            {
                mValue = default(T);
                mIsSet = false;
            }

            public void ClearAll()
            {
                mValue = default(T);
                mIsSet = false;
            }
        }

        class PList<T> : List<T>, IBlackboardList<T>, ICopy
        {
            public bool IsEmpty { get { return Count == 0; } }
            
            public bool IsList { get { return true; } }

            public Type TypeDefine { get { return typeof(T); } }

            public ICopy CopyThis()
            {
                var p = new PList<T>();
                p.AddRange(this);
                return p;
            }

            public void CopyValue(ICopy copyTo)
            {
                if (Count == 0)
                    return;
                var p = copyTo as PList<T>;
                if (p != null)
                    p.AddRange(this);
            }

            public void GatherData(ICollection<object> collection)
            {
                if(Count > 0)
                {
                    foreach(var v in this)
                    {
                        collection.Add(v);
                    }
                }
            }

            public void ClearAll() { Clear(); }
        }

        BlackboardAsset.VariableDefine[] mPropertyDefines;
        ICopy[] mProperties;

        public BTBlackboard(BlackboardAsset asset)
        {
            if (asset == null)
            {
                mPropertyDefines = new BlackboardAsset.VariableDefine[0];
                mProperties = new ICopy[0];
            }
            else
            {
                mPropertyDefines = new BlackboardAsset.VariableDefine[asset.Length];
                for (int i = 0; i < mPropertyDefines.Length; i++)
                {
                    mPropertyDefines[i] = asset[i];
                }
                mProperties = new ICopy[mPropertyDefines.Length];
            }
        }


        public int Length { get { return mProperties.Length; } }
        public string GetPropertyName(int index) { return mPropertyDefines[index].name; }
        public IBlackboardProperty this[int index] { get { return mProperties[index]; } }
        
        public bool IsSet(int index)
        {
            return index >= 0 && index < mProperties.Length && mProperties[index] != null && !mProperties[index].IsEmpty;
        }

        public int GetIndex(string propertyName)
        {
            return string.IsNullOrEmpty(propertyName) ? -1 : GlobalUtil.FindIndex(mPropertyDefines, (x) => x.name == propertyName);
        }

        bool IsValid<T>(int index, bool isList)
        {
            if (isList ^ mPropertyDefines[index].isList || typeof(T).FullName != mPropertyDefines[index].typeDef)
                return false;
            return true;
        }

        public IBlackboardValue<T> Value<T>(int index)
        {
            if (!IsValid<T>(index, false))
                return null;
            var p = mProperties[index];
            if(p == null)
            {
                p = new PValue<T>();
                mProperties[index] = p;
            }
            return (IBlackboardValue<T>)p;
        }

        public IBlackboardValue<T> Value<T>(string propertyName)
        {
            var index = GetIndex(propertyName);
            return index == -1 ? null : Value<T>(index);
        }
        
        public IBlackboardList<T> List<T>(string propertyName)
        {
            var index = GetIndex(propertyName);
            return index == -1 ? null : List<T>(index);
        }

        public IBlackboardList<T> List<T>(int index)
        {
            if (!IsValid<T>(index, true))
                return null;
            var p = mProperties[index];
            if(p == null)
            {
                p = new PList<T>();
                mProperties[index] = p;
            }
            return (IBlackboardList<T>)p;
        }

        public void ClearAt(int index)
        {
            var p = mProperties[index];
            if (p != null)
                p.ClearAll();
        }
    }
}