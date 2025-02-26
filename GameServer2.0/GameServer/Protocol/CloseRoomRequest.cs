
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class CloseRoomRequest : BaseRequest
    {
        public int RoomID { get; set; }
    }
}
