
using Newtonsoft.Json;
using Serilog;

namespace Utils
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        private Dictionary<string, string> configs = new Dictionary<string, string>();

        private Dictionary<string, List<IConfig>> castConfigs = new Dictionary<string, List<IConfig>>();

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
            string[] jsonFiles = new string[] { "Utils/Config/Json/RoadConfig.json" };

            foreach (var file in jsonFiles)
            {
                string json = File.ReadAllText("./" + file);

                string fileName = Path.GetFileNameWithoutExtension(file);

                configs.Add(fileName, json);

                Log.Information("º”‘ÿ≈‰÷√Œƒº˛£∫" + fileName);
            }
        }

        public List<T>? GetAllConfigs<T>() where T : IConfig
        {
            string fileName = typeof(T).Name;

            List<T>? values = new List<T>();

            if (configs.ContainsKey(fileName))
            {
                string json = configs[fileName];
                if (castConfigs.ContainsKey(fileName))
                {
                    values = castConfigs[fileName].Cast<T>().ToList();
                }
                else
                {
                    values = JsonConvert.DeserializeObject<List<T>>(json);

                    if (values != null)
                    {
                        castConfigs.Add(fileName, values.Cast<IConfig>().ToList());
                    }
                }
            }

            return values;
        }

        public T? GetConfig<T>(int id) where T : IConfig
        {
            string fileName = typeof(T).Name;

            List<T>? values = GetAllConfigs<T>();

            if (values != null)
            {
                return values.FirstOrDefault((a) => a.ID == id);
            }

            return default(T);
        }

    }
}