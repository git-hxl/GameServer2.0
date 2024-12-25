
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
                case OperationCode.JoinRoom:
                    OnJointRoom(peer, data, deliveryMethod);
                    break;
                case OperationCode.OnJoinRoom:
                    break;
                case OperationCode.OnOtherJoinRoom:
                    break;
                case OperationCode.LeaveRoom:
                    OnLeaveRoom(peer, data, deliveryMethod);
                    break;
                case OperationCode.OnLeaveRoom:
                    break;
                case OperationCode.OnOtherLeaveRoom:
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

        private void OnJointRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomRequest joinRoomRequest = MessagePackSerializer.Deserialize<JoinRoomRequest>(data);

            Room room = RoomManager.Instance.GetOrCreateRoom(joinRoomRequest.RoomID);

            Player? player = room.GetPlayer(joinRoomRequest.PlayerID);

            if (player == null)
            {
                player = PlayerManager.Instance.GetOrCreatePlayer(joinRoomRequest.PlayerID, netPeer);

                PlayerInfoInRoom playerInfoInRoom = new PlayerInfoInRoom();
                playerInfoInRoom.PlayerID = player.ID;

                data = MessagePackSerializer.Serialize(playerInfoInRoom);

                room.AddPlayer(player);

                JoinRoomResponse joinRoomResponse = new JoinRoomResponse();

                joinRoomResponse.MasterID = room.MasterID;
                joinRoomResponse.RoomID = room.RoomID;
                joinRoomResponse.Others = new List<PlayerInfoInRoom>();

                foreach (var item in room.Players)
                {
                    Player other = item.Value;
                    PlayerInfoInRoom otherInfo = new PlayerInfoInRoom();
                    otherInfo.PlayerID = other.ID;
                    joinRoomResponse.Others.Add(otherInfo);
                    if (other != player && other.NetPeer != null)
                    {
                        other.NetPeer.SendResponse(OperationCode.OnOtherJoinRoom, ReturnCode.Success, data, deliveryMethod);
                    }
                }

                data = MessagePackSerializer.Serialize(joinRoomResponse);

                netPeer.SendResponse(OperationCode.OnJoinRoom, ReturnCode.Success, data, deliveryMethod);
            }
            else
            {
                netPeer.SendResponse(OperationCode.OnJoinRoom, ReturnCode.JoinRoomFail, null, deliveryMethod);
            }
        }

        private void OnLeaveRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomRequest request = MessagePackSerializer.Deserialize<LeaveRoomRequest>(data);

            Player? player = PlayerManager.Instance.GetPlayer(request.PlayerID);

            if (player != null && player.Room != null)
            {
                Room room = player.Room;
                room.RemovePlayer(request.PlayerID);

                PlayerInfoInRoom playerInfoInRoom = new PlayerInfoInRoom();
                playerInfoInRoom.PlayerID = player.ID;

                data = MessagePackSerializer.Serialize(playerInfoInRoom);

                foreach (var item in room.Players)
                {
                    Player other = item.Value;

                    if (other.NetPeer != null)
                    {
                        other.NetPeer.SendResponse(OperationCode.OnOtherLeaveRoom, ReturnCode.Success, data, deliveryMethod);
                    }
                }

                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.Success, null, deliveryMethod);
            }
            else
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, deliveryMethod);
            }
        }

        private void OnSyncEvent(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncEventData eventData = MessagePackSerializer.Deserialize<SyncEventData>(data);

            Player? player = PlayerManager.Instance.GetPlayer(eventData.PlayerID);

            if (player != null)
            {
                Room? room = player.Room;
                if (room != null)
                {
                    foreach (var item in room.Players)
                    {
                        player = item.Value;

                        if (player.NetPeer != null)
                        {
                            player.NetPeer.SendResponse(OperationCode.SyncEvent, ReturnCode.Success, data, deliveryMethod);
                        }
                    }
                }
            }
        }
    }
}
