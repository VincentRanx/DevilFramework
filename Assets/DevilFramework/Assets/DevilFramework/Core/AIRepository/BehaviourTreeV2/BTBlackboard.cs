using Devil.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BTBlackboarProperty
    {
        public bool IsSet { get; set; }
        public string Name { get; private set; }
        public string TypeName { get; private set; }
        public object Value { get; set; }

        public BTBlackboarProperty(string name, string typeName)
        {
            this.Name = name;
            this.TypeName = typeName;
        }

        public BTBlackboardGetter<T> Getter<T>()
        {
            if (typeof(T).FullName != TypeName)
                return null;
            else
                return new BTBlackboardGetter<T>(this);
        }

        public BTBlackboardSetter<T> Setter<T>()
        {
            if (typeof(T).FullName != TypeName)
                return null;
            else
                return new BTBlackboardSetter<T>(this);
        }

    }

    public class BTBlackboardGetter<T>
    {
        BTBlackboarProperty mProperty;

        public BTBlackboardGetter(BTBlackboarProperty prop)
        {
            mProperty = prop;
        }

        public bool IsSet { get { return mProperty.IsSet; } }

        public T GetValue()
        {
            return (T)mProperty.Value;
        }
    }

    public class BTBlackboardSetter<T>
    {
        BTBlackboarProperty mProperty;

        public BTBlackboardSetter(BTBlackboarProperty prop)
        {
            mProperty = prop;
        }

        public bool IsSet { get { return mProperty.IsSet; } }

        public void SetValue(T valule)
        {
            mProperty.Value = valule;
            mProperty.IsSet = true;
        }

        public void UnsetValue()
        {
            mProperty.Value = null;
            mProperty.IsSet = false;
        }
    }

    public class BTBlackboard
    {

        static List<BTBlackboarProperty> mCache;
        static List<BTBlackboarProperty> Cache
        {
            get
            {
                if (mCache == null)
                    mCache = new List<BTBlackboarProperty>();
                return mCache;
            }
        }

        BTBlackboarProperty[] mVariables;

        public BTBlackboard(BlackboardAsset asset)
        {
            mVariables = new BTBlackboarProperty[asset.m_Properties.Length];
            for (int i = 0; i < mVariables.Length; i++)
            {
                mVariables[i] = new BTBlackboarProperty(asset.m_Properties[i].m_Key, asset.m_Properties[i].m_Value);
            }
        }

        public BTBlackboard()
        {
            mVariables = new BTBlackboarProperty[0];
        }

        public int GetPropertyId(string propertyName)
        {
            for (int i = 0; i < mVariables.Length; i++)
            {
                if (propertyName == mVariables[i].Name)
                    return i;
            }
            return -1;
        }

        public bool IsPropertySet(int propertyId)
        {
            if (propertyId < 0 || propertyId >= mVariables.Length)
                return false;
            else
                return mVariables[propertyId].IsSet;
        }

        public BTBlackboardGetter<T> Getter<T>(string propertyName)
        {
            for(int i = 0; i < mVariables.Length; i++)
            {
                if (propertyName == mVariables[i].Name)
                    return mVariables[i].Getter<T>();
            }
            return null;
        }

        public BTBlackboardSetter<T> Setter<T>(string propertyName)
        {
            for (int i = 0; i < mVariables.Length; i++)
            {
                if (propertyName == mVariables[i].Name)
                    return mVariables[i].Setter<T>();
            }
            return null;
        }
    }
}