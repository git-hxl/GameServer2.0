
using MessagePack;
using UnityEngine;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class TransformData
    {
        public int ObjectID { get; set; }
        public UnityEngine.Vector3 Position { get; set; }
        public UnityEngine.Vector3 EulerAngles { get; set; }
        public UnityEngine.Vector3 Scale { get; set; }
        public float Speed { get; set; }
        public float Time {  get; set; }
    }
}
