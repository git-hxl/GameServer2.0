﻿using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncEventRequest
    {
        public int PlayerID { get; set; }
        public ushort EventID { get; set; }
        public byte[] SyncData { get; set; }
    }
}