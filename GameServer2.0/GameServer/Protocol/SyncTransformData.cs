
using MessagePack;
using UnityEngine;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncTransformData
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public float Speed { get; set; }
    }
}
