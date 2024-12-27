
using LiteNetLib;
using Serilog;

namespace GameServer
{
    internal class Server : Singleton<Server>
    {
        private NetManager? netManager;

        private EventBasedNetListener? listener;

        private ServerConfig? config;

        private OperationHandler? operationHandler;

        public static float DeltaTime { get; set; }

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

            config = serverConfig;
        }


        public void Start()
        {
            if (config == null)
            {
                return;
            }
            if (netManager == null)
            {
                return;
            }

            netManager.Start(config.Port);
            Log.Information("start server:{0}", netManager.LocalPort);
        }

        public void Update(float deltaTime)
        {
            if (netManager != null)
            {
                netManager.PollEvents();
            }

            DeltaTime = deltaTime;
        }

        /// <summary>
        /// 连接请求
        /// </summary>
        /// <param name="request"></param>
        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Log.Information("OnConnectionRequest {0}", request.RemoteEndPoint.ToString());

            if (config == null)
                return;
            request.AcceptIfKey(config.ConnectKey);
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
                //Log.Information("Listener_NetworkReceiveEvent {0}", peer.ToString());

                OperationCode operationCode = (OperationCode)reader.GetUShort();

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

            Player? player = PlayerManager.Instance.GetPlayer(peer);

            if (player != null)
            {
                PlayerManager.Instance.RemovePlayer(player.ID);
            }
        }
    }
}
