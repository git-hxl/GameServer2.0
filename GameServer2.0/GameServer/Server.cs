using GameServer.Protocol;
using LiteNetLib;
using MessagePack;
using Serilog;
using System.Diagnostics;
using Utils;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace GameServer
{
    internal class Server : Singleton<Server>
    {
        private NetManager? netManager;

        private EventBasedNetListener? listener;

        private OperationHandler? operationHandler;

        public ServerConfig? Config { get; private set; }

        public static int UpdateInterval { get; private set; }
        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();

            listener = new EventBasedNetListener();

            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent; ;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent; ;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent; ;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent; ;

            netManager = new NetManager(listener);

            operationHandler = new OperationHandler();
        }

        public void InitConfig(ServerConfig serverConfig)
        {
            if (netManager == null)
            {
                return;
            }
            netManager.PingInterval = serverConfig.PingInterval;
            netManager.DisconnectTimeout = serverConfig.DisconnectTimeout;
            netManager.ReconnectDelay = serverConfig.ReconnectDelay;
            netManager.MaxConnectAttempts = serverConfig.MaxConnectAttempts;

            Config = serverConfig;

            UpdateInterval = serverConfig.UpdateInterval;
        }


        public void Start()
        {
            if (Config == null)
            {
                return;
            }
            if (netManager == null)
            {
                return;
            }

            netManager.Start(Config.Port);
            Log.Information("start server:{0}", netManager.LocalPort);

            RoomManager.Instance.CreateRoom<TestRoom>(-1);
            RoomManager.Instance.CreateRoom<BaseRoom>(1);

            DebugStatisticsInfo();
        }

        public void Update()
        {
            if (netManager != null)
            {
                netManager.PollEvents();
            }
        }

        /// <summary>
        /// 连接请求
        /// </summary>
        /// <param name="request"></param>
        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Log.Information("OnConnectionRequest {0}", request.RemoteEndPoint.ToString());

            if (Config == null)
                return;
            request.AcceptIfKey(Config.ConnectKey);
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="peer"></param>
        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Log.Information("OnPeerConnected {0}", peer.ToString());
        }


        /// <summary>
        /// 接受消息
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="reader"></param>
        /// <param name="channel"></param>
        /// <param name="deliveryMethod"></param>
        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            if (operationHandler == null)
                return;
            try
            {
                OperationCode operationCode = (OperationCode)reader.GetUShort();

               // Log.Information("接收请求：{0}  ping：{1}", operationCode, peer.Ping);

                operationHandler.OnRequest(peer, operationCode, reader.GetRemainingBytes(), deliveryMethod);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="disconnectInfo"></param>
        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.Information("OnPeerDisconnected {0}", peer.ToString());
        }

        //输出统计信息
        private void DebugStatisticsInfo()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            Task.Run(async() =>
            {
                while (true)
                {
                    await Task.Delay(5000);
                    float cpuUsage = cpuCounter.NextValue();
                    float availableMemory = ramCounter.NextValue();

                   // var o = new { RoomCount = RoomManager.Instance.Rooms.Count, PlayerCount = PlayerManager.Instance.Players.Count, Info = $"CPU: {cpuUsage.ToString("F1")}% Mem: {availableMemory.ToString("F1")}%" };

                    Log.Information("RoomCount: {0} PlayerCount：{1} CPU：{2:F1}% Mem：{3:F1}%", RoomManager.Instance.Rooms.Count, PlayerManager.Instance.Players.Count, cpuUsage, availableMemory);
                }
            });
        }
    }
}
