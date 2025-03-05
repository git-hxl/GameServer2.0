

namespace GameServer
{
    public enum ReturnCode : ushort
    {
        Success = 0,

        CreateRoomFail,
        JoinRoomFail,

        LeaveRoomFail,

        UpdatePlayerInfoFail,
        UpdateRoomInfoFail,

        SyncEventFail,
    }
}