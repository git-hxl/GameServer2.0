
using MessagePack;
using UnityEngine;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncTransformData
    {
        public int ObjectID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public Vector3 Scale { get; set; }
        public float Speed { get; set; }
    }
}
