using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class Room : IReference
    {
        public int RoomID { get; private set; }

        public int MasterID { get; private set; }
        public ConcurrentDictionary<int, Player> Players { get; } = new ConcurrentDictionary<int, Player>();
        /// <summary>
        /// 不活动时间
        /// </summary>
        public float InactiveTime { get; private set; }
        public bool Inactived { get; private set; }

        public virtual void Init(int roomID)
        {
            RoomID = roomID;

            MasterID = -1;

            Inactived = false;
            InactiveTime = 0;
        }

        public bool AddPlayer(Player player)
        {
            if (Players.TryAdd(player.ID, player))
            {
                player.OnJoinRoom(this);

                if (MasterID == -1)
                {
                    MasterID = player.ID;
                }

                return true;
            }
            return false;
        }

        public Player? GetPlayer(int playerID)
        {
            Player? player;
            Players.TryGetValue(playerID, out player);

            return player;
        }

        public void RemovePlayer(int id)
        {
            if (Players.TryRemove(id, out Player? player))
            {
                player.OnExitRoom();

                if (MasterID == id)
                {
                    Player? master = Players.Values.FirstOrDefault((a) => a.NetPeer != null);
                    MasterID = master != null ? master.ID : -1;
                }
            }
        }

        public void OnCloseRoom()
        {
            Log.Information("OnCloseRoom RoomID:{0}", RoomID);

            foreach (var item in Players)
            {
                item.Value.OnExitRoom();
            }

            Players.Clear();

            RoomID = -1;
        }

        public void OnAcquire()
        {
            //throw new NotImplementedException();
        }

        public void OnRelease()
        {
            //throw new NotImplementedException();
        }

        public virtual void Update()
        {
            if (Players.Count == 0)
            {
                InactiveTime += Server.DeltaTime;

                if (InactiveTime > 1 * 60 * 60)
                {
                    Inactived = true;
                }
            }
        }
    }
}