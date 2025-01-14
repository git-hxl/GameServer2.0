
using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
using Utils;

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

            netPeer.SendResponse(OperationCode.Register, ReturnCode.Success, null, deliveryMethod);
        }

        private void OnLogin(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LoginRequest loginRequest = MessagePackSerializer.Deserialize<LoginRequest>(data);

            LoginResponse loginResponse = new LoginResponse();
            loginResponse.ID = 0;

            loginResponse.ServerTime = DateTimeUtil.TimeStamp;

            var respdata = MessagePackSerializer.Serialize(loginResponse);

            netPeer.SendResponse(OperationCode.Login, ReturnCode.Success, respdata, deliveryMethod);
        }

        private void OnDisconnect(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            var player = PlayerManager.Instance.GetPlayer(netPeer);

            if (player != null)
            {
                PlayerManager.Instance.RemovePlayer(player.ID);
            }

            netPeer.Disconnect();
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

            var respdata = MessagePack.MessagePackSerializer.Serialize(createRoomResponse);

            netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.Success, respdata, deliveryMethod);

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

            if (player == null)
            {
                if (playerInfo == null)
                {
                    playerInfo = new PlayerInfo();
                    playerInfo.PlayerID = request.PlayerID;
                }

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
            CloseRoomRequest request = MessagePackSerializer.Deserialize<CloseRoomRequest>(data);

            var room = RoomManager.Instance.GetRoom(request.RoomID);
            if (room != null)
            {
                RoomManager.Instance.CloseRoom(request.RoomID);
            }
        }


        private void OnSyncEvent(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncRequestData syncRequest = MessagePackSerializer.Deserialize<SyncRequestData>(data);

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
