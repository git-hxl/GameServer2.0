
using MessagePack;
using UnityEngine;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class ObjectData
    {
        public int ObjectID { get; set; }
        public string AssetPath { get; set; }
      
    }
}
