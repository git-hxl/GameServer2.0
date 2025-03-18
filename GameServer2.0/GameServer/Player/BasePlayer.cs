
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

        public UserInfo? UserInfo { get; protected set; }

        public float InactiveTime { get; private set; }

        public int RoomID { get; protected set; }

        public virtual void OnInit(int id, NetPeer? netPeer)
        {
            ID = id;

            NetPeer = netPeer;

            InactiveTime = 0;

            RoomID = -1;
        }

        public void OnAcquire()
        {

        }

        public void OnRelease()
        {
            ID = -1;
            NetPeer = null;
        }

        public virtual void OnJoinRoom(BaseRoom room)
        {
            RoomID = room.ID;
            Log.Information("OnJoinRoom PlayerID {0} RoomID {1}", ID, room.ID);
        }

        public virtual void OnLeaveRoom(BaseRoom room)
        {
            RoomID = -1;
            Log.Information("OnLeaveRoom PlayerID {0} RoomID {1}", ID, room.ID);

            PlayerManager.Instance.RemovePlayer(ID);
        }

        public virtual void UpdatePlayerInfo(UserInfo userInfo)
        {
            UserInfo = userInfo;
        }

        /// <summary>
        /// 每帧调用
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {
            if (NetPeer == null || (NetPeer != null && NetPeer.ConnectionState != ConnectionState.Connected))
            {
                InactiveTime += deltaTime;

                //移除长时间掉线的玩家 60s
                if (InactiveTime > 60f)
                {
                    var room = RoomManager.Instance.GetRoom<BaseRoom>(RoomID);
                    if (room != null)
                    {
                        room.RemovePlayer(this);
                    }
                    else
                    {
                        PlayerManager.Instance.RemovePlayer(ID);
                    }
                }
            }
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

            Type type = baseRequest.GetType();

            byte[] data = MessagePackSerializer.Serialize(type, baseRequest);

            netDataWriter.Put(data);

            NetPeer.Send(netDataWriter, deliveryMethod);
        }
    }
}
