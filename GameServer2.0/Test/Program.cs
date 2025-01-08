using GameServer.Protocol;
using GameServer;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using StackExchange.Redis;
using System.Diagnostics;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                        Connect();
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

                        LeaveRoom();
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

        private static void Connect()
        {
            Client.Instance.Connect("127.0.0.1", 8888);
        }

        private static void Disconnect()
        {
            Client.Instance.Send(OperationCode.Disconnect, null, DeliveryMethod.ReliableOrdered);
            // Client.Instance.DisConnect();
        }

        private static void CreateRoom(int id)
        {
            Random rand = new Random();
            CreateRoomRequest roomRequest = new CreateRoomRequest();
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.RoomID = id;

            roomInfo.RoomName = "Test";

            roomRequest.RoomInfo = roomInfo;

            byte[] data = MessagePack.MessagePackSerializer.Serialize(roomRequest);

            Client.Instance.Send(OperationCode.CreateRoom, data, DeliveryMethod.ReliableOrdered);

        }

        private static void CloseRoom(int id)
        {
            CloseRoomRequest roomRequest = new CloseRoomRequest();
            roomRequest.RoomID = id;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(roomRequest);

            Client.Instance.Send(OperationCode.CloseRoom, data, DeliveryMethod.ReliableOrdered);
        }

        private static void JoinRoom(int id)
        {
            Random rand = new Random();
            JoinRoomRequest joinRoomRequest = new JoinRoomRequest();


            joinRoomRequest.PlayerID = 123;
            joinRoomRequest.RoomID = id;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(joinRoomRequest);

            Client.Instance.Send(OperationCode.JoinRoom, data, DeliveryMethod.ReliableOrdered);
        }

        private static void LeaveRoom()
        {
            LeaveRoomRequest leaveRoomRequest = new LeaveRoomRequest();
            leaveRoomRequest.PlayerID = 123;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(leaveRoomRequest);

            Client.Instance.Send(OperationCode.LeaveRoom, data, DeliveryMethod.ReliableOrdered);
        }

        private static void JoinRobot(int id)
        {
            JoinRoomRequest joinRoomRequest = new JoinRoomRequest();

            Random rand = new Random();
            joinRoomRequest.PlayerID = rand.Next(1, 100);
            joinRoomRequest.RoomID = id;
            PlayerInfo playerInfo = new PlayerInfo();
            playerInfo.IsRobot = true;
            joinRoomRequest.PlayeInfo = playerInfo;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(joinRoomRequest);

            Client.Instance.Send(OperationCode.JoinRoom, data, DeliveryMethod.ReliableOrdered);
        }
    }
}
