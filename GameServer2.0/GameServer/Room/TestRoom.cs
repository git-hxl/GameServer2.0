

using Utils;

namespace GameServer
{
    public class TestRoom : Room
    {
        public override void Init(int roomID)
        {
            base.Init(roomID);



        }

        public override void OnCreateRoom()
        {
            base.OnCreateRoom();
            AddRobot(100);
        }


        public void AddRobot(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Robot robot = PlayerManager.Instance.GetOrCreateRobot(i - 9999);
                AddPlayer(robot);
            }
        }

    }
}
