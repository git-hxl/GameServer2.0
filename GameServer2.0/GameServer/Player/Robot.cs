

using GameServer.Protocol;
using GameServer.Utils;
using MessagePack;
using System;
using System.Numerics;

namespace GameServer
{
    public class Robot : Player
    {
        public float Speed = 10;
        public Vector3 Position;
        public Quaternion Rotation;

        private float RandomMoveTimer;

        public override void OnJoinRoom(Room room)
        {
            base.OnJoinRoom(room);

            InitPos();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            RandomMove();
        }

        private void InitPos()
        {
            Random random = new Random();
            Vector3 randPos = new Vector3(random.Next(-10, 10), 0, random.Next(-10, 10));
            Position = randPos;

            Quaternion quaternion = Quaternion.Identity;

            Speed = 10;

            //SyncTransfrom();
        }

        private void RandomMove()
        {
            if (RandomMoveTimer <= 0)
            {
                Random random = new Random();
                Vector3 randPos = new Vector3(random.Next(-10, 10), 0, random.Next(-10, 10));

                Vector3 direction = Vector3.Normalize(randPos - Position);

                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.Atan2(direction.X, direction.Z));

                RandomMoveTimer = 10;
            }

            RandomMoveTimer -= Server.DeltaTime;

            Vector3 vectorFromQuaternion = Vector3.Transform(Vector3.UnitZ, Rotation);
            Position += vectorFromQuaternion * Speed * Server.DeltaTime;

            SyncTransfrom();
        }

        private void SyncTransfrom()
        {
            SyncEventRequest syncEventRequest = new SyncEventRequest();
            syncEventRequest.PlayerID = ID;
            syncEventRequest.EventID = (ushort)EventCode.SyncTransform;

            SyncTransformData syncTransformData = new SyncTransformData();
            syncTransformData.Position = new UnityEngine.Vector3(Position.X, Position.Y, Position.Z);
            syncTransformData.Rotation = new UnityEngine.Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);
            syncTransformData.Speed = Speed;

            syncEventRequest.SyncData = MessagePack.MessagePackSerializer.Serialize(syncTransformData);

            byte[] data = MessagePackSerializer.Serialize(syncEventRequest);

            if (Room != null)
            {
                foreach (var item in Room.Players)
                {
                    Player player = item.Value;
                    if (player.NetPeer != null)
                    {
                        player.NetPeer.SendResponse(OperationCode.SyncEvent, ReturnCode.Success, data, LiteNetLib.DeliveryMethod.Sequenced);
                    }
                }
            }

        }
    }
}
