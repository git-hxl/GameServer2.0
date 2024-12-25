using Newtonsoft.Json;
using Serilog;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
                loggerConfiguration.WriteTo.File("./Log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true).WriteTo.Console();
                loggerConfiguration.MinimumLevel.Debug();
                Log.Logger = loggerConfiguration.CreateLogger();

                ServerConfig? serverConfig = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText("./ServerConfig.json"));

                if (serverConfig == null)
                {
                    throw new Exception("读取服务器配置文件失败！");
                }

                Server.Instance.InitConfig(serverConfig);

                DateTime dateTime = DateTime.Now;

                Server.Instance.Start();

                while (true)
                {
                    Thread.Sleep(serverConfig.UpdateInterval);

                    TimeSpan deltaTime = DateTime.Now - dateTime;
                    Server.Instance.Update((float)deltaTime.TotalSeconds);

                    dateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
