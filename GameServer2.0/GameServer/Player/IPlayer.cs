
using Utils;

namespace GameServer
{
    public interface IPlayer : IReference
    {
        int ID { get; }
        IRoom? Room { get; }
        void Init(int id);

        void OnJoinRoom(IRoom room);
        void OnLeaveRoom();

        void OnUpdate();
    }
}
