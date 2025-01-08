
using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class CloseRoomRequest
    {
        public int RoomID {  get; set; }
    }
}
