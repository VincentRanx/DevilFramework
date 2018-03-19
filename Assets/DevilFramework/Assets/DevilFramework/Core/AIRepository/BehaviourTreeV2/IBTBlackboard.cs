using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public interface IBTBlackboard
    {
        BehaviourTreeRunner Runner { get; }

        int HashPropertyId(string propertyName);

        T GetProperty<T>(int propertyId);

        bool SetProperty<T>(int propertyId, T value);

        void UnsetProperty(int propertyId);

        bool IsPropertySetted(int propertyId);

    }
}