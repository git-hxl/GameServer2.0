
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject]
    public class Request
    {
        [Key(0)]
        public OperationCode OperationCode;
        [Key(1)]
        public long Timestamp;
        [Key(2)]
        public byte[] Data;
    }

    [MessagePackObject]
    public class Response
    {
        [Key(0)]
        public OperationCode OperationCode;
        [Key(1)]
        public ReturnCode ReturnCode;
        [Key(2)]
        public string ErrorMsg;
        [Key(3)]
        public long Timestamp;
        [Key(4)]
        public byte[] Data;
    }
}
