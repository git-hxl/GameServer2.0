
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

            return default(T);
        }

        public void RemovePlayer(int id)
        {
            BasePlayer? basePlayer;
            if (Players.TryRemove(id, out basePlayer))
            {
                var room = RoomManager.Instance.GetRoom<BaseRoom>(basePlayer.ID);
                if (room != null)
                {
                    Log.Error("玩家移除异常 检查是否已经退出房间");
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