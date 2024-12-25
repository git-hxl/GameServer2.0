

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
        public int MasterID { get; set; }
        public List<PlayerInfoInRoom> Others { get; set; }
    }

    [MessagePackObject(true)]
    public class PlayerInfoInRoom
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
    }
}