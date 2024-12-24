
using Newtonsoft.Json;
using Serilog;

namespace Utils
{
    public class ConfigManager : Singleton<ConfigManager>
    {

        private Dictionary<string, string> _config = new Dictionary<string, string>();

        private Dictionary<string, List<IConfig>> _configValues = new Dictionary<string, List<IConfig>>();

        protected override void OnDispose()
        {
            //throw new System.NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new System.NotImplementedException();

            ReadConfig();
        }

        private void ReadConfig()
        {
            string[] jsonFiles = new string[] { "TestConfig.json" };

            foreach (var file in jsonFiles)
            {
                string json = File.ReadAllText( "./" + file);

                string fileName = Path.GetFileNameWithoutExtension(file);

                _config.Add(fileName, json);

                Log.Information("º”‘ÿ≈‰÷√Œƒº˛£∫" + fileName);
            }
        }

        public List<T> GetAllConfigs<T>() where T : IConfig
        {
            string fileName = typeof(T).Name;

            List<T> values = new List<T>();

            if (_config.ContainsKey(fileName))
            {
                if (_configValues.ContainsKey(fileName))
                {
                    foreach (var item in _configValues[fileName])
                    {
                        values.Add((T)item);
                    }
                }
                else
                {
                    values = JsonConvert.DeserializeObject<List<T>>(_config[fileName]);

                    List<IConfig> configValues = new List<IConfig>();

                    foreach (var item in values)
                    {
                        configValues.Add(item);
                    }

                    _configValues.Add(fileName, configValues);
                }
            }

            return values;
        }

        public T GetConfig<T>(int id) where T : IConfig
        {
            string fileName = typeof(T).Name;

            List<T> values = GetAllConfigs<T>();

            if (values != null)
            {
                return values.FirstOrDefault((a) => a.ID == id);
            }

            return default(T);
        }

    }
}