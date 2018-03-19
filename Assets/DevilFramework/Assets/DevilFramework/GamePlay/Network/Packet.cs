using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Devil.GamePlay.Network
{
    public struct PacketHeader
    {
        public ushort length;
        public short actionId;
    }

    public class PacketStream
    {
        private byte[] mBuffer;

        public PacketStream(int bufferSize = 2 * NetworkDefine.KB_SIZE)
        {
            mBuffer = new byte[bufferSize];
        }


    }
}