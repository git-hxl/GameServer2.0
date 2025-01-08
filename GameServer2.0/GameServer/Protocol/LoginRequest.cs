
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class LoginRequest
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }

    [MessagePackObject(true)]
    public class LoginResponse
    {
        public int ID { get; set; }
        public long ServerTime { get; set; }
    }
}