
using LiteNetLib;
using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        public ConcurrentDictionary<int, BasePlayer> Players { get; } = new ConcurrentDictionary<int, BasePlayer>();


        public T? GetPlayer<T>(int id) where T : BasePlayer
        {
            BasePlayer? basePlayer = null;
            if (Players.TryGetValue(id, out basePlayer))
            {
                return basePlayer as T;
            }
            return null;
        }

        public T? CreatePlayer<T>(int userID, NetPeer netPeer) where T : BasePlayer, new()
        {
            BasePlayer? player = null;

            if (Players.TryGetValue(userID, out player))
            {
                return player as T;
            }

            player = ReferencePool.Instance.Acquire<T>();

            if (Players.TryAdd(userID, player))
            {
                player.OnInit(userID, netPeer);

                return player as T;
            }

            ReferencePool.Instance.Release(player);

            Log.Information("CreatePlayer Failed,UserID {0} is existed!", userID);

            return default(T);
        }

        public void RemovePlayer(int id)
        {
            BasePlayer? basePlayer;
            if (Players.TryRemove(id, out basePlayer))
            {
                var room = basePlayer.Room;
                if (room != null)
                {
                    room.OnLeavePlayer(basePlayer);
                }
                ReferencePool.Instance.Release(basePlayer);
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