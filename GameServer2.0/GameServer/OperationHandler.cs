
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

            Log.Information("OnCreateRoom peerID:{0} roomID:{1} userID:{2}", netPeer.Id, createRoomRequest.RoomID, createRoomRequest.UserID);

            BaseRoom? room = RoomManager.Instance.GetRoom<BaseRoom>(createRoomRequest.RoomID);

            if (room != null)
            {
                netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.CreateRoomFail, null, "room is existed", deliveryMethod);
                return;
            }

            room = RoomManager.Instance.CreateRoom<BaseRoom>(createRoomRequest.RoomID);

            if (room == null)
            {
                netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.CreateRoomFail, null, "room is null", deliveryMethod);
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

            Log.Information("OnJoinRoom peerID:{0} roomID:{1} userID:{2}", netPeer.Id, joinRoomRequest.RoomID, joinRoomRequest.UserID);

            UserInfo? userInfo = joinRoomRequest.UserInfo;

            BaseRoom? room = RoomManager.Instance.GetRoom<BaseRoom>(joinRoomRequest.RoomID);

            if (room == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, "room is not existed", deliveryMethod);
                return;
            }

            BasePlayer? player = PlayerManager.Instance.CreatePlayer<BasePlayer>(joinRoomRequest.UserID, netPeer);

            if (player == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, "player created failed", deliveryMethod);
                return;
            }

            var playerRoom = RoomManager.Instance.GetRoom<BaseRoom>(player.RoomID);

            if (playerRoom != null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, "player is in room", deliveryMethod);
                return;
            }

            player.OnUpdatePlayerInfo(userInfo);
            room.OnJoinPlayer(player);
        }

        private void OnLeaveRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomRequest leaveRoomRequest = MessagePackSerializer.Deserialize<LeaveRoomRequest>(data);

            Log.Information("OnLeaveRoom peerID:{0} roomID:{1} userID:{2}", netPeer.Id, leaveRoomRequest.RoomID, leaveRoomRequest.UserID);

            BasePlayer? player = PlayerManager.Instance.GetPlayer<BasePlayer>(leaveRoomRequest.UserID);
            if (player == null)
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, "player is null", deliveryMethod);
                return;
            }

            var playerRoom = RoomManager.Instance.GetRoom<BaseRoom>(player.RoomID);

            if (playerRoom == null)
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, "player is not in room", deliveryMethod);
                return;
            }

            playerRoom.OnLeavePlayer(player);
        }


        private void OnCloseRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            CloseRoomRequest closeRoomRequest = MessagePackSerializer.Deserialize<CloseRoomRequest>(data);

            Log.Information("OnCloseRoom peerID:{0} roomID:{1} userID:{2}", netPeer.Id, closeRoomRequest.RoomID, closeRoomRequest.UserID);

            var room = RoomManager.Instance.GetRoom<BaseRoom>(closeRoomRequest.RoomID);
            if (room != null)
            {
                RoomManager.Instance.CloseRoom(closeRoomRequest.RoomID);
            }
        }

        private void OnUpdatePlayerInfo(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            UserInfo userInfo = MessagePackSerializer.Deserialize<UserInfo>(data);

            Log.Information("OnUpdatePlayerInfo peerID:{0} userID:{1}", netPeer.Id, userInfo.UserID);

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

            Log.Information("OnUpdateRoomInfo peerID:{0} roomID:{1}", netPeer.Id, roomInfo.RoomID);

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