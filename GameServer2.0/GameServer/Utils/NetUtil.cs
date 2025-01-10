

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


        public static void SendSyncEvent(this NetPeer netPeer, int playerID, SyncEventCode syncCode, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncRequestData syncEventRequest = new SyncRequestData();
            syncEventRequest.PlayerID = playerID;
            syncEventRequest.SyncEventCode = (ushort)syncCode;

            syncEventRequest.SyncData = data;
            syncEventRequest.Timestamp = DateTimeUtil.TimeStamp;

            data = MessagePackSerializer.Serialize(syncEventRequest);

            SendResponse(netPeer, OperationCode.SyncEvent, ReturnCode.Success, data, deliveryMethod);
        }

    }
}
