

using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class LeaveRoomRequest
    {
        public int PlayerID {  get; set; }
    }

    [MessagePackObject(true)]
    public class LeaveRoomResponse
    {
        public int PlayerID { get; set; }
        public RoomInfo RoomInfo { get; set; }
    }
}
