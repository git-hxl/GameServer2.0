

namespace GameServer
{
    public enum OperationCode : ushort
    {
        Login = 0,
        Register,

        JoinRoom,
        OnJoinRoom,
        OnOtherJoinRoom,

        LeaveRoom,
        OnLeaveRoom,
        OnOtherLeaveRoom,

        OnRoomClose,

        SyncEvent,
    }

    public enum EventCode : ushort
    {
        SyncTransform,
    }
}
