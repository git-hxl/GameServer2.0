

using MessagePack;
using System.Collections.Generic;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class JoinRoomRequest
    {
        public int RoomID { get; set; }
        public int PlayerID { get; set; }
        public PlayerInfo? PlayeInfo { get; set; }
    }

    [MessagePackObject(true)]
    public class JoinRoomResponse
    {
        public int RoomID { get; set; }
        public int PlayerID { get; set; }
        public RoomInfo? RoomInfo { get; set; }

        public List<PlayerInfo?>? PlayerInfos { get; set; }
    }
}