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

                OnPlayerJoinRoomResponse(player);

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
                OnPlayerLeaveRoomResponse(player);
            }
        }

        public void OnCloseRoom()
        {
            Log.Information("OnCloseRoom RoomID:{0}", RoomID);

            NoticeToAll(OperationCode.OnRoomClose, ReturnCode.Success, null);

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

        public void NoticeToPlayer(Player player, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (player.NetPeer != null)
            {
                player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
            }
        }

        public void NoticeToOthers(Player self, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
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

        public void NoticeToAll(OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
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


        public void OnPlayerJoinRoomResponse(Player player)
        {
            JoinRoomResponse joinRoomResponse = new JoinRoomResponse();

            joinRoomResponse.MasterID = MasterID;
            joinRoomResponse.RoomID = RoomID;
            joinRoomResponse.Others = new List<PlayerInfoInRoom>();

            foreach (var item in Players)
            {
                if (item.Value != player)
                {
                    PlayerInfoInRoom otherInfo = GetPlayerInfoInRoom(item.Value);
                    joinRoomResponse.Others.Add(otherInfo);
                }
            }

            byte[] data = MessagePackSerializer.Serialize(joinRoomResponse);

            NoticeToPlayer(player, OperationCode.OnJoinRoom, ReturnCode.Success, data);

            PlayerInfoInRoom playerInfoInRoom = new PlayerInfoInRoom();
            playerInfoInRoom.PlayerID = player.ID;

            data = MessagePackSerializer.Serialize(playerInfoInRoom);

            NoticeToOthers(player, OperationCode.OnOtherJoinRoom, ReturnCode.Success, data);
        }

        public void OnPlayerLeaveRoomResponse(Player player)
        {
            NoticeToPlayer(player, OperationCode.OnLeaveRoom, ReturnCode.Success, null);

            PlayerInfoInRoom playerInfoInRoom = GetPlayerInfoInRoom(player);
            byte[] data = MessagePackSerializer.Serialize(playerInfoInRoom);

            NoticeToOthers(player, OperationCode.OnOtherJoinRoom, ReturnCode.Success, data);
        }

        public PlayerInfoInRoom GetPlayerInfoInRoom(Player player)
        {
            PlayerInfoInRoom playerInfoInRoom = new PlayerInfoInRoom();
            playerInfoInRoom.PlayerID = player.ID;
            return playerInfoInRoom;
        }
    }
}