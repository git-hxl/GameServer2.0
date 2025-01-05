
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class CreateRoomRequest
    {
        public int PlayerID {  get; set; }

        public RoomInfo RoomInfo { get; set; }
    }

    [MessagePackObject(true)]
    public class CreateRoomResponse
    {
        public int PlayerID { get; set; }

        public RoomInfo RoomInfo { get; set; }
    }
}
