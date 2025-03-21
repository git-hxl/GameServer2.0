using GameServer.Protocol;
using GameServer.Utils;
using LiteNetLib;
using MessagePack;
using Serilog;
using System.Collections.Concurrent;
using Utils;

namespace GameServer
{
    public class BaseRoom : IReference
    {
        public int ID { get; private set; }

        public int MasterID { get; private set; }

        public RoomInfo? RoomInfo { get; private set; }
        public ConcurrentDictionary<int, BasePlayer> Players { get; private set; } = new ConcurrentDictionary<int, BasePlayer>();

        public DateTime CreateTime { get; private set; }

        public float InactiveTime { get; private set; }

        public bool IsClosed { get; private set; }

        public virtual void OnInit(int id)
        {
            ID = id;

            MasterID = -1;

            CreateTime = DateTime.Now;

            InactiveTime = 0;

            Log.Information("OnCreateRoom RoomID {0} ", id);

            RoomInfo = new RoomInfo();
            RoomInfo.RoomID = ID;
            RoomInfo.MasterID = MasterID;

            IsClosed = false;

            StartUpdate();
        }


        private async Task StartUpdate()
        {
            var loopTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(Server.UpdateInterval));

            while (await loopTimer.WaitForNextTickAsync())
            {
                try
                {
                    if (IsClosed)
                    {
                        break;
                    }

                    // 执行轮询任务

                    float deltaTime = (float)loopTimer.Period.TotalSeconds;

                    OnUpdate(deltaTime);

                    Log.Information("Test RoomID:{0} ThreadID:{1} Period:{2}", ID, Thread.CurrentThread.ManagedThreadId, deltaTime);

                    if (Players.Count <= 0)
                    {
                        InactiveTime += deltaTime;
                    }
                    else
                    {
                        InactiveTime = 0f;
                    }

                    //移除长时间空载的房间

                    if (InactiveTime > 1 * 60)
                    {
                        RoomManager.Instance.CloseRoom(ID);

                        Log.Information("OnStopUpdateRoom RoomID:{0}", ID);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);

                    RoomManager.Instance.CloseRoom(ID);

                    break;
                }
            }
        }


        public void OnAcquire()
        {

        }

        public virtual void OnRelease()
        {
           
        }

        public List<BasePlayer> GetActivePlayers()
        {
            lock (this)
            {
                return Players.Values.Where((a) => a.NetPeer != null && a.NetPeer.ConnectionState == ConnectionState.Connected).ToList();
            }
        }

        public void JoinPlayer(BasePlayer player)
        {
            if (Players.TryAdd(player.PlayerID, player))
            {
                player.OnJoinRoom(this);

                if (MasterID == -1)
                {
                    OnUpdateMaster(player.PlayerID);
                }

                JoinRoomResponse joinRoomResponse = new JoinRoomResponse();

                joinRoomResponse.RoomID = ID;
                joinRoomResponse.UserID = player.PlayerID;
                joinRoomResponse.RoomInfo = RoomInfo;

                var players = GetActivePlayers();

                joinRoomResponse.Users = players.Select((a) => a.UserInfo).ToList();

                byte[] data = MessagePackSerializer.Serialize(joinRoomResponse);

                foreach (var item in players)
                {
                    item.NetPeer?.SendResponse(OperationCode.JoinRoom, ReturnCode.Success, joinRoomResponse, "");
                }

                OnJoinPlayer(player);
            }
        }

        public void RemovePlayer(BasePlayer player)
        {
            if (Players.TryRemove(player.PlayerID, out _))
            {
                OnLeavePlayer(player);

                var players = GetActivePlayers();

                if (MasterID == player.PlayerID)
                {
                    var master = players.FirstOrDefault();
                    if (master != null)
                    {
                        OnUpdateMaster(master.PlayerID);
                    }
                    else
                    {
                        OnUpdateMaster(-1);
                    }
                }

                LeaveRoomResponse leaveRoomResponse = new LeaveRoomResponse();

                leaveRoomResponse.UserID = player.PlayerID;
                leaveRoomResponse.RoomInfo = RoomInfo;

                byte[] data = MessagePackSerializer.Serialize(leaveRoomResponse);

                foreach (var item in players)
                {
                    item.NetPeer?.SendResponse(OperationCode.LeaveRoom, ReturnCode.Success, leaveRoomResponse);
                }

                player.NetPeer?.SendResponse(OperationCode.LeaveRoom, ReturnCode.Success, leaveRoomResponse);

                player.OnLeaveRoom(this);
            }
        }

        protected virtual void OnJoinPlayer(BasePlayer player)
        {

        }

        protected virtual void OnLeavePlayer(BasePlayer player)
        {

        }

        /// <summary>
        /// 关闭房间
        /// </summary>
        public virtual void OnCloseRoom()
        {
            Log.Information("OnCloseRoom  RoomID {0}", ID);

            IsClosed = true;

            var players = GetActivePlayers();

            foreach (var item in players)
            {
                item.OnLeaveRoom(this);
            }

            foreach (var item in players)
            {
                item.NetPeer?.SendResponse(OperationCode.CloseRoom, ReturnCode.Success, null);
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
        protected virtual void OnUpdate(float deltaTime)
        {
            foreach (var item in Players)
            {
                var player = item.Value;

                player.OnUpdate(deltaTime);
            }

            // Log.Information("OnUpdate RoomID:{0} deltaTime:{1}", ID, deltaTime);
        }
    }
}