

using MessagePack;
using System.Collections.Generic;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class JoinRoomRequest
    {
        public int RoomID { get; set; }
        public int PlayerID { get; set; }
        public bool IsRobot { get; set; }
    }

    [MessagePackObject(true)]
    public class JoinRoomResponse
    {
        public int RoomID { get; set; }
        public int MasterID { get; set; }
        public int PlayerID { get; set; }
        public List<PlayerInfo> Others { get; set; }
    }
}