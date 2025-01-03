
using LiteNetLib;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class OLDPlayerManager : Singleton<OLDPlayerManager>
    {
        public ConcurrentDictionary<int, OLDPlayer> Players { get; } = new ConcurrentDictionary<int, OLDPlayer>();

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
            foreach (var item in Players)
            {
                ReferencePool.Instance.Release(item.Value);
            }

            Players.Clear();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }

        

        public OLDPlayer GetOrCreatePlayer(int id, NetPeer netPeer)
        {
            OLDPlayer? player = GetPlayer(id);
            if (player == null)
            {
                player = ReferencePool.Instance.Acquire<OLDPlayer>();

                player.Init(id, netPeer);

                Players.TryAdd(id, player);
            }

            return player;
        }

        public TestRobot GetOrCreateRobot(int id)
        {
            TestRobot? robot = GetPlayer(id) as TestRobot;
            if (robot == null)
            {
                robot = ReferencePool.Instance.Acquire<TestRobot>();

                robot.Init(id, null);

                Players.TryAdd(id, robot);
            }

            return robot;
        }


        public OLDPlayer? GetPlayer(int id)
        {
            OLDPlayer? player;
            Players.TryGetValue(id, out player);
            return player;
        }

        public OLDPlayer? GetPlayer(NetPeer netPeer)
        {
            return Players.Values.FirstOrDefault((a) => a.NetPeer == netPeer);
        }

        public void RemovePlayer(int id)
        {
            if (Players.TryRemove(id, out OLDPlayer? player))
            {
                OLDRoom? room = player.Room;
                if (room != null)
                {
                    room.RemovePlayer(id);
                }

                ReferencePool.Instance.Release(player);
            }
        }
    }
}