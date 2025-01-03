﻿using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
using Serilog;
using System.Numerics;
using Utils;


namespace GameServer
{
    public class OLDPlayer : IReference
    {
        public int ID { get; protected set; }

        public NetPeer? NetPeer { get; protected set; }

        public OLDRoom? Room { get; protected set; }

        public Vector3 Position { get; protected set; }

        public void OnAcquire()
        {
            //throw new NotImplementedException();
        }

        public virtual void Init(int id, NetPeer? netPeer)
        {
            ID = id;
            NetPeer = netPeer;
        }

        public void OnRelease()
        {
            //throw new NotImplementedException();

            ID = -1;
            NetPeer = null;
        }

        public virtual void OnUpdate()
        {

        }


        public virtual void OnJoinRoom(OLDRoom room)
        {
            if (Room != null)
            {
                if (Room != room)
                {
                    Room.RemovePlayer(ID);
                }
            }
            Room = room;

            Log.Information("OnJoinRoom ID:{0} RoomID:{1}", ID, Room.RoomID);
        }

        public virtual void OnExitRoom()
        {
            if (Room != null)
            {
                Log.Information("OnExitRoom ID:{0} RoomID:{1}", ID, Room.RoomID);
                Room = null;
            }
        }
    }
}