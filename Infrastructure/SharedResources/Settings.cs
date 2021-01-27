using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.SharedResources {
    public static class Settings {
        private const string PATH = "./prioConfig.json";

        public static bool DoSettingsExists() => File.Exists(PATH) && File.ReadAllText(PATH).Trim().Length > 5;

        /// <summary> Save settings for a module, or a specific instance of a module if an instanceID is supplied </summary>
        /// <typeparam name="T"> Settings class for the module </typeparam>
        /// <param name="moduleConfig"> Settings object </param>
        /// <param name="moduleName"> Name of the module to load </param>
        public static void SaveSettings<T>(T moduleConfig, string moduleName) where T : class {
            static bool FileOlderOrNull(string path, int hours) {
                return !File.Exists(path) || File.GetLastWriteTime(path) <= DateTime.Now.AddHours(-hours);
            }

            if(!DoSettingsExists()) {
                File.WriteAllText(PATH, JsonConvert.SerializeObject(new Dictionary<string, object>(), Formatting.Indented));
            } else if(FileOlderOrNull($"{PATH}.bak0", 1)) {
                // w/ constant saves, bak0 would be [0, 1) hours old
                // bak1 would be [1, 24) hours old, and bak2 would be [24, 48) hours old.
                if(FileOlderOrNull($"{PATH}.bak1", 24)) {
                    if(File.Exists($"{PATH}.bak1")) File.Copy($"{PATH}.bak1", $"{PATH}.bak2", true);
                    if(File.Exists($"{PATH}.bak0")) File.Copy($"{PATH}.bak0", $"{PATH}.bak1", true);
                }
                File.Copy(PATH, $"{PATH}.bak0", true);
            }

            var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(PATH));
            fullConfig[moduleName] = moduleConfig;

            File.WriteAllText(PATH, JsonConvert.SerializeObject(fullConfig, Formatting.Indented));
        }

        /// <summary> Loads settings for a module, or a specific instance of a module that was saved with an instanceID </summary>
        /// <typeparam name="T"> Settings class for the module </typeparam>
        /// <param name="moduleName"> Name of the module to load </param>
        public static T LoadSettings<T>(string moduleName) where T : class {
            if(!DoSettingsExists()) return null; //TODO write code to check for .bak files first

            var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(PATH));
            return fullConfig.TryGetValue(moduleName, out object mDictObj) ? ((JObject) mDictObj).ToObject<T>() : null;
        }
    }

    public class ConcreteConverter<T> : JsonConverter {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader,
                                        Type objectType, object existingValue, JsonSerializer serializer) {
            return serializer.Deserialize<T>(reader);
        }

        public override void WriteJson(JsonWriter writer,
                                       object value, JsonSerializer serializer) {
            serializer.Serialize(writer, value);
        }
    }
}