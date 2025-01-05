
using GameServer.Protocol;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public interface IRoom : IReference
    {
        int ID { get; }


        int MasterID { get; }

        RoomInfo? RoomInfo { get; }

        ConcurrentDictionary<int, IPlayer> Players { get; }

        void OnInit(int id);

        void OnJoinPlayer(IPlayer player);

        void OnLeavePlayer(IPlayer player);

        void OnCloseRoom();

        void OnUpdateMaster(int master);

        void OnUpdateRoomInfo(RoomInfo roomInfo);

        void OnUpdate();
    }
}
