using GameServer.Protocol;
using LiteNetLib;
using MessagePack;
using Serilog;
using System.Diagnostics;
using Utils;

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
            RoomManager.Instance.CreateRoom<Room>(1);
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
                Request request = MessagePackSerializer.Deserialize<Request>(reader.GetRemainingBytes());

                Log.Information("接收请求：{0} 延迟：{1} ping：{2}", request.OperationCode, DateTimeUtil.TimeStamp - request.Timestamp, peer.Ping);

                operationHandler.OnRequest(peer, request, deliveryMethod);
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

            var player = PlayerManager.Instance.GetPlayer(peer);

            if (player != null)
            {
                PlayerManager.Instance.RemovePlayer(player.ID);
            }
        }
    }
}
