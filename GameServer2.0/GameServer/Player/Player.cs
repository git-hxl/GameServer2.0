using LiteNetLib;
using Serilog;
using Utils;

namespace GameServer
{
    public class Player : IReference
    {
        public int ID { get; private set; }

        public NetPeer? NetPeer { get; private set; }

        public Room? Room { get; private set; }

        public void OnAcquire()
        {
            //throw new NotImplementedException();
        }

        public void Init(int id, NetPeer netPeer)
        {
            ID = id;
            NetPeer = netPeer;
        }

        public void OnRelease()
        {
            //throw new NotImplementedException();

            ID = -1;
            NetPeer = null;
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
    }
}