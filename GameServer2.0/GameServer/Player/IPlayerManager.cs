
using LiteNetLib;
using System.Collections.Concurrent;

namespace GameServer
{
    public interface IPlayerManager
    {
        ConcurrentDictionary<int, IPlayer> Players { get; }

        IPlayer? GetPlayer(int id);
        T? CreatePlayer<T>(int id, NetPeer netPeer) where T : IPlayer, new();
        void RemovePlayer(int id);
    }
}