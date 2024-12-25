using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncEventData
    {
        public int PlayerID { get; set; }
        public int EventID { get; set; }
        public byte[] SyncData { get; set; }
    }
}