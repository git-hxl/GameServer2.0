using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
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
                if (MasterID == id)
                {
                    Player? master = Players.Values.FirstOrDefault((a) => a.NetPeer != null);
                    MasterID = master != null ? master.ID : -1;
                }
                player.OnExitRoom();
                OnPlayerExit(player);
            }
        }

        public void OnCloseRoom()
        {
            Log.Information("OnCloseRoom RoomID:{0}", RoomID);

            SendToAll(OperationCode.OnRoomClose, ReturnCode.Success, null);

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

            foreach (var item in Players)
            {
                Player player = item.Value;

                player.OnUpdate();
            }
        }

        public void SendToPlayer(Player player, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (player.NetPeer != null)
            {
                player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
            }
        }

        public void SendToOthers(Player self, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in Players)
            {
                Player player = item.Value;
                if (player.NetPeer != null && player != self)
                {
                    player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
                }
            }
        }

        public void SendToAll(OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in Players)
            {
                Player player = item.Value;
                if (player.NetPeer != null)
                {
                    player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
                }
            }
        }


        public void OnPlayerJoin(Player player)
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

            SendToPlayer(player, OperationCode.OnJoinRoom, ReturnCode.Success, data);

            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.PlayerID = player.ID;

            data = MessagePackSerializer.Serialize(playerInfo);

            SendToOthers(player, OperationCode.OnOtherJoinRoom, ReturnCode.Success, data);
        }

        public void OnPlayerExit(Player player)
        {
            SendToPlayer(player, OperationCode.OnLeaveRoom, ReturnCode.Success, null);

            PlayerInfo playerInfo = GetPlayerInfo(player);
            byte[] data = MessagePackSerializer.Serialize(playerInfo);

            SendToOthers(player, OperationCode.OnOtherJoinRoom, ReturnCode.Success, data);
        }

        public PlayerInfo GetPlayerInfo(Player player)
        {
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.PlayerID = player.ID;
            return playerInfo;
        }
    }
}