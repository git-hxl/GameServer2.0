using GameServer.Protocol;
using LiteNetLib;
using Utils;

namespace GameServer
{
    public interface IPlayer : IReference
    {
        int ID { get; }

        NetPeer? NetPeer { get; }
        int AreaID { get; }

        IRoom? Room { get; }

        PlayerInfo? PlayerInfo { get; }

        void OnInit(int id, NetPeer? netPeer);

        void OnJoinRoom(IRoom room);
        void OnLeaveRoom();
        void OnUpdatePlayerInfo(PlayerInfo info);
        void OnUpdate(float deltaTime);
    }
}
