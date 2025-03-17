

using Dapper;
using MySqlConnector;
using Newtonsoft.Json;
using Serilog;
using System.Data;

namespace GameServer.Database
{
    internal class MysqlManager : Singleton<MysqlManager>
    {
        private string _connectionString;
        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }

        public void Init()
        {
            _connectionString = Server.Instance.Config.SQLConnectionStr;

           // Test();
        }

        public void Test()
        {
            List<Table_User> dataTable = Query<Table_User>("select * from user");

            Log.Information(JsonConvert.SerializeObject(dataTable));
        }

        /// <summary>
        /// 执行查询操作，返回实体列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">SQL 查询语句</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>实体列表</returns>
        public List<T> Query<T>(string query, object parameters = null)
        {
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                try
                {
                    return db.Query<T>(query, parameters).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行查询时出错: {ex.Message}");
                    return new List<T>();
                }
            }
        }

        /// <summary>
        /// 执行非查询操作（如插入、更新、删除）
        /// </summary>
        /// <param name="query">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响的行数</returns>
        public int Execute(string query, object parameters = null)
        {
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                try
                {
                    return db.Execute(query, parameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行非查询操作时出错: {ex.Message}");
                    return -1;
                }
            }
        }

        /// <summary>
        /// 执行单个结果查询
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="query">SQL 查询语句</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>单个结果</returns>
        public T QuerySingle<T>(string query, object parameters = null)
        {
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                try
                {
                    return db.QuerySingle<T>(query, parameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行单个结果查询时出错: {ex.Message}");
                    return default(T);
                }
            }
        }

        /// <summary>
        /// 执行单个结果查询，如果无结果返回默认值
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="query">SQL 查询语句</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>单个结果或默认值</returns>
        public T QuerySingleOrDefault<T>(string query, object parameters = null)
        {
            using (IDbConnection db = new MySqlConnection(_connectionString))
            {
                try
                {
                    return db.QuerySingleOrDefault<T>(query, parameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行单个结果查询（默认值）时出错: {ex.Message}");
                    return default(T);
                }
            }
        }
    }
}
