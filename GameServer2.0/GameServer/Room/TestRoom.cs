

using GameServer.Protocol;
using Utils;

namespace GameServer
{
    public class TestRoom : BaseRoom
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
                int id = i - 9999;
                TestRobot? robot = PlayerManager.Instance.CreatePlayer<TestRobot>(id, null);
                UserInfo userInfo = new UserInfo();
                userInfo.IsRobot = true;
                robot.OnUpdatePlayerInfo(userInfo);
               
                OnJoinPlayer(robot);
            }
        }

    }
}
