

using GameServer.Utils;
using LiteNetLib;
using System.Numerics;

namespace GameServer
{
    public static class RoomEx
    {

        public static void SendToOthers(this OLDRoom room, OLDPlayer self, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                OLDPlayer player = item.Value;
                if (player.NetPeer != null && player != self)
                {
                    player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
                }
            }
        }

        public static void SendToOthersSyncEvent(this OLDRoom room, OLDPlayer self, SyncCode code, byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                OLDPlayer player = item.Value;
                if (player.NetPeer != null && player != self)
                {
                    player.NetPeer.SendSyncEvent(self.ID, code, data, deliveryMethod);
                }
            }
        }

        public static void SendToAll(this OLDRoom room, OperationCode code, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                OLDPlayer player = item.Value;
                if (player.NetPeer != null)
                {
                    player.NetPeer.SendResponse(code, returnCode, data, deliveryMethod);
                }
            }
        }

        public static void SendToAllSyncEvent(this OLDRoom room, OLDPlayer self, SyncCode code, byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            foreach (var item in room.Players)
            {
                OLDPlayer player = item.Value;
                if (player.NetPeer != null && IsInAOI(self, player))
                {
                    player.NetPeer.SendSyncEvent(self.ID, code, data, deliveryMethod);
                }
            }
        }


        public static bool IsInAOI(OLDPlayer self, OLDPlayer other)
        {
            Vector3 posSelf = self.Position;
            Vector3 posOther = other.Position;

            float rangle = 5;

            if (posOther.Z < posSelf.Z + rangle && posOther.Z > posSelf.Z - rangle && posOther.X < posSelf.X + rangle && posOther.X > posSelf.X - rangle)
            {
                return true;
            }

            return false;
        }
    }
}
