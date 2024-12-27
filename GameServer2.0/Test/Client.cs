using GameServer.Protocol;
using GameServer;
using LiteNetLib.Utils;
using LiteNetLib;
using MessagePack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class Client : Singleton<Client>
    {
        private LiteNetLib.NetManager netManager;
        private EventBasedNetListener listener;
        private NetPeer server;
        private void StartClient()
        {
            listener = new EventBasedNetListener();

            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;

            netManager = new LiteNetLib.NetManager(listener);
        }


        public void Connect(string ip, int port)
        {
            netManager.Start();

            server = netManager.Connect(ip, port, "qwer123456");
        }

        public void DisConnect()
        {
            if (server != null)
            {
                server.Disconnect();
            }
            server = null;
        }

        public void Send(OperationCode code, byte[] data, DeliveryMethod delivery)
        {
            if (server == null)
            {
                return;
            }
            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put((ushort)code);
            if (data != null)
            {
                netDataWriter.Put(data);
            }
            server.Send(netDataWriter, delivery);
        }

        public void Update()
        {
            if (netManager != null)
            {
                netManager.PollEvents();
            }
        }

        private void OnDestroy()
        {
            if (netManager != null)
                netManager.Stop();
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            OperationCode operationCode = (OperationCode)reader.GetUShort();
            ReturnCode returnCode = (ReturnCode)reader.GetUShort();

            byte[] data = reader.GetRemainingBytes();

            Log.Information(string.Format("Listener_NetworkReceiveEvent {0} {1} {2}", peer.Address.ToString(), operationCode, returnCode));

            if (returnCode != ReturnCode.Success)
            {
                return;
            }

            switch (operationCode)
            {
                case OperationCode.Login:
                    break;
                case OperationCode.Register:
                    break;
                case OperationCode.JoinRoom:
                    break;
                case OperationCode.OnJoinRoom:
                    OnJoinRoom(data, deliveryMethod);

                    break;
                case OperationCode.OnOtherJoinRoom:
                    OnOtherJoinRoom(data, deliveryMethod);
                    break;
                case OperationCode.LeaveRoom:
                    break;
                case OperationCode.OnLeaveRoom:
                    OnLeaveRoom(data, deliveryMethod);
                    break;
                case OperationCode.OnOtherLeaveRoom:
                    OnOtherLeaveRoom(data, deliveryMethod);
                    break;
                case OperationCode.SyncEvent:
                    break;
                default:
                    break;
            }
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Log.Information(string.Format("Listener_PeerDisconnectedEvent {0}", peer.Address.ToString()));
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Log.Information(string.Format("Listener_PeerConnectedEvent {0}", peer.Address.ToString()));
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Log.Information(string.Format("Listener_ConnectionRequestEvent {0} {1}", request.RemoteEndPoint.ToString(), request.Data.GetString()));
        }

        private void OnJoinRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomResponse response = MessagePackSerializer.Deserialize<JoinRoomResponse>(data);
        }

        private void OnOtherJoinRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            PlayerInfoInRoom playerInfoInRoom = MessagePackSerializer.Deserialize<PlayerInfoInRoom>(data);
        }

        private void OnLeaveRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
        }

        private void OnOtherLeaveRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            PlayerInfoInRoom playerInfoInRoom = MessagePackSerializer.Deserialize<PlayerInfoInRoom>(data);
        }

        private void OnSyncEvent(byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncEventRequest syncEventData = MessagePackSerializer.Deserialize<SyncEventRequest>(data);
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();

            StartClient();
        }

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }
    }
}
