using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class OLDRoom : IReference
    {
        public int RoomID { get; private set; }

        public int MasterID { get; private set; }
        public ConcurrentDictionary<int, OLDPlayer> Players { get; } = new ConcurrentDictionary<int, OLDPlayer>();
        /// <summary>
        /// 不活动时间
        /// </summary>
        public float InactiveTime { get; private set; }
        public bool Inactived { get; private set; }


        private CancellationTokenSource? tokenSource;
        public virtual void Init(int roomID)
        {
            RoomID = roomID;

            MasterID = -1;

            Inactived = false;
            InactiveTime = 0;
        }

        public virtual void OnCreateRoom()
        {
            Log.Information("OnCreateRoom RoomID:{0}", RoomID);

            Task.Run(Update);
        }

        public bool AddPlayer(OLDPlayer player)
        {
            if (Players.TryAdd(player.ID, player))
            {
                if (MasterID == -1)
                {
                    MasterID = player.ID;
                }

                OnPlayerJoin(player);

                player.OnJoinRoom(this);
                return true;
            }
            return false;
        }

        public OLDPlayer? GetPlayer(int playerID)
        {
            OLDPlayer? player;
            Players.TryGetValue(playerID, out player);

            return player;
        }

        public void RemovePlayer(int id)
        {
            if (Players.TryRemove(id, out OLDPlayer? player))
            {
                if (MasterID == id)
                {
                    OLDPlayer? master = Players.Values.FirstOrDefault((a) => a.NetPeer != null);
                    MasterID = master != null ? master.ID : -1;
                }
                player.OnExitRoom();
                OnPlayerExit(player);
            }
        }

        public void OnCloseRoom()
        {
            this.SendToAll(OperationCode.OnRoomClose, ReturnCode.Success, null);

            foreach (var item in Players)
            {
                item.Value.OnExitRoom();
            }

            if (tokenSource != null)
            {
                tokenSource.Cancel();
                tokenSource = null;
            }

            Players.Clear();
            Log.Information("OnCloseRoom RoomID:{0}", RoomID);
        }

        public void OnAcquire()
        {
            //throw new NotImplementedException();
        }

        public void OnRelease()
        {
            //throw new NotImplementedException();
        }

        protected async virtual void Update()
        {
            tokenSource = new CancellationTokenSource();
            try
            {
                while (true)
                {
                    await Task.Delay((int)(Server.DeltaTime * 1000), tokenSource.Token);

                    foreach (var item in Players)
                    {
                        OLDPlayer player = item.Value;

                        player.OnUpdate();
                    }

                    if (Players.Count == 0)
                    {
                        InactiveTime += Server.DeltaTime;

                        if (InactiveTime > 10)
                        {
                            Inactived = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is not TaskCanceledException)
                    Log.Information(ex.ToString());
            }
            Log.Information("OnStopUpdateRoom RoomID:{0}", RoomID);
        }


        private void OnPlayerJoin(OLDPlayer player)
        {
            JoinRoomResponse joinRoomResponse = new JoinRoomResponse();

            joinRoomResponse.MasterID = MasterID;
            joinRoomResponse.RoomID = RoomID;
            joinRoomResponse.PlayerID = player.ID;
            joinRoomResponse.Others = new List<PlayerInfo>();

            foreach (var item in Players)
            {
                if (item.Value != player)
                {
                    PlayerInfo otherInfo = GetPlayerInfo(item.Value);
                    joinRoomResponse.Others.Add(otherInfo);
                }
            }

            byte[] data = MessagePackSerializer.Serialize(joinRoomResponse);

            if (player.NetPeer != null)
            {
                player.NetPeer.SendResponse(OperationCode.OnJoinRoom, ReturnCode.Success, data, DeliveryMethod.ReliableOrdered);
            }

            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.PlayerID = player.ID;

            data = MessagePackSerializer.Serialize(playerInfo);

            this.SendToOthers(player, OperationCode.OnOtherJoinRoom, ReturnCode.Success, data);
        }

        private void OnPlayerExit(OLDPlayer player)
        {
            if (player.NetPeer != null)
            {
                player.NetPeer.SendResponse(OperationCode.OnLeaveRoom, ReturnCode.Success, null, DeliveryMethod.ReliableOrdered);
            }

            PlayerInfo playerInfo = GetPlayerInfo(player);
            byte[] data = MessagePackSerializer.Serialize(playerInfo);

            this.SendToOthers(player, OperationCode.OnOtherJoinRoom, ReturnCode.Success, data);
        }

        public PlayerInfo GetPlayerInfo(OLDPlayer player)
        {
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.PlayerID = player.ID;
            return playerInfo;
        }
    }
}