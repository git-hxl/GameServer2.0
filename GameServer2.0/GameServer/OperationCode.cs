

namespace GameServer
{
    public enum OperationCode : ushort
    {
        Login =0,
        Register,

        JoinRoom,
        OnJoinRoom,
        OnOtherJoinRoom,

        LeaveRoom,
        OnLeaveRoom,
        OnOtherLeaveRoom,

        SyncEvent,
    }
}
