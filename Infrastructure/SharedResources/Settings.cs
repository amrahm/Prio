using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Infrastructure.SharedResources {
    public static class Settings {
        private const string Path = "./prioConfig.json";

        public static void SaveSettings<T>(T moduleConfig, string moduleName) where T : class {
            if(!File.Exists(Path)) {
                File.WriteAllText(Path, JsonConvert.SerializeObject(new Dictionary<string, object>(), Formatting.Indented));
            } else {
                if(File.Exists($"{Path}.bak1")) File.Copy($"{Path}.bak1", $"{Path}.bak2", true);
                if(File.Exists($"{Path}.bak0")) File.Copy($"{Path}.bak0", $"{Path}.bak1", true);
                File.Copy(Path, $"{Path}.bak0", true);
            }

            Dictionary<string, object> fullConfig =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path));

            fullConfig[moduleName] = moduleConfig;

            File.WriteAllText(Path, JsonConvert.SerializeObject(fullConfig, Formatting.Indented));
        }

        public static T LoadSettings<T>(string moduleName) where T : class {
            Dictionary<string, string> fullConfig =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path));

            return fullConfig.ContainsKey(moduleName) ? JsonConvert.DeserializeObject<T>(fullConfig[moduleName]) : null;
        }
    }
}