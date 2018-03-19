using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay.Network
{
    public enum EQoS
    {
        not_important,
        immediate,
    }

    public class NetworkDefine
    {
        public const int KB_SIZE = 1 << 10;
        public const int MB_SIZE = 1 << 20;
        public const int GB_SIZE = 1 << 30;
    }
}