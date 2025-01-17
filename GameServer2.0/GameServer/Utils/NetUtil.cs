

using GameServer.Protocol;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using Utils;

namespace GameServer.Utils
{
    public static class NetUtil
    {
        public static void SendRequest(this NetPeer netPeer, OperationCode operationCode, byte[] data, DeliveryMethod deliveryMethod)
        {
            Request request = new Request();
            request.OperationCode = operationCode;
            request.Timestamp = DateTimeUtil.TimeStamp;
            request.Data = data;

            var sendData = MessagePackSerializer.Serialize(request);

            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put(sendData);
            netPeer.Send(netDataWriter, deliveryMethod);
        }

        public static void SendResponse(this NetPeer netPeer, OperationCode operationCode, ReturnCode returnCode, string errorMsg, byte[] data, DeliveryMethod deliveryMethod)
        {
            Response response = new Response();
            response.OperationCode = operationCode;
            response.ReturnCode = returnCode;
            response.ErrorMsg = errorMsg;
            response.Timestamp = DateTimeUtil.TimeStamp;
            response.Data = data;

            var sendData = MessagePackSerializer.Serialize(response);

            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put(sendData);
            netPeer.Send(netDataWriter, deliveryMethod);
        }

        public static void SendSyncEvent(this NetPeer netPeer, int playerID, SyncEventCode syncEventCode, byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncEventRequest syncEventRequest = new SyncEventRequest();
            syncEventRequest.SyncEventCode = syncEventCode;
            syncEventRequest.SyncData = data;
            syncEventRequest.PlayerID = playerID;

            var sendData = MessagePackSerializer.Serialize(syncEventRequest);

            SendResponse(netPeer, OperationCode.SyncEvent, ReturnCode.Success, "", sendData, deliveryMethod);
        }
    }
}
