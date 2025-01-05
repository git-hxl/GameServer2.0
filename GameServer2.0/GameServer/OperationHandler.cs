
using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;

namespace GameServer
{
    public class OperationHandler
    {
        public void OnRequest(NetPeer peer, OperationCode code, byte[] data, DeliveryMethod deliveryMethod)
        {
            switch (code)
            {
                case OperationCode.Login:
                    OnLogin(peer, data, deliveryMethod);
                    break;
                case OperationCode.Register:
                    OnRegister(peer, data, deliveryMethod);
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

            netPeer.SendResponse(OperationCode.Register, ReturnCode.Success, null, deliveryMethod);
        }

        private void OnLogin(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LoginRequest loginRequest = MessagePackSerializer.Deserialize<LoginRequest>(data);

            netPeer.SendResponse(OperationCode.Login, ReturnCode.Success, null, deliveryMethod);
        }


        private void OnCreateRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            CreateRoomRequest createRoomRequest = MessagePackSerializer.Deserialize<CreateRoomRequest>(data);

            RoomInfo roomInfo = createRoomRequest.RoomInfo;

            IRoom? room = RoomManager.Instance.GetRoom(roomInfo.RoomID);

            if (room != null)
            {
                netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.CreateRoomFail, null, deliveryMethod);
                return;
            }

            room = RoomManager.Instance.CreateRoom<Room>(roomInfo.RoomID);

            if (room != null)
            {
                room.OnUpdateRoomInfo(roomInfo);
            }

            CreateRoomResponse createRoomResponse = new CreateRoomResponse();
            createRoomResponse.PlayerID = createRoomRequest.PlayerID;

            createRoomResponse.RoomInfo = roomInfo;

            data = MessagePack.MessagePackSerializer.Serialize(createRoomResponse);

            netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.Success, data, deliveryMethod);

        }

        private void OnJoinRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomRequest request = MessagePackSerializer.Deserialize<JoinRoomRequest>(data);
            PlayerInfo? playerInfo = request.PlayeInfo;
            IRoom? room = RoomManager.Instance.GetRoom(request.RoomID);

            if (room == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, deliveryMethod);
                return;
            }

            IPlayer? player = PlayerManager.Instance.GetPlayer(request.PlayerID);

            if (player == null && playerInfo != null)
            {
                if (playerInfo.IsRobot)
                {
                    player = PlayerManager.Instance.CreatePlayer<Robot>(request.PlayerID, null);
                }
                else
                {
                    player = PlayerManager.Instance.CreatePlayer<Player>(request.PlayerID, netPeer);
                }

                if (player != null)
                {
                    player.OnUpdatePlayerInfo(playerInfo);
                }
            }

            if (player == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, null, deliveryMethod);
                return;
            }

            room.OnJoinPlayer(player);
        }

        private void OnLeaveRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomRequest request = MessagePackSerializer.Deserialize<LeaveRoomRequest>(data);

            var player = PlayerManager.Instance.GetPlayer(request.PlayerID);

            if (player != null && player.Room != null)
            {
                player.Room.OnLeavePlayer(player);
            }
            else
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, deliveryMethod);
            }
        }


        private void OnCloseRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            int roomID = MessagePackSerializer.Deserialize<int>(data);

            var room = RoomManager.Instance.GetRoom(roomID);
            if (room != null)
            {
                room.OnCloseRoom();
            }
        }


        private void OnSyncEvent(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncEventRequest syncRequest = MessagePackSerializer.Deserialize<SyncEventRequest>(data);

            var player = PlayerManager.Instance.GetPlayer(syncRequest.PlayerID);

            if (player != null && player.Room != null)
            {
                foreach (var item in player.Room.Players)
                {
                    if (item.Value.NetPeer != null)
                    {
                        item.Value.NetPeer.SendResponse(OperationCode.SyncEvent, ReturnCode.Success, data, deliveryMethod);
                    }
                }
            }
        }
    }
}
