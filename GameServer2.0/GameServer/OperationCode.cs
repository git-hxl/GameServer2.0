

namespace GameServer
{
    public enum OperationCode : ushort
    {
        JoinRoom,
        OnJoinRoom,

        LeaveRoom,
        OnLeaveRoom,

        OnMasterChange,
    }
}
