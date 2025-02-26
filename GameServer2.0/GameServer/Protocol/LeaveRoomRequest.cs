

using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class LeaveRoomRequest : BaseRequest
    {

    }

    [MessagePackObject(true)]
    public class LeaveRoomResponse : BaseResponse
    {
        public int UserID { get; set; }
        public RoomInfo RoomInfo { get; set; }
    }
}
