using GameServer.Protocol;
using GameServer.Utils;
using System.Numerics;

namespace GameServer
{
    public class TestRobot : Player
    {
        private float speed = 10;
        private Vector3 position;
        private Vector3 direction;
        private float randomMoveTimer;
        private float syncTimer;

        public override void OnJoinRoom(IRoom room)
        {
            base.OnJoinRoom(room);
            InitPos();
        }
      
        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            RandomMove(deltaTime);
        }

        private void InitPos()
        {
            System.Random random = new System.Random();
            Vector3 randPos = new Vector3(random.Next(-10, 10), 0, random.Next(-10, 10));
            position = randPos;

            speed = random.Next(5, 20);

            direction = Vector3.UnitZ;

            Quaternion quaternion = Quaternion.Identity;

            speed = random.Next(1, 10);

            //SyncTransfrom();
        }

        private void RandomMove(float deltaTime)
        {
            randomMoveTimer -= deltaTime;
            if (randomMoveTimer <= 0)
            {
                System.Random random = new System.Random();

                Vector3 randPos = new Vector3(random.Next(-10, 10), 0, random.Next(-10, 10));

                direction = Vector3.Normalize(randPos - position);
                randomMoveTimer = random.Next(1, 10);
            }

            position += direction * speed * deltaTime;
            SyncTransfrom(deltaTime);
        }

        private void SyncTransfrom(float deltaTime)
        {
            syncTimer += deltaTime;

            if (syncTimer < 0.1f)
                return;

            SyncTransformData syncTransformData = new SyncTransformData();
            syncTransformData.Position = new UnityEngine.Vector3(position.X, position.Y, position.Z);
            syncTransformData.Direction = new UnityEngine.Vector3(direction.X, direction.Y, direction.Z);
            syncTransformData.Scale = new UnityEngine.Vector3(1, 1, 1);
            syncTransformData.Speed = speed;

            byte[] data = MessagePack.MessagePackSerializer.Serialize(syncTransformData);

            if (Room != null)
            {

                foreach (var item in Room.Players)
                {
                    if(item.Value.NetPeer!=null)
                    {
                        item.Value.NetPeer.SendSyncEvent(ID, SyncEventCode.SyncTransform, data, LiteNetLib.DeliveryMethod.Sequenced);
                    }
                }
            }

            syncTimer = 0;
        }
    }
}