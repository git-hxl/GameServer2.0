
using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
using Utils;

namespace GameServer
{
    public class OperationHandler
    {
        public void OnRequest(NetPeer peer, Request request, DeliveryMethod deliveryMethod)
        {
            switch (request.OperationCode)
            {
                case OperationCode.Login:
                    OnLogin(peer, request, deliveryMethod);
                    break;
                case OperationCode.Register:
                    OnRegister(peer, request, deliveryMethod);
                    break;
                case OperationCode.Disconnect:
                    OnDisconnect(peer, request, deliveryMethod);
                    break;

                case OperationCode.CreateRoom:
                    OnCreateRoom(peer, request, deliveryMethod);
                    break;
                case OperationCode.JoinRoom:
                    OnJoinRoom(peer, request, deliveryMethod);
                    break;
                case OperationCode.LeaveRoom:
                    OnLeaveRoom(peer, request, deliveryMethod);
                    break;
                case OperationCode.CloseRoom:
                    OnCloseRoom(peer, request, deliveryMethod);
                    break;
                case OperationCode.UpdateRoomInfo:
                    break;
                case OperationCode.UpdatePlayerInfo:
                    break;
                case OperationCode.SyncEvent:
                    OnSyncEvent(peer, request, deliveryMethod);
                    break;
                default:
                    break;
            }
        }

        private void OnRegister(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            RegisterRequest registerRequest = MessagePackSerializer.Deserialize<RegisterRequest>(request.Data);

            netPeer.SendResponse(OperationCode.Register, ReturnCode.Success, "", null, deliveryMethod);
        }

        private void OnLogin(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            LoginRequest loginRequest = MessagePackSerializer.Deserialize<LoginRequest>(request.Data);

            LoginResponse loginResponse = new LoginResponse();
            loginResponse.ID = 0;

            loginResponse.ServerTime = DateTimeUtil.TimeStamp;

            var respdata = MessagePackSerializer.Serialize(loginResponse);

            netPeer.SendResponse(OperationCode.Login, ReturnCode.Success, "", respdata, deliveryMethod);
        }

        private void OnDisconnect(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            var player = PlayerManager.Instance.GetPlayer(netPeer);

            if (player != null)
            {
                PlayerManager.Instance.RemovePlayer(player.ID);
            }

            netPeer.Disconnect();
        }

        private void OnCreateRoom(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            CreateRoomRequest createRoomRequest = MessagePackSerializer.Deserialize<CreateRoomRequest>(request.Data);

            RoomInfo roomInfo = createRoomRequest.RoomInfo;

            IRoom? room = RoomManager.Instance.GetRoom(roomInfo.RoomID);

            if (room != null)
            {
                netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.CreateRoomFail, "room is not existed", null, deliveryMethod);
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

            var respdata = MessagePackSerializer.Serialize(createRoomResponse);

            netPeer.SendResponse(OperationCode.CreateRoom, ReturnCode.Success, "", respdata, deliveryMethod);

        }

        private void OnJoinRoom(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            JoinRoomRequest joinRoomRequest = MessagePackSerializer.Deserialize<JoinRoomRequest>(request.Data);
            PlayerInfo? playerInfo = joinRoomRequest.PlayeInfo;
            IRoom? room = RoomManager.Instance.GetRoom(joinRoomRequest.RoomID);

            if (room == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, "room is not existed", null, deliveryMethod);
                return;
            }

            IPlayer? player = PlayerManager.Instance.GetPlayer(joinRoomRequest.PlayerID);

            if (player == null)
            {
                if (playerInfo == null)
                {
                    playerInfo = new PlayerInfo();
                    playerInfo.PlayerID = joinRoomRequest.PlayerID;
                }

                if (playerInfo.IsRobot)
                {
                    player = PlayerManager.Instance.CreatePlayer<Robot>(joinRoomRequest.PlayerID, null);
                }
                else
                {
                    player = PlayerManager.Instance.CreatePlayer<Player>(joinRoomRequest.PlayerID, netPeer);
                }

                if (player != null)
                {
                    player.OnUpdatePlayerInfo(playerInfo);
                }
            }

            if (player == null)
            {
                netPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.JoinRoomFail, "player is not existed", null, deliveryMethod);
                return;
            }

            room.OnJoinPlayer(player);
        }

        private void OnLeaveRoom(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            LeaveRoomRequest leaveRoomRequest = MessagePackSerializer.Deserialize<LeaveRoomRequest>(request.Data);

            var player = PlayerManager.Instance.GetPlayer(leaveRoomRequest.PlayerID);

            if (player != null && player.Room != null)
            {
                player.Room.OnLeavePlayer(player);
            }
            else
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, "player or room is not existed", null, deliveryMethod);
            }
        }


        private void OnCloseRoom(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            CloseRoomRequest closeRoomRequest = MessagePackSerializer.Deserialize<CloseRoomRequest>(request.Data);

            var room = RoomManager.Instance.GetRoom(closeRoomRequest.RoomID);
            if (room != null)
            {
                RoomManager.Instance.CloseRoom(closeRoomRequest.RoomID);
            }
        }


        private void OnSyncEvent(NetPeer netPeer, Request request, DeliveryMethod deliveryMethod)
        {
            SyncEventRequest syncRequest = MessagePackSerializer.Deserialize<SyncEventRequest>(request.Data);

            var player = PlayerManager.Instance.GetPlayer(syncRequest.PlayerID);

            if (player != null && player.Room != null)
            {
                foreach (var item in player.Room.Players)
                {
                    if (item.Value.NetPeer != null)
                    {
                        item.Value.NetPeer.SendResponse(OperationCode.SyncEvent, ReturnCode.Success,"", request.Data, deliveryMethod);
                    }
                }
            }
        }
    }
}