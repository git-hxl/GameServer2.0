
using LiteNetLib;
using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class PlayerManager : Singleton<PlayerManager>, IPlayerManager
    {
        public ConcurrentDictionary<int, IPlayer> Players { get; private set; } = new ConcurrentDictionary<int, IPlayer>();

        public ConcurrentDictionary<int, int> PlayerIDs { get; private set; } = new ConcurrentDictionary<int, int>();

        public T? CreatePlayer<T>(int id, NetPeer? netPeer) where T : IPlayer, new()
        {
            IPlayer player = ReferencePool.Instance.Acquire<T>();

            if (Players.TryAdd(id, player))
            {
                player.OnInit(id, netPeer);

                if (netPeer != null)
                {
                    PlayerIDs.AddOrUpdate(netPeer.Id, id, (key, value) => value = id);
                }

                return (T)player;
            }

            ReferencePool.Instance.Release(player);

            Log.Information("CreatePlayer Failed,ID {0} is existed!", id);

            return default(T);
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

        public IPlayer? GetPlayer(NetPeer netPeer)
        {
            if (PlayerIDs.TryGetValue(netPeer.Id, out int playerID))
            {
                return GetPlayer(playerID);
            }
            return null;
        }

        public void RemovePlayer(int id)
        {
            IPlayer? player;
            if (Players.TryRemove(id, out player))
            {
                var room = player.Room;
                if (room != null)
                {
                    room.OnLeavePlayer(player);
                }
                if (player.NetPeer != null)
                {
                    PlayerIDs.TryRemove(player.NetPeer.Id, out _);
                }
                ReferencePool.Instance.Release(player);
            }
        }

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }
    }
}