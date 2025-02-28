

namespace GameServer
{
    public enum SyncCode : ushort
    {
        SyncSpawnObject = 0,

        SyncRemoveObject,
        SyncTransform,
        SyncAnimation,
    }
}
