
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class LoginRequest : BaseRequest
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }

    [MessagePackObject(true)]
    public class LoginResponse : BaseResponse
    {
    }
}