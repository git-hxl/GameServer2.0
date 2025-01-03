
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class PlayerManager : IPlayerManager
    {
        public ConcurrentDictionary<int, IPlayer> Players { get; private set; } = new ConcurrentDictionary<int, IPlayer>();

        public IPlayer CreatePlayer(int id)
        {
            IPlayer? player;
            if (Players.TryGetValue(id, out player))
            {
                return player;
            }

            player = ReferencePool.Instance.Acquire<Player>();

            if (Players.TryAdd(id, player))
            {
                player.Init(id);
            }

            return player;
        }

        public IPlayer? GetPlayer(int id)
        {
            IPlayer? player;
            if (Players.TryGetValue(id, out player))
            {
                return player;
            }
            return null;
        }

        public void RemovePlayer(int id)
        {
            IPlayer? player;
            if (Players.TryRemove(id, out player))
            {
                ReferencePool.Instance.Release(player);
            }
        }
    }
}