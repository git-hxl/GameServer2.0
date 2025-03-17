

using Serilog;
using StackExchange.Redis;

namespace GameServer.Database
{
    internal class RedisManager : Singleton<RedisManager>
    {
        private ConnectionMultiplexer? redis;

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();

        }

        public async Task Init()
        {
            try
            {
                string connectionString = Server.Instance.Config.RedisConnectionStr;
                //connectionString = "localhost:6379,password=pitpat,defaultDatabase=0,connectTimeout=5000";
                redis = await ConnectionMultiplexer.ConnectAsync(connectionString);

                Log.Information($"Redis 连接状态：{redis.IsConnected}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public string? StringGet(string key, int dataBase = 0)
        {
            try
            {
                if (redis == null)
                    return null;

                IDatabase database = redis.GetDatabase(dataBase);

                string? info = database.StringGet(key);

                return info;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting string from Redis: {ex.Message}");
                return null;
            }
        }


        public bool StringSet(string key, string value, int dataBase = 0)
        {
            try
            {
                if (redis == null)
                    return false;

                IDatabase database = redis.GetDatabase(dataBase);
                var result = database.StringSet(key, value);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error setting string in Redis: {ex.Message}");
                return false;
            }
        }

        public bool DeleteKey(string key, int dataBase = 0)
        {
            try
            {
                if (redis == null)
                    return false;

                IDatabase database = redis.GetDatabase(dataBase);
                var result = database.KeyDelete(key);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error deleting key from Redis: {ex.Message}");
                return false;
            }
        }


        // 关闭连接
        public void CloseConnection()
        {
            try
            {
                if (redis == null)
                    return;

                if (redis.IsConnected)
                {
                    redis.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error closing Redis connection: {ex.Message}");
            }
        }
    }
}
