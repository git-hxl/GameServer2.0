
using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using Serilog;
using Utils;

namespace GameServer
{
    public class OperationHandler
    {
        public void OnRequest(NetPeer peer, OperationCode operationCode, byte[] data, DeliveryMethod deliveryMethod)
        {
            switch (operationCode)
            {
                case OperationCode.Login:
                    OnLogin(peer, data, deliveryMethod);
                    break;
                case OperationCode.Register:
                    OnRegister(peer, data, deliveryMethod);
                    break;
                case OperationCode.Disconnect:
                    OnDisconnect(peer, data, deliveryMethod);
                    break;

                case OperationCode.CreateRoom:
                    OnCreateRoom(peer, data, deliveryMethod);
                    break;
                case OperationCode.JoinRoom:
                    OnJoinRoom(peer, data, deliveryMethod);
                    break;
                case OperationCode.LeaveRoom:
                    OnLeaveRoom(peer, data, deliveryMethod);
                    break;
                case OperationCode.CloseRoom:
                    OnCloseRoom(peer, data, deliveryMethod);
                    break;
                case OperationCode.UpdateRoomInfo:
                    OnUpdateRoomInfo(peer, data, deliveryMethod);
                    break;
                case OperationCode.UpdatePlayerInfo:
                    OnUpdatePlayerInfo(peer, data, deliveryMethod);
                    break;
                case OperationCode.SyncEvent:
                    OnSyncEvent(peer, data, deliveryMethod);
                    break;
                default:
                    break;
            }
        }

        private void OnRegister(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            RegisterRequest registerRequest = MessagePackSerializer.Deserialize<RegisterRequest>(data);

            Log.Information("OnRegister peerID:{0}", netPeer.Id);

            //todo:
        }

        private void OnLogin(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LoginRequest loginRequest = MessagePackSerializer.Deserialize<LoginRequest>(data);

            Log.Information("OnLogin peerID:{0}", netPeer.Id);
            //todo:


        }

        private void OnDisconnect(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            Log.Information("OnDisconnect peerID:{0}", netPeer.Id);
            netPeer.Disconnect();
        }

        private void OnCreateRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            CreateRoomRequest createRoomRequest = MessagePackSerializer.Deserialize<CreateRoomRequest>(data);

            Log.Information("创建房间 userID {0} roomID {1} ", createRoomRequest.UserID, createRoomRequest.RoomID);

            BaseRoom? room = RoomManager.Instance.CreateRoom<BaseRoom>(createRoomRequest.RoomID);

            if (room == null)
            {
                netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.CreateRoomFail, null, "room is existed", deliveryMethod);

                Log.Information("创建房间失败，房间已存在 roomId {0}", createRoomRequest.RoomID);

                return;
            }

            room.OnUpdateRoomInfo(createRoomRequest.RoomInfo);

            CreateRoomResponse createRoomResponse = new CreateRoomResponse();
            createRoomResponse.RoomInfo = room.RoomInfo;

            var respdata = MessagePackSerializer.Serialize(createRoomResponse);

            netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.Success, createRoomResponse);
        }

        private void OnJoinRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomRequest joinRoomRequest = MessagePackSerializer.Deserialize<JoinRoomRequest>(data);

            Log.Information("加入房间 userID {0} roomID {1} ", joinRoomRequest.UserID, joinRoomRequest.RoomID);

            UserInfo? userInfo = joinRoomRequest.UserInfo;

            BaseRoom? room = RoomManager.Instance.GetRoom<BaseRoom>(joinRoomRequest.RoomID);

            if (room == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, "room is not existed", deliveryMethod);

                Log.Information("加入房间失败 房间不存在 userID {0} roomID {1} ", joinRoomRequest.UserID, joinRoomRequest.RoomID);
                return;
            }

            BasePlayer? player = PlayerManager.Instance.CreatePlayer<BasePlayer>(joinRoomRequest.UserID, netPeer);

            if (player == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, "player created failed", deliveryMethod);
                Log.Information("加入房间失败 玩家创建失败 userID {0} roomID {1} ", joinRoomRequest.UserID, joinRoomRequest.RoomID);
                return;
            }

            player.OnUpdatePlayerInfo(userInfo);
            room.OnJoinPlayer(player);
        }

        private void OnLeaveRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomRequest leaveRoomRequest = MessagePackSerializer.Deserialize<LeaveRoomRequest>(data);

            Log.Information("离开房间 userID {0} roomID {1} ", leaveRoomRequest.UserID, leaveRoomRequest.RoomID);

            BasePlayer? player = PlayerManager.Instance.GetPlayer<BasePlayer>(leaveRoomRequest.UserID);
            if (player == null)
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, "player is null", deliveryMethod);
                Log.Information("离开房间失败 玩家不存在 userID {0} roomID {1} ", leaveRoomRequest.UserID, leaveRoomRequest.RoomID);
                return;
            }

            var playerRoom = RoomManager.Instance.GetRoom<BaseRoom>(player.RoomID);

            if (playerRoom == null)
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, "player is not in room", deliveryMethod);
                Log.Information("离开房间失败 房间不存在 userID {0} roomID {1} ", leaveRoomRequest.UserID, leaveRoomRequest.RoomID);
                return;
            }

            playerRoom.OnLeavePlayer(player);
        }


        private void OnCloseRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            CloseRoomRequest closeRoomRequest = MessagePackSerializer.Deserialize<CloseRoomRequest>(data);

            Log.Information("关闭 userID {0} roomID {1} ", closeRoomRequest.UserID, closeRoomRequest.RoomID);

            var room = RoomManager.Instance.GetRoom<BaseRoom>(closeRoomRequest.RoomID);
            if (room != null)
            {
                RoomManager.Instance.CloseRoom(closeRoomRequest.RoomID);
            }
        }

        private void OnUpdatePlayerInfo(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            UserInfo userInfo = MessagePackSerializer.Deserialize<UserInfo>(data);

            Log.Information("更新玩家信息 userID {0}", userInfo.UserID);

            BasePlayer? player = PlayerManager.Instance.GetPlayer<BasePlayer>(userInfo.UserID);
            if (player == null)
            {
                netPeer.SendResponse(OperationCode.UpdatePlayerInfo, ReturnCode.UpdatePlayerInfoFail, null, "player is null", deliveryMethod);
                return;
            }

            netPeer.SendResponse(OperationCode.UpdatePlayerInfo, ReturnCode.Success, null, "", deliveryMethod);
        }

        private void OnUpdateRoomInfo(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            RoomInfo roomInfo = MessagePackSerializer.Deserialize<RoomInfo>(data);

            Log.Information("更新房间信息 peerID:{0} roomID:{1}", netPeer.Id, roomInfo.RoomID);

            BaseRoom? room = RoomManager.Instance.GetRoom<BaseRoom>(roomInfo.RoomID);

            if (room == null)
            {
                netPeer.SendResponse(OperationCode.UpdateRoomInfo, ReturnCode.UpdateRoomInfoFail, null, "room is not existed", deliveryMethod);
                return;
            }

            netPeer.SendResponse(OperationCode.UpdateRoomInfo, ReturnCode.Success, null, "", deliveryMethod);
        }


        private void OnSyncEvent(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncRequest syncRequest = MessagePackSerializer.Deserialize<SyncRequest>(data);

            var player = PlayerManager.Instance.GetPlayer<BasePlayer>(syncRequest.UserID);

            if (player == null)
            {
                netPeer.SendResponse(OperationCode.SyncEvent, ReturnCode.SyncEventFail, null, "player is null", deliveryMethod);

                return;
            }


            var playerRoom = RoomManager.Instance.GetRoom<BaseRoom>(player.RoomID);

            if (playerRoom == null)
            {
                netPeer.SendResponse(OperationCode.SyncEvent, ReturnCode.SyncEventFail, null, "player is not in room", deliveryMethod);
                return;
            }

            var players = playerRoom.GetActivePlayers();
            foreach (var item in players)
            {
                if (item.NetPeer != null)
                {
                    item.NetPeer.SendSyncEvent(data, deliveryMethod);
                }
            }
        }


    }
}