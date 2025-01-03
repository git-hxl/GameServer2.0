
using Serilog;

namespace GameServer
{
    public class Player : IPlayer
    {
        public int ID { get; private set; }

        public IRoom? Room { get; private set; }

        public void Init(int id)
        {
            ID = id;
        }

        public void OnAcquire()
        {

        }

        public virtual void OnJoinRoom(IRoom room)
        {
            if (Room != null)
            {
                Room.OnJoinPlayer(this);
            }

            Room = room;

            Log.Information("OnJoinRoom PlayerID {0} RoomID {1}", ID, Room.ID);
        }

        public virtual void OnLeaveRoom()
        {
            if (Room != null)
            {
                Log.Information("OnLeaveRoom PlayerID {0} RoomID {1}", ID, Room.ID);

                Room.OnLeavePlayer(this);
                Room = null;
            }
        }

        public virtual void OnUpdate()
        {

        }

        public void OnRelease()
        {
            ID = -1;
            Room = null;
        }
    }
}
