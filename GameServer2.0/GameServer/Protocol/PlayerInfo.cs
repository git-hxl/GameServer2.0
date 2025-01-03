

using MessagePack;
using System.Collections.Generic;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class PlayerInfo
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
    }
}