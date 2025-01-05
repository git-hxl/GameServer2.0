

using Utils;

namespace GameServer
{
    public class TestRoom : Room
    {
        public override void OnInit(int id)
        {
            base.OnInit(id);

            AddRobot(100);
        }

        public void AddRobot(int count)
        {
            for (int i = 0; i < count; i++)
            {
                TestRobot? robot = PlayerManager.Instance.CreatePlayer<TestRobot>(i - 9999, null);
                robot.OnJoinRoom(this);
            }
        }

    }
}
