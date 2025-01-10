
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncAnimationData
    {
        public int ObjectID { get; set; }
        public int StateHash { get; set; }
        public int LayerID { get; set; }
        public float Weight { get; set; }
        public float NormalizedTimeTime { get; set; }
        public float Speed { get; set; }

        public Dictionary<int, int> IntParams { get; set; }
        public Dictionary<int, float> FloatParams { get; set; }
        public Dictionary<int, bool> BoolParams { get; set; }

    }
}
