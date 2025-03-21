
using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class RoomManager : Singleton<RoomManager>
    {
        public ConcurrentDictionary<int, BaseRoom> Rooms { get; } = new ConcurrentDictionary<int, BaseRoom>();

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
            BaseRoom? room;
            if (Rooms.TryRemove(roomId, out room))
            {
                room.OnCloseRoom();
                ReferencePool.Instance.Release(room);
            }
        }

        public T? CreateRoom<T>(int roomId) where T : BaseRoom, new()
        {
            if (GetRoom<T>(roomId) != null)
            {
                Log.Information("CreateRoom Failed,RoomID {0} is existed!", roomId);
                return null;
            }

            var room = ReferencePool.Instance.Acquire<T>();

            if (Rooms.TryAdd(roomId, room))
            {
                room.OnInit(roomId);

                return room as T;
            }

            ReferencePool.Instance.Release(room);

            Log.Information("CreateRoom Failed,RoomID {0} is existed!", roomId);

            return default(T);
        }

        public T? GetRoom<T>(int roomId) where T : BaseRoom
        {
            BaseRoom? room;
            if (Rooms.TryGetValue(roomId, out room))
            {
                return room as T;
            }
            return null;
        }
    }
}
