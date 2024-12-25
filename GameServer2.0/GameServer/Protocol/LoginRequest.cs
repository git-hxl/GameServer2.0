
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class LoginRequest
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }
}