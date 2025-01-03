
using System.Collections.Concurrent;

namespace GameServer
{
    public interface IPlayerManager
    {
        ConcurrentDictionary<int, IPlayer> Players { get; }

        IPlayer? GetPlayer(int id);
        IPlayer CreatePlayer(int id);

        void RemovePlayer(int id);
    }
}