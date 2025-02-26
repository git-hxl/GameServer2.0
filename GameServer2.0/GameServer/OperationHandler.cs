
using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
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
                    break;
                case OperationCode.UpdatePlayerInfo:
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
            //todo:
        }

        private void OnLogin(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LoginRequest loginRequest = MessagePackSerializer.Deserialize<LoginRequest>(data);
            //todo:

            BasePlayer player = PlayerManager.Instance.CreatePlayer<BasePlayer>(loginRequest.UserID, netPeer);

            if (player != null)
            {
                player.SendResponse(OperationCode.Login, ReturnCode.Success, null);
            }
        }

        private void OnDisconnect(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            netPeer.Disconnect();
        }

        private void OnCreateRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            CreateRoomRequest createRoomRequest = MessagePackSerializer.Deserialize<CreateRoomRequest>(data);

            RoomInfo roomInfo = createRoomRequest.RoomInfo;

            BaseRoom? room = RoomManager.Instance.CreateRoom<BaseRoom>(roomInfo.RoomID);

            BasePlayer? player = PlayerManager.Instance.GetPlayer<BasePlayer>(createRoomRequest.UserID);

            if (player == null)
                return;

            if (room == null)
            {
                player.SendResponse(OperationCode.CreateRoom, ReturnCode.CreateRoomFail, null, "room is not existed", deliveryMethod);
                return;
            }

            room = RoomManager.Instance.CreateRoom<BaseRoom>(roomInfo.RoomID);

            if (room != null)
            {
                room.OnUpdateRoomInfo(roomInfo);
            }

            CreateRoomResponse createRoomResponse = new CreateRoomResponse();
            createRoomResponse.RoomInfo = roomInfo;

            var respdata = MessagePackSerializer.Serialize(createRoomResponse);

            player.SendResponse(OperationCode.CreateRoom, ReturnCode.Success, createRoomResponse);
        }

        private void OnJoinRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomRequest joinRoomRequest = MessagePackSerializer.Deserialize<JoinRoomRequest>(data);
            UserInfo? userInfo = joinRoomRequest.UserInfo;

            BaseRoom? room = RoomManager.Instance.GetRoom<BaseRoom>(joinRoomRequest.RoomID);

            BasePlayer? player = PlayerManager.Instance.GetPlayer<BasePlayer>(joinRoomRequest.UserID);

            if (player == null)
                return;

            if (room == null)
            {
                player.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, "room is not existed", deliveryMethod);
                return;
            }

            player.OnUpdatePlayerInfo(userInfo);

            room.OnJoinPlayer(player);
        }

        private void OnLeaveRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomRequest leaveRoomRequest = MessagePackSerializer.Deserialize<LeaveRoomRequest>(data);
            BasePlayer? player = PlayerManager.Instance.GetPlayer<BasePlayer>(leaveRoomRequest.UserID);

            if (player == null)
                return;

            if (player.Room != null)
            {
                player.Room.OnLeavePlayer(player);
            }
            else
            {
                player.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, "player is not in room", deliveryMethod);
            }
        }


        private void OnCloseRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            CloseRoomRequest closeRoomRequest = MessagePackSerializer.Deserialize<CloseRoomRequest>(data);

            var room = RoomManager.Instance.GetRoom<BaseRoom>(closeRoomRequest.RoomID);
            if (room != null)
            {
                RoomManager.Instance.CloseRoom(closeRoomRequest.RoomID);
            }
        }


        private void OnSyncEvent(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncRequest syncRequest = MessagePackSerializer.Deserialize<SyncRequest>(data);

            var player = PlayerManager.Instance.GetPlayer<BasePlayer>(syncRequest.UserID);

            if (player != null && player.Room != null)
            {
                foreach (var item in player.Room.Players)
                {
                    if (item.Value != null)
                    {
                        item.Value.SendSyncEvent(data, deliveryMethod);
                    }
                }
            }
        }
    }
}