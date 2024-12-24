

using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class JoinRoomRequest
    {
        public int RoomID { get; set; }
        public int PlayerID { get; set; }
    }

    [MessagePackObject(true)]
    public class JoinRoomResponse
    {
        public int RoomID { get; set; }
        public int PlayerID { get; set; }
    }
}