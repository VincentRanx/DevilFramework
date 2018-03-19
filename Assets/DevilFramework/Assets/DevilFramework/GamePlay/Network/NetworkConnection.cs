using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay.Network
{
    public abstract class NetworkConnection
    {
        public abstract void SendData(byte[] buffer, int index, int len);

        public abstract int ReceiveData(byte[] buffer, int index);
    }
}