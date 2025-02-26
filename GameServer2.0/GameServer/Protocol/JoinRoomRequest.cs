

using MessagePack;
using System.Collections.Generic;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class JoinRoomRequest : BaseRequest
    {
        public int RoomID { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    [MessagePackObject(true)]
    public class JoinRoomResponse : BaseResponse
    {
        public int RoomID { get; set; }
        public int UserID { get; set; }
        public RoomInfo RoomInfo { get; set; }
        public List<UserInfo> Users { get; set; }
    }
}