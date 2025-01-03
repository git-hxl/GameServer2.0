
using System.Collections.Concurrent;

namespace GameServer
{
    public interface IRoomManager
    {
        ConcurrentDictionary<int, IRoom> Rooms { get; }

        IRoom CreateRoom(int roomId); 
        IRoom GetRoom(int roomId);
        void CloseRoom(int roomId);
    }
}
