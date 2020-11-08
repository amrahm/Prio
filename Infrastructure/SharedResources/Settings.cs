using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.SharedResources {
    public static class Settings {
        private const string PATH = "./prioConfig.json";

        /// <summary> Save settings for a module, or a specific instance of a module if an instanceID is supplied </summary>
        /// <typeparam name="T"> Settings class for the module </typeparam>
        /// <param name="moduleConfig"> Settings object </param>
        /// <param name="moduleName"> Name of the module to load </param>
        /// <param name="instanceID"> Must be supplied if saved with instanceID </param>
        public static void SaveSettings<T>(T moduleConfig, string moduleName, Guid? instanceID = null) where T : class {
            if(!File.Exists(PATH)) {
                File.WriteAllText(PATH, JsonConvert.SerializeObject(new Dictionary<string, object>(), Formatting.Indented));
            } else {
                if(File.Exists($"{PATH}.bak1")) File.Copy($"{PATH}.bak1", $"{PATH}.bak2", true);
                if(File.Exists($"{PATH}.bak0")) File.Copy($"{PATH}.bak0", $"{PATH}.bak1", true);
                File.Copy(PATH, $"{PATH}.bak0", true);
            }

            var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(PATH));

            if(instanceID.HasValue) {
                Dictionary<Guid, object> moduleDict;
                if((moduleDict = GetFromJObjDict<string, Dictionary<Guid, object>>(moduleName, fullConfig)) == null) {
                    moduleDict = new Dictionary<Guid, object>();
                }
                moduleDict[instanceID.Value] = moduleConfig;
                fullConfig[moduleName] = moduleDict;
            } else
                fullConfig[moduleName] = moduleConfig;

            File.WriteAllText(PATH, JsonConvert.SerializeObject(fullConfig, Formatting.Indented));
        }

        /// <summary> Loads settings for a module, or a specific instance of a module that was saved with an instanceID </summary>
        /// <typeparam name="T"> Settings class for the module </typeparam>
        /// <param name="moduleName"> Name of the module to load </param>
        /// <param name="instanceID"> Must be supplied if saved with instanceID </param>
        public static T LoadSettings<T>(string moduleName, Guid? instanceID = null) where T : class {
            if(!File.Exists(PATH)) return null; //TODO write code to check for .bak files first

            var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(PATH));

            if(instanceID.HasValue) {
                var moduleDict = GetFromJObjDict<string, Dictionary<Guid, object>>(moduleName, fullConfig);
                return moduleDict != null ? GetFromJObjDict<Guid, T>(instanceID.Value, moduleDict) : null;
            }

            return GetFromJObjDict<string, T>(moduleName, fullConfig);
        }

        /// <summary> For modules that save instances, this loads the whole dict of instances </summary>
        /// <typeparam name="T"> Settings class for the module </typeparam>
        /// <param name="moduleName"> Name of the module to load </param>
        /// <returns></returns>
        public static Dictionary<Guid, T> LoadSettingsDict<T>(string moduleName) where T : class {
            if(!File.Exists(PATH)) return null; //TODO write code to check for .bak files first

            var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(PATH));
            return GetFromJObjDict<string, Dictionary<Guid, T>>(moduleName, fullConfig);
        }

        private static T GetFromJObjDict<TK, T>(TK moduleName, IReadOnlyDictionary<TK, object> fullConfig)
            where T : class {
            return fullConfig.TryGetValue(moduleName, out object mDictObj) ? ((JObject) mDictObj).ToObject<T>() : null;
        }
    }
}