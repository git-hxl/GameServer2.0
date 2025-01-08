

namespace GameServer
{
    public enum OperationCode : ushort
    {
        Login = 0,
        Register,
        CreateRoom,
        JoinRoom,
        LeaveRoom,
        CloseRoom,

        UpdateRoomInfo,
        UpdatePlayerInfo,

        Disconnect,

        SyncEvent,
    }
}
