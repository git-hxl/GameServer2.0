
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public interface IRoom : IReference
    {
        int ID { get; }


        ConcurrentDictionary<int, IPlayer> Players { get; }

        void Init(int id);

        void OnJoinPlayer(IPlayer player);

        void OnLeavePlayer(IPlayer player);

        void OnCloseRoom();

        void Update();
    }
}
