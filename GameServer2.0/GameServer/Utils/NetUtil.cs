

using GameServer.Protocol;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using Utils;

namespace GameServer.Utils
{
    public static class NetUtil
    {
        public static void SendResponse(this NetPeer netPeer, OperationCode operationCode, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod)
        {
            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put((ushort)operationCode);
            netDataWriter.Put((ushort)returnCode);

            if (data != null)
            {
                netDataWriter.Put(data);
            }

            netPeer.Send(netDataWriter, deliveryMethod);
        }


        public static void SendSyncEvent(this NetPeer netPeer, int playerID, SyncCode syncCode, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncEventRequest syncEventRequest = new SyncEventRequest();
            syncEventRequest.PlayerID = playerID;
            syncEventRequest.SyncCode = (ushort)syncCode;

            syncEventRequest.SyncData = data;
            syncEventRequest.Timestamp = DateTimeUtil.TimeStamp;

            data = MessagePackSerializer.Serialize(syncEventRequest);

            SendResponse(netPeer, OperationCode.SyncEvent, ReturnCode.Success, data, deliveryMethod);
        }

    }
}
