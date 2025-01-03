
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
                    OnJoinRoom(peer, data, deliveryMethod);
                    break;
                case OperationCode.LeaveRoom:
                    OnLeaveRoom(peer, data, deliveryMethod);
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

        private void OnJoinRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomRequest joinRoomRequest = MessagePackSerializer.Deserialize<JoinRoomRequest>(data);

            OLDRoom room = RoomManager.Instance.GetOrCreateRoom(joinRoomRequest.RoomID);

            OLDPlayer? player = room.GetPlayer(joinRoomRequest.PlayerID);

            if (player == null)
            {
                if (joinRoomRequest.IsRobot)
                {
                    player = OLDPlayerManager.Instance.GetOrCreateRobot(joinRoomRequest.PlayerID);
                }
                else
                {
                    player = OLDPlayerManager.Instance.GetOrCreatePlayer(joinRoomRequest.PlayerID, netPeer);
                }

                PlayerInfo playerInfo = new PlayerInfo();
                playerInfo.PlayerID = player.ID;

                data = MessagePackSerializer.Serialize(playerInfo);

                room.AddPlayer(player);
            }
            else
            {
                netPeer.SendResponse(OperationCode.OnJoinRoom, ReturnCode.JoinRoomFail, null, deliveryMethod);
            }
        }

        private void OnLeaveRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomRequest request = MessagePackSerializer.Deserialize<LeaveRoomRequest>(data);

            OLDPlayer? player = OLDPlayerManager.Instance.GetPlayer(request.PlayerID);

            if (player != null && player.Room != null)
            {
                OLDRoom room = player.Room;
                room.RemovePlayer(request.PlayerID);
            }
            else
            {
                netPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.LeaveRoomFail, null, deliveryMethod);
            }
        }

        private void OnSyncEvent(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            //SyncEventRequest eventData = MessagePackSerializer.Deserialize<SyncEventRequest>(data);

            OLDPlayer? player = OLDPlayerManager.Instance.GetPlayer(netPeer);

            if (player != null)
            {
                OLDRoom? room = player.Room;
                if (room != null)
                {
                    room.SendToAll(OperationCode.SyncEvent, ReturnCode.Success, data, deliveryMethod);
                }
            }
        }
    }
}
