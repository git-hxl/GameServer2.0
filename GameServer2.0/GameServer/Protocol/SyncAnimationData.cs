
using MessagePack;
using UnityEngine;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncAnimationData
    {
        public string Animation { get; set; }
        public float NormalTime { get; set; }
        public float Speed { get; set; }
    }
}
