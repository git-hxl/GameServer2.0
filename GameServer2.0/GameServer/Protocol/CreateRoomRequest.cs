
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class CreateRoomRequest : BaseRequest
    {
        public RoomInfo RoomInfo { get; set; }
    }

    [MessagePackObject(true)]
    public class CreateRoomResponse : BaseResponse
    {
        public RoomInfo RoomInfo { get; set; }
    }
}
