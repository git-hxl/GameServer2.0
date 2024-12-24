
using GameServer.Protocol;
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
                case OperationCode.JoinRoom:
                    OnJointRoom(peer, data, deliveryMethod);
                    break;
            }
        }

        private void OnJointRoom(NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomRequest joinRoomRequest = MessagePackSerializer.Deserialize<JoinRoomRequest>(data);

            Room room = RoomManager.Instance.GetOrCreateRoom(joinRoomRequest.RoomID);

            Player player = PlayerManager.Instance.GetOrCreatePlayer(joinRoomRequest.PlayerID, netPeer);

            bool result = room.AddPlayer(player);

            if (result)
            {
                player.SendResponse(OperationCode.OnJoinRoom, ReturnCode.JoinRoomSuccess, null, deliveryMethod);
            }
            else
            {
                player.SendResponse(OperationCode.OnJoinRoom, ReturnCode.JoinRoomFail, null, deliveryMethod);
            }
        }
    }
}
