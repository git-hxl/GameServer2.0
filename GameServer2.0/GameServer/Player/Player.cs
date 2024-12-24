using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using Utils;

namespace GameServer
{
    public class Player : IReference
    {
        public int ID { get; private set; }

        public NetPeer? Peer { get; private set; }

        public Room? Room { get; private set; }

        public void OnAcquire()
        {
            //throw new NotImplementedException();
        }

        public void Init(int id, NetPeer netPeer)
        {
            ID = id;
            Peer = netPeer;
        }

        public void OnRelease()
        {
            //throw new NotImplementedException();

            ID = -1;
            Peer = null;
        }


        public virtual void OnJoinRoom(Room room)
        {
            if (Room != null)
            {
                if (Room != room)
                {
                    Room.RemovePlayer(ID);
                }
            }
            Room = room;

            Log.Information("OnJoinRoom ID:{0} RoomID:{1}", ID, Room.RoomID);
        }

        public virtual void OnExitRoom()
        {
            if (Room != null)
            {
                Log.Information("OnExitRoom ID:{0} RoomID:{1}", ID, Room.RoomID);
                Room = null;
            }
        }

        public void SendResponse(OperationCode operationCode, ReturnCode returnCode, byte[]? data, DeliveryMethod deliveryMethod)
        {
            if (Peer != null)
            {
                NetDataWriter netDataWriter = new NetDataWriter();

                netDataWriter.Put((ushort)operationCode);
                netDataWriter.Put((ushort)(returnCode));

                if (data != null)
                    netDataWriter.Put(data);

                Peer.Send(netDataWriter, deliveryMethod);
            }
        }
    }
}