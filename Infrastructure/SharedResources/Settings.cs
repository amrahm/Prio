using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.SharedResources {
    public static class Settings {
        private const string Path = "./prioConfig.json";

        public static void SaveSettings<T>(T moduleConfig, string moduleName, Guid? instanceID = null) where T : class {
            if(!File.Exists(Path)) {
                File.WriteAllText(Path, JsonConvert.SerializeObject(new Dictionary<string, object>(), Formatting.Indented));
            } else {
                if(File.Exists($"{Path}.bak1")) File.Copy($"{Path}.bak1", $"{Path}.bak2", true);
                if(File.Exists($"{Path}.bak0")) File.Copy($"{Path}.bak0", $"{Path}.bak1", true);
                File.Copy(Path, $"{Path}.bak0", true);
            }

            var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path));

            if(instanceID.HasValue) {
                Dictionary<Guid, object> moduleDict;
                if((moduleDict = GetFromJObjDict<string, Dictionary<Guid, object>>(moduleName, fullConfig)) == null) {
                    moduleDict = new Dictionary<Guid, object>();
                }
                moduleDict[instanceID.Value] = moduleConfig;
                fullConfig[moduleName] = moduleDict;
            } else
                fullConfig[moduleName] = moduleConfig;

            File.WriteAllText(Path, JsonConvert.SerializeObject(fullConfig, Formatting.Indented));
        }

        public static T LoadSettings<T>(string moduleName, Guid? instanceID = null) where T : class {
            if(!File.Exists(Path)) return null; //TODO write code to check for .bak files first

            var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(Path));

            if(instanceID.HasValue) {
                var moduleDict = GetFromJObjDict<string, Dictionary<Guid, object>>(moduleName, fullConfig);
                return moduleDict != null ? GetFromJObjDict<Guid, T>(instanceID.Value, moduleDict) : null;
            }

            return GetFromJObjDict<string, T>(moduleName, fullConfig);
        }

        private static T GetFromJObjDict<TK, T>(TK moduleName, IReadOnlyDictionary<TK, object> fullConfig)
            where T : class {
            return fullConfig.TryGetValue(moduleName, out object mDictObj) ? ((JObject) mDictObj).ToObject<T>() : null;
        }
    }
}