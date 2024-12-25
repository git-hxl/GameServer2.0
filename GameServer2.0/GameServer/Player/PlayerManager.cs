
using LiteNetLib;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        public ConcurrentDictionary<int, Player> Players { get; } = new ConcurrentDictionary<int, Player>();

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
            foreach (var item in Players)
            {
                ReferencePool.Instance.Release(item.Value);
            }

            Players.Clear();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }

        public Player GetOrCreatePlayer(int id, NetPeer netPeer)
        {
            Player? player = GetPlayer(id);
            if (player == null)
            {
                player = ReferencePool.Instance.Acquire<Player>();

                player.Init(id, netPeer);

                Players.TryAdd(id, player);
            }

            return player;
        }

        public Player? GetPlayer(int id)
        {
            Player? player;
            Players.TryGetValue(id, out player);
            return player;
        }

        public Player? GetPlayer(NetPeer netPeer)
        {
            return Players.Values.FirstOrDefault((a) => a.NetPeer == netPeer);
        }

        public void RemovePlayer(int id)
        {
            if (Players.TryRemove(id, out Player? player))
            {
                Room? room = player.Room;
                if (room != null)
                {
                    room.RemovePlayer(id);
                }

                ReferencePool.Instance.Release(player);
            }
        }
    }
}