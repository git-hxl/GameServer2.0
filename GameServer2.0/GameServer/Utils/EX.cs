
using GameServer.Protocol;
using LiteNetLib.Utils;
using LiteNetLib;
using MessagePack;
using Utils;
using Serilog;

namespace GameServer.Utils
{
    public static class EX
    {
        public static void SendResponse(this NetPeer netPeer, OperationCode code, ReturnCode returnCode, BaseResponse? baseResponse, string? returnMsg = "", DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (netPeer == null)
                return;

            NetDataWriter netDataWriter = new NetDataWriter();

            netDataWriter.Put((ushort)code);
            netDataWriter.Put((ushort)returnCode);

            if (baseResponse == null)
            {
                baseResponse = new BaseResponse();
            }

            baseResponse.ErrorMsg = returnMsg;
            baseResponse.Timestamp = DateTimeUtil.TimeStamp;

            Type type = baseResponse.GetType();

            byte[] data = MessagePackSerializer.Serialize(type, baseResponse);

            netDataWriter.Put(data);

            netPeer.Send(netDataWriter, deliveryMethod);
        }

        public static void SendSyncEvent(this NetPeer netPeer, byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (netPeer == null)
                return;

            NetDataWriter netDataWriter = new NetDataWriter();

            netDataWriter.Put((ushort)OperationCode.SyncEvent);
            netDataWriter.Put((ushort)ReturnCode.Success);

            netDataWriter.Put(data);

            netPeer.Send(netDataWriter, deliveryMethod);
        }
    }
}
