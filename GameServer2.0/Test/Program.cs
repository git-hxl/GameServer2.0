using GameServer.Protocol;
using GameServer;
using LiteNetLib;

using Serilog;


namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration.WriteTo.File("./Log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true).WriteTo.Console();
            loggerConfiguration.MinimumLevel.Debug();
            Log.Logger = loggerConfiguration.CreateLogger();



            while (true)
            {
                Thread.Sleep(100);

                Client.Instance.Update();

                string info = Console.ReadLine();
                info = info.ToLower();

                string key = info.Split(' ')[0];

                switch (key)
                {
                    case "connect":
                        Test();
                        break;

                    case "disconnect":
                        Disconnect();
                        break;

                    case "createroom":
                        if (info.Contains(" "))
                        {
                            int value = int.Parse(info.Split(' ')[1]);

                            CreateRoom(value);
                        }
                        break;

                    case "closeroom":
                        if (info.Contains(" "))
                        {
                            int value = int.Parse(info.Split(' ')[1]);

                            CloseRoom(value);
                        }
                        break;

                    case "joinroom":

                        if (info.Contains(" "))
                        {
                            int value = int.Parse(info.Split(' ')[1]);

                            JoinRoom(value);
                        }

                        break;

                    case "leaveroom":

                        LeaveRoom(11);
                        break;

                    case "joinrobot":

                        if (info.Contains(" "))
                        {
                            int value = int.Parse(info.Split(' ')[1]);

                            JoinRobot(value);
                        }

                        break;
                }
            }
        }

        private static async Task Test()
        {

            Random rand = new Random();
            while (true)
            {
                int roomid = rand.Next(int.MinValue, int.MaxValue);
                for (int i = 0; i < 1000; i++)
                {
                    OperationCode item = (OperationCode)Enum.GetValues(typeof(OperationCode)).GetValue(rand.Next(0, 10));

                    switch (item)
                    {
                        case OperationCode.JoinRoom:
                            JoinRoom(roomid);
                            break;

                        case OperationCode.LeaveRoom:

                            LeaveRoom(roomid);
                            break;

                        case OperationCode.Disconnect:

                            Disconnect();
                            break;
                        case OperationCode.Login:

                            Connect();
                            break;

                        case OperationCode.CreateRoom:


                            CreateRoom(roomid);

                            break;
                    }
                }

                await Task.Delay(100);
            }
        }

        private static void Connect()
        {
            Client.Instance.Connect("127.0.0.1", 8888);

            LoginRequest loginRequest = new LoginRequest();


            Client.Instance.SendRequest(OperationCode.Login, loginRequest, DeliveryMethod.ReliableOrdered);
        }

        private static void Disconnect()
        {
            Client.Instance.SendRequest(OperationCode.Disconnect, null, DeliveryMethod.ReliableOrdered);
            // Client.Instance.DisConnect();
        }

        private static void CreateRoom(int id)
        {
            Random rand = new Random();
            CreateRoomRequest roomRequest = new CreateRoomRequest();
            roomRequest.RoomID = id;
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.RoomID = id;

            roomInfo.RoomName = "Test";

            roomRequest.RoomInfo = roomInfo;

            byte[] data = MessagePack.MessagePackSerializer.Serialize(roomRequest);

            Client.Instance.SendRequest(OperationCode.CreateRoom, roomRequest, DeliveryMethod.ReliableOrdered);

        }

        private static void CloseRoom(int id)
        {
            CloseRoomRequest roomRequest = new CloseRoomRequest();
            roomRequest.RoomID = id;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(roomRequest);

            Client.Instance.SendRequest(OperationCode.CloseRoom, roomRequest, DeliveryMethod.ReliableOrdered);
        }

        private static void JoinRoom(int id)
        {

            JoinRoomRequest joinRoomRequest = new JoinRoomRequest();

            joinRoomRequest.RoomID = id;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(joinRoomRequest);

            Client.Instance.SendRequest(OperationCode.JoinRoom, joinRoomRequest, DeliveryMethod.ReliableOrdered);
        }

        private static void LeaveRoom(int id)
        {
            LeaveRoomRequest leaveRoomRequest = new LeaveRoomRequest();
            leaveRoomRequest.RoomID = id;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(leaveRoomRequest);

            Client.Instance.SendRequest(OperationCode.LeaveRoom, leaveRoomRequest, DeliveryMethod.ReliableOrdered);
        }

        private static void JoinRobot(int id)
        {
            JoinRoomRequest joinRoomRequest = new JoinRoomRequest();

            Random rand = new Random();
            joinRoomRequest.UserID = rand.Next(1, 100);
            joinRoomRequest.RoomID = id;
            UserInfo playerInfo = new UserInfo();
            playerInfo.IsRobot = true;
            joinRoomRequest.UserInfo = playerInfo;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(joinRoomRequest);

            Client.Instance.SendRequest(OperationCode.JoinRoom, joinRoomRequest, DeliveryMethod.ReliableOrdered);
        }
    }
}
