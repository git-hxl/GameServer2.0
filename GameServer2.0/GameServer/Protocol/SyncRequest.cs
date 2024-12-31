using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncRequest
    {
        public int PlayerID { get; set; }
        public ushort SyncCode { get; set; }
        public byte[] SyncData { get; set; }
        public long Timestamp {  get; set; }
    }
}