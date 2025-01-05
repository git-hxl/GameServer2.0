

using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class PlayerInfo
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public bool IsRobot { get; set; }
    }
}