
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class BaseRequest
    {
        public int UserID { get; set; }
        public long Timestamp { get; set; }
    }

    [MessagePackObject(true)]
    public class BaseResponse
    {
        public string ErrorMsg;
        public long Timestamp;
    }
}