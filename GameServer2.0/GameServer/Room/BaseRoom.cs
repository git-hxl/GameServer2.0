
using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
using Serilog;
using System.Collections.Concurrent;

namespace GameServer
{
    public class BaseRoom : IRoom
    {
        public int ID { get; private set; }

        public int MasterID { get; private set; }

        public RoomInfo? RoomInfo { get; private set; }
        public ConcurrentDictionary<int, IPlayer> Players { get; private set; } = new ConcurrentDictionary<int, IPlayer>();

        private CancellationTokenSource? _cancellationTokenSource;

        public DateTime CreateTime { get; private set; }

        public long InactiveTime { get; private set; }
        public virtual void OnInit(int id)
        {
            ID = id;

            MasterID = -1;

            CreateTime = DateTime.Now;

            Task.Run(Update);

            Log.Information("OnCreateRoom RoomID {0} ", id);
        }

        public void OnAcquire()
        {

        }

        public virtual void OnRelease()
        {
            // throw new NotImplementedException();
        }


        public virtual void OnJoinPlayer(IPlayer player)
        {
            if (Players.TryAdd(player.ID, player))
            {
                player.OnJoinRoom(this);

                if (MasterID == -1)
                {
                    OnUpdateMaster(player.ID);
                }

                JoinRoomResponse joinRoomResponse = new JoinRoomResponse();

                joinRoomResponse.RoomID = ID;

                joinRoomResponse.PlayerID = player.ID;

                joinRoomResponse.RoomInfo = RoomInfo;

                joinRoomResponse.PlayerInfos = Players.Values.Select((a) => a.PlayerInfo).ToList();

                byte[] data = MessagePackSerializer.Serialize(joinRoomResponse);

                foreach (var item in Players)
                {
                    if (item.Value.NetPeer != null)
                    {
                        item.Value.NetPeer.SendResponse(OperationCode.JoinRoom, ReturnCode.Success, "", data, LiteNetLib.DeliveryMethod.ReliableOrdered);
                    }
                }
            }



        }

        public virtual void OnLeavePlayer(IPlayer player)
        {
            if (Players.TryRemove(player.ID, out _))
            {
                player.OnLeaveRoom();

                if (MasterID == player.ID)
                {
                    var master = Players.Values.FirstOrDefault((a) => a.NetPeer != null);
                    if (master != null)
                    {
                        OnUpdateMaster(master.ID);
                    }
                }

                LeaveRoomResponse leaveRoomResponse = new LeaveRoomResponse();

                leaveRoomResponse.PlayerID = player.ID;
                leaveRoomResponse.RoomInfo = RoomInfo;

                byte[] data = MessagePackSerializer.Serialize(leaveRoomResponse);

                foreach (var item in Players)
                {
                    if (item.Value.NetPeer != null)
                    {
                        item.Value.NetPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.Success, "", data, LiteNetLib.DeliveryMethod.ReliableOrdered);
                    }
                }

                if (player.NetPeer != null)
                {
                    player.NetPeer.SendResponse(OperationCode.LeaveRoom, ReturnCode.Success, "", data, LiteNetLib.DeliveryMethod.ReliableOrdered);
                }
            }
        }

        /// <summary>
        /// 关闭房间
        /// </summary>
        public virtual void OnCloseRoom()
        {
            Log.Information("OnCloseRoom  RoomID {0}", ID);

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }

            foreach (var item in Players)
            {
                item.Value.OnLeaveRoom();
            }

            foreach (var item in Players)
            {
                if (item.Value.NetPeer != null)
                {
                    item.Value.NetPeer.SendResponse(OperationCode.CloseRoom, ReturnCode.Success, "", null, LiteNetLib.DeliveryMethod.ReliableOrdered);
                }
            }

            Players.Clear();
        }

        /// <summary>
        /// 房主更新
        /// </summary>
        /// <param name="master"></param>
        public void OnUpdateMaster(int master)
        {
            MasterID = master;
            if (RoomInfo != null)
            {
                RoomInfo.MasterID = master;
            }

        }

        /// <summary>
        /// 房间信息更新
        /// </summary>
        /// <param name="roomInfo"></param>
        public void OnUpdateRoomInfo(RoomInfo roomInfo)
        {

        }

        /// <summary>
        /// 每帧调用
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {
            foreach (var item in Players)
            {
                IPlayer player = item.Value;

                player.OnUpdate(deltaTime);
            }
        }


        private async void Update()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                while (true)
                {
                    await Task.Delay(Server.UpdateInterval, _cancellationTokenSource.Token);

                    OnUpdate(Server.UpdateInterval / 1000f);

                    if (Players.Count <= 0)
                    {
                        InactiveTime += Server.UpdateInterval;
                    }

                    if (InactiveTime > 2 * 60 * 60 * 1000)
                    {
                        break;
                    }
                }

                RoomManager.Instance.CloseRoom(ID);
            }
            catch (Exception ex)
            {
                if (ex is not TaskCanceledException)
                    Log.Information(ex.ToString());
            }
            Log.Information("OnStopUpdateRoom RoomID:{0}", ID);
        }
    }
}