

namespace GameServer
{
    public enum OperationCode : ushort
    {
        Login = 0,
        Register,
        JoinRoom,
        LeaveRoom,
        CloseRoom,

        UpdateRoomInfo,
        UpdatePlayerInfo,

        SyncEvent,
    }
}
