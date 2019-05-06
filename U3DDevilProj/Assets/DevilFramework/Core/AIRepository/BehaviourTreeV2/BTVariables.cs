using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class Variable<T> : IBlackboardData
    {
        public string varName;
        public T varValue;

        public virtual void UpdateBlackboard(BTBlackboard blackboard)
        {
            var btv = blackboard.Value<T>(varName);
            if (btv != null)
                btv.Set(varValue);
        }
    }

    public class VariableList<T> : IBlackboardData
    {
        public string name;
        public List<T> value = new List<T>();

        public virtual void UpdateBlackboard(BTBlackboard blackboard)
        {
            var btv = blackboard.List<T>(name);
            if (btv != null)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    btv.Add(value[i]);
                }
            }
        }
    }


    [System.Serializable]
    public class BTFloatVariable : Variable<float>
    { }

    [System.Serializable]
    public class BTFloatList : VariableList<float>
    { }

    [System.Serializable]
    public class BTIntVariable : Variable<int>
    { }

    [System.Serializable]
    public class BTIntList : VariableList<int>
    { }

    [System.Serializable]
    public class BTVector3Variable : Variable<Vector3>
    { }

    [System.Serializable]
    public class BTVector3List : VariableList<Vector3>
    { }

    [System.Serializable]
    public class BTStringValue: Variable<string>
    { }
}