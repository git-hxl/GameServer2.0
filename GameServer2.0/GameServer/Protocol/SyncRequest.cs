using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncRequest : BaseRequest
    {
        public SyncCode SyncCode { get; set; }
        public byte[] SyncData { get; set; }
    }
}