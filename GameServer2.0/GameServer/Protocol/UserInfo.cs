

using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class UserInfo
    {
        public string UserName { get; set; }
        public bool IsRobot { get; set; }
    }
}