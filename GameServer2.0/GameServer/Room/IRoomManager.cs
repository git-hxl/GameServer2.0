
using System.Collections.Concurrent;

namespace GameServer
{
    public interface IRoomManager
    {
        ConcurrentDictionary<int, IRoom> Rooms { get; }

        T? CreateRoom<T>(int roomId) where T : IRoom, new();
        IRoom? GetRoom(int roomId);
        void CloseRoom(int roomId);
    }
}
