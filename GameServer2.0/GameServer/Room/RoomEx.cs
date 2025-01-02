

using GameServer.Utils;
using LiteNetLib;

namespace GameServer
{
    public static class RoomEx
    {

        public static void SendToOthers(this Room room, Player self, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                Player player = item.Value;
                if (player.NetPeer != null && player != self)
                {
                    player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
                }
            }
        }

        public static void SendToOthersSyncEvent(this Room room, Player self, SyncCode code, byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                Player player = item.Value;
                if (player.NetPeer != null && player != self)
                {
                    player.NetPeer.SendSyncEvent(self.ID, code, data, deliveryMethod);
                }
            }
        }

        public static void SendToAll(this Room room, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                Player player = item.Value;
                if (player.NetPeer != null)
                {
                    player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
                }
            }
        }

        public static void SendToAllSyncEvent(this Room room, Player self, SyncCode code, byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                Player player = item.Value;
                if (player.NetPeer != null)
                {
                    player.NetPeer.SendSyncEvent(self.ID, code, data, deliveryMethod);
                }
            }
        }
    }
}
