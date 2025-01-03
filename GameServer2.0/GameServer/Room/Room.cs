
using Serilog;
using System.Collections.Concurrent;

namespace GameServer
{
    public class Room : IRoom
    {
        public int ID { get; private set; }

        public ConcurrentDictionary<int, IPlayer> Players { get; private set; } = new ConcurrentDictionary<int, IPlayer>();

        public void Init(int id)
        {
            ID = id;
        }

        public void OnAcquire()
        {

        }

        public void OnCloseRoom()
        {
            Log.Information("OnCloseRoom  RoomID {0}", ID);

            foreach (var item in Players)
            {
                item.Value.OnLeaveRoom();
            }

            Players.Clear();

            ID = -1;
        }

        public void OnJoinPlayer(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public void OnLeavePlayer(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public void OnRelease()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
