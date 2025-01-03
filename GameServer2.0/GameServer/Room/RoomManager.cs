

using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class RoomManager : Singleton<RoomManager>
    {
        private ConcurrentDictionary<int, OLDRoom> rooms = new ConcurrentDictionary<int, OLDRoom>();

        protected override void OnInit()
        {
            //throw new NotImplementedException();

            GetOrCreateRoom(1);

            GetOrCreateRoom(2);

            Task.Run(async () =>
            {
                await Task.Delay(1000);

                CloseRoom(1);
            });
        }

        public OLDRoom GetOrCreateRoom(int roomID)
        {
            OLDRoom? room = GetRoom(roomID);

            if (room == null || room.Inactived)
            {
                switch (roomID)
                {
                    case 1:
                        room = ReferencePool.Instance.Acquire<TestRoom>();

                        break;
                    default:

                        room = ReferencePool.Instance.Acquire<OLDRoom>();

                        break;
                }

                room.Init(roomID);
                room.OnCreateRoom();
                rooms.TryAdd(roomID, room);
            }
            return room;
        }

        public OLDRoom? GetRoom(int roomID)
        {
            rooms.TryGetValue(roomID, out OLDRoom? room);
            return room;
        }

        public void CloseRoom(int roomID)
        {
            OLDRoom? room;
            if (rooms.TryRemove(roomID, out room))
            {
                room.OnCloseRoom();
                ReferencePool.Instance.Release(room);
            }
        }

        protected override void OnDispose()
        {
            //throw new NotImplementedException();

            foreach (var item in rooms)
            {
                OLDRoom room = item.Value;
                if (room != null)
                {
                    room.OnCloseRoom();
                    ReferencePool.Instance.Release(room);
                }
            }

            rooms.Clear();
        }

        public void Update()
        {
            var inactiveRooms = new List<int>();

            foreach (var item in rooms)
            {
                if (item.Value.Inactived)
                {
                    inactiveRooms.Add(item.Key);
                }
            }

            for (int i = 0; i < inactiveRooms.Count; i++)
            {
                CloseRoom(inactiveRooms[i]);
            }
        }
    }
}
