
using GameServer.Protocol;
using LiteNetLib;
using Serilog;
using UnityEngine;

namespace GameServer
{
    public class BasePlayer : IPlayer
    {
        public int ID { get; private set; }

        public NetPeer? NetPeer { get; private set; }

        public int AreaID { get; private set; }

        public IRoom? Room { get; private set; }

        public PlayerInfo? PlayerInfo { get; private set; }

        public void OnInit(int id, NetPeer? netPeer)
        {
            ID = id;

            NetPeer = netPeer;
        }

        public void OnAcquire()
        {

        }

        public virtual void OnJoinRoom(IRoom room)
        {
            if (Room != null)
            {
                Room.OnLeavePlayer(this);
            }

            Room = room;

            Log.Information("OnJoinRoom PlayerID {0} RoomID {1}", ID, Room.ID);
        }

        public virtual void OnLeaveRoom()
        {
            if (Room != null)
            {
                Log.Information("OnLeaveRoom PlayerID {0} RoomID {1}", ID, Room.ID);
                Room = null;
            }
        }

        public virtual void OnUpdatePlayerInfo(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        public virtual void OnUpdateArea(Vector3 pos)
        {
            AreaID = (int)pos.x + (int)pos.y + (int)pos.z;
        }

        /// <summary>
        /// 每帧调用
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {

        }

        public void OnRelease()
        {
            ID = -1;
            Room = null;

            NetPeer = null;
        }
    }
}
