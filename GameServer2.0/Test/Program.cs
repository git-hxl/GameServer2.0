using GameServer.Protocol;
using GameServer;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using StackExchange.Redis;
using System.Diagnostics;
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
                switch (info)
                {
                    case "connect":
                        Connect();
                        break;

                    case "disconnect":
                        Disconnect();
                        break;
                    case "joinroom":
                        JoinRoom();
                        break;
                    case "leaveroom":
                        LeaveRoom();
                        break;

                    case "joinrobot":
                        JoinRobot();
                        break;
                }
            }
        }

        private static void Connect()
        {
            Client.Instance.Connect("127.0.0.1", 1111);
        }

        private static void Disconnect()
        {
            Client.Instance.DisConnect();
        }

        private static void JoinRoom()
        {
            JoinRoomRequest joinRoomRequest = new JoinRoomRequest();

            Random rand = new Random();
            joinRoomRequest.PlayerID = 123;
            joinRoomRequest.RoomID = 1;
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

        private static void JoinRobot()
        {
            JoinRoomRequest joinRoomRequest = new JoinRoomRequest();

            Random rand = new Random();
            joinRoomRequest.PlayerID = rand.Next(1, 100);
            joinRoomRequest.RoomID = 1;
            joinRoomRequest.IsRobot = true;
            byte[] data = MessagePack.MessagePackSerializer.Serialize(joinRoomRequest);

            Client.Instance.Send(OperationCode.JoinRoom, data, DeliveryMethod.ReliableOrdered);
        }
    }
}
