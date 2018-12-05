namespace Devil.GamePlay
{
    public enum EQoS
    {
        not_important,
        immediate,
    }

    public enum EPacketType
    {
        Unknown,
        Protobuf,
        Json,
    }

    static public class NetworkDefine
    {
        public const int KB_SIZE = 1 << 10;
        public const int MB_SIZE = 1 << 20;
        public const int GB_SIZE = 1 << 30;

        public const int MAX_PACKET_SIZE = MB_SIZE;

        public static readonly EPacketType PacketType = EPacketType.Protobuf;
        public static readonly bool HasPacketHeader = false;
    }
}