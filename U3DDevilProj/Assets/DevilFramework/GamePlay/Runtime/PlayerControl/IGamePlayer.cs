using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public interface IGamePlayer
    {
        int PlayerId { get; }
        string PlayerName { get; }
        Vector3 PlayerPositoin { get; set; }
        Quaternion PlayerRotation { get; set; }
        Vector3 PlayerVelocity { get; }

        IMovement CurrentBaseMovement { get; }
        IMovement CurrentAdditiveMovement { get; }
        IMovement CurrentDecoratorMovement { get; }

        void AddInput(InputMask mask, bool isPress, bool isRelease);

        void AddMove(Vector3 moveDir);

    }
}