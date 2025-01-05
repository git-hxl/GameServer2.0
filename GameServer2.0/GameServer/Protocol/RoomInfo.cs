

using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class RoomInfo
    {
        public int RoomID {  get; set; }
        public string RoomName { get; set; }
        public int MasterID {  get; set; }
    }
}
