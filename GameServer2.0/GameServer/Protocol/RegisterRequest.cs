

namespace GameServer.Protocol
{
    public class RegisterRequest : BaseRequest
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class RegisterResponse : BaseResponse
    {

    }
}
