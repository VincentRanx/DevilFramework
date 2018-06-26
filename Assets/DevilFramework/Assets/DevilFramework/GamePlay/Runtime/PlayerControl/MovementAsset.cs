using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Devil.GamePlay
{

    public abstract class MovementAsset : ScriptableObject
    {
        public abstract IMovement CreateMovement(IGamePlayer player);
    }
}