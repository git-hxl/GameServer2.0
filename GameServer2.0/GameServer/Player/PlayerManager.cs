
using LiteNetLib;
using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class PlayerManager : Singleton<PlayerManager>, IPlayerManager
    {
        public ConcurrentDictionary<int, IPlayer> Players { get; private set; } = new ConcurrentDictionary<int, IPlayer>();

        public T? CreatePlayer<T>(int id, NetPeer? netPeer) where T : IPlayer, new()
        {
            IPlayer player = ReferencePool.Instance.Acquire<T>();

            if (Players.TryAdd(id, player))
            {
                player.OnInit(id, netPeer);

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