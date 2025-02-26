
using GameServer.Protocol;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using Serilog;
using Utils;

namespace GameServer
{
    public class BasePlayer : IReference
    {
        public int ID { get; protected set; }

        public NetPeer? NetPeer { get; protected set; }

        public BaseRoom? Room { get; protected set; }

        public UserInfo? UserInfo { get; protected set; }

        public virtual void OnInit(int id, NetPeer? netPeer)
        {
            ID = id;

            NetPeer = netPeer;
        }

        public void OnAcquire()
        {

        }

        public virtual void OnJoinRoom(BaseRoom room)
        {
            if (Room != null)
            {
                Room.OnLeavePlayer(this);
            }

            Room = room;

            Log.Information("OnJoinRoom PlayerID {0} RoomID {1}", ID, Room.ID);
        }

        public virtual void OnLeaveRoom()
        {
            if (Room != null)
            {
                Log.Information("OnLeaveRoom PlayerID {0} RoomID {1}", ID, Room.ID);
                Room = null;
            }
        }

        public virtual void OnUpdatePlayerInfo(UserInfo userInfo)
        {
            UserInfo = userInfo;
        }


        /// <summary>
        /// 每帧调用
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {

        }

        public void OnRelease()
        {
            ID = -1;
            Room = null;

            NetPeer = null;
        }


        public void SendRequest(OperationCode code, BaseRequest? baseRequest, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (NetPeer == null)
                return;

            NetDataWriter netDataWriter = new NetDataWriter();

            netDataWriter.Put((ushort)code);

            if (baseRequest == null)
            {
                baseRequest = new BaseRequest();
            }

            baseRequest.UserID = ID;
            baseRequest.Timestamp = DateTimeUtil.TimeStamp;

            byte[] data = MessagePackSerializer.Serialize(baseRequest);

            netDataWriter.Put(data);

            NetPeer.Send(netDataWriter, deliveryMethod);
        }

        public void SendResponse(OperationCode code, ReturnCode returnCode, BaseResponse? baseResponse, string? returnMsg = "", DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (NetPeer == null)
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

            byte[] data = MessagePackSerializer.Serialize(baseResponse);

            netDataWriter.Put(data);

            NetPeer.Send(netDataWriter, deliveryMethod);
        }

        public void SendSyncEvent(byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (NetPeer == null)
                return;

            NetDataWriter netDataWriter = new NetDataWriter();

            netDataWriter.Put((ushort)OperationCode.SyncEvent);
            netDataWriter.Put((ushort)ReturnCode.Success);

            netDataWriter.Put(data);

            NetPeer.Send(netDataWriter, deliveryMethod);
        }
    }
}
