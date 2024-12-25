

using LiteNetLib;
using LiteNetLib.Utils;

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
    }
}
