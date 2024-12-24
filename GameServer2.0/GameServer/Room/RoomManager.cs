

using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class RoomManager : Singleton<RoomManager>
    {
        private ConcurrentDictionary<int, Room> rooms = new ConcurrentDictionary<int, Room>();

        private CancellationTokenSource? updateTokenSource;
        protected override void OnInit()
        {
            //throw new NotImplementedException();
            Update();
        }

        public Room GetOrCreateRoom(int roomID)
        {
            Room? room = GetRoom(roomID);

            if (room == null)
            {
                room = ReferencePool.Instance.Acquire<Room>();
                room.Init(roomID);
                rooms.TryAdd(roomID, room);
            }
            return room;
        }

        public Room? GetRoom(int roomID)
        {
            rooms.TryGetValue(roomID, out Room? room);
            return room;
        }

        public void CloseRoom(int roomID)
        {
            Room? room;
            if (rooms.TryRemove(roomID, out room))
            {
                room.OnCloseRoom();
                ReferencePool.Instance.Release(room);
            }
        }

        public async void Update()
        {
            try
            {
                updateTokenSource = new CancellationTokenSource();
                List<int> clearRoomIDs = new List<int>();
                while (true)
                {
                    await Task.Delay((int)(Server.DeltaTime * 1000), updateTokenSource.Token);

                    foreach (var item in rooms)
                    {
                        Room room = item.Value;
                        if (room == null || room.Inactived)
                        {
                            clearRoomIDs.Add(item.Key);
                        }
                        else
                        {
                            room.Update();
                        }
                    }

                    foreach (var item in clearRoomIDs)
                    {
                        CloseRoom(item);
                    }

                    clearRoomIDs.Clear();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        protected override void OnDispose()
        {
            //throw new NotImplementedException();

            foreach (var item in rooms)
            {
                Room room = item.Value;
                if (room != null)
                {
                    room.OnCloseRoom();
                    ReferencePool.Instance.Release(room);
                }
            }

            rooms.Clear();
        }
    }
}
