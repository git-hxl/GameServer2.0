
using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class RoomManager : Singleton<RoomManager>, IRoomManager
    {
        public ConcurrentDictionary<int, IRoom> Rooms { get; } = new ConcurrentDictionary<int, IRoom>();

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }

        public void CloseRoom(int roomId)
        {
            IRoom? room;
            if (Rooms.TryRemove(roomId, out room))
            {
                room.OnCloseRoom();

                ReferencePool.Instance.Release(room);
            }
        }

        public T? CreateRoom<T>(int roomId) where T : IRoom, new()
        {
            IRoom room = ReferencePool.Instance.Acquire<T>();

            if (Rooms.TryAdd(roomId, room))
            {
                room.OnInit(roomId);

                return (T)room;
            }

            ReferencePool.Instance.Release(room);

            Log.Information("CreateRoom Failed,RoomID {0} is existed!", roomId);

            return default(T);
        }

        public IRoom? GetRoom(int roomId)
        {
            IRoom? room;
            if (Rooms.TryGetValue(roomId, out room))
            {
                return room;
            }
            return null;
        }
    }
}
