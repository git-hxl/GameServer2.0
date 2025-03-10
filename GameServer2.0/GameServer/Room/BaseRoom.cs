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

        private Timer? _timer;
        public virtual void OnInit(int id)
        {
            ID = id;

            MasterID = -1;

            CreateTime = DateTime.Now;

            InactiveTime = 0;

            Log.Information("OnCreateRoom RoomID {0} ", id);

            _timer = new Timer(Update, null, 0, Server.UpdateInterval);
        }

        public void OnAcquire()
        {

        }

        public virtual void OnRelease()
        {
            if (_timer != null)
                _timer.Dispose();
            _timer = null;
        }

        public List<BasePlayer> GetActivePlayers()
        {
            lock (this)
            {
                return Players.Values.Where((a) => a.NetPeer != null && a.NetPeer.ConnectionState == ConnectionState.Connected).ToList();
            }
        }

        public virtual void OnJoinPlayer(BasePlayer player)
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
                joinRoomResponse.UserID = player.ID;
                joinRoomResponse.RoomInfo = RoomInfo;

                var players = GetActivePlayers();

                joinRoomResponse.Users = players.Select((a) => a.UserInfo).ToList();

                byte[] data = MessagePackSerializer.Serialize(joinRoomResponse);

                foreach (var item in players)
                {
                    item.NetPeer?.SendResponse(OperationCode.JoinRoom, ReturnCode.Success, joinRoomResponse, "");
                }
            }
        }

        public virtual void OnLeavePlayer(BasePlayer player)
        {
            if (Players.TryRemove(player.ID, out _))
            {
                if (MasterID == player.ID)
                {
                    var master = Players.Values.FirstOrDefault((a) => a.NetPeer != null);
                    if (master != null)
                    {
                        OnUpdateMaster(master.ID);
                    }
                }

                LeaveRoomResponse leaveRoomResponse = new LeaveRoomResponse();

                leaveRoomResponse.UserID = player.ID;
                leaveRoomResponse.RoomInfo = RoomInfo;

                byte[] data = MessagePackSerializer.Serialize(leaveRoomResponse);

                var players = GetActivePlayers();

                foreach (var item in players)
                {
                    item.NetPeer?.SendResponse(OperationCode.LeaveRoom, ReturnCode.Success, leaveRoomResponse);
                }

                player.NetPeer?.SendResponse(OperationCode.LeaveRoom, ReturnCode.Success, leaveRoomResponse);

                player.OnLeaveRoom(this);
            }
        }

        /// <summary>
        /// 关闭房间
        /// </summary>
        public virtual void OnCloseRoom()
        {
            Log.Information("OnCloseRoom  RoomID {0}", ID);

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


        private void Update(object? state)
        {
            try
            {
                float deltaTime = Server.UpdateInterval / 1000f;

                OnUpdate(deltaTime);

                if (Players.Count <= 0)
                {
                    InactiveTime += deltaTime;
                }
                else
                {
                    InactiveTime = 0f;
                }

                //移除长时间空载的房间

                if (InactiveTime > 60f)
                {
                    RoomManager.Instance.CloseRoom(ID);

                    Log.Information("OnStopUpdateRoom RoomID:{0}", ID);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());

                RoomManager.Instance.CloseRoom(ID);
            }
        }
    }
}