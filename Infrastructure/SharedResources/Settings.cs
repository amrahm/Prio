using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.SharedResources {
    public static class Settings {
        private const string PATH = "./prioConfig.json";

        private static readonly List<string> BakAdditions = new() {""};

        static Settings() {
            for(int i = 0; i <= 5; i++) BakAdditions.Add($".bak{i}");
        }

        public static bool SettingsExists() => BakAdditions.Any(addition => _SettingsExists($"{PATH}{addition}"));

        private static bool _SettingsExists(string path = PATH) =>
                File.Exists(path) && File.ReadAllText(path).Trim().Length > 5;

        /// <summary> Save settings for a module, or a specific instance of a module if an instanceID is supplied </summary>
        /// <typeparam name="T"> Settings class for the module </typeparam>
        /// <param name="moduleConfig"> Settings object </param>
        /// <param name="moduleName"> Name of the module to load </param>
        public static void SaveSettings<T>(T moduleConfig, string moduleName) where T : class {
            static bool FileOlderOrNull(string path, int hours) {
                return !File.Exists(path) || File.GetLastWriteTime(path) <= DateTime.Now.AddHours(-hours);
            }

            if(!_SettingsExists()) {
                File.WriteAllText(PATH, JsonConvert.SerializeObject(new Dictionary<string, object>(), Formatting.Indented));
            } else if(FileOlderOrNull($"{PATH}.bak0", 1)) {
                // w/ constant saves, bak0 would be [0, 1) hours old
                // bak1 would be [1, 24) hours old, and bak2 would be [24, 48) hours old, etc.
                if(FileOlderOrNull($"{PATH}.bak1", 24)) {
                    for(int i = BakAdditions.Count - 2; i >= 0; i--) {
                        (string ext, string next) = (BakAdditions[i], BakAdditions[i + 1]);
                        if(File.Exists($"{PATH}{ext}")) File.Copy($"{PATH}{ext}", $"{PATH}{next}", true);
                    }
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
            T setting = null;

            foreach(string addition in BakAdditions) {
                try {
                    string path = $"{PATH}{addition}";
                    if(!_SettingsExists(path)) continue;

                    var fullConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(path));
                    setting = (fullConfig.GetValueOrDefault(moduleName, null) as JObject)?.ToObject<T>();
                    if(setting != null) break;
                } catch(Exception) {
                    // ignored
                }
            }

            return setting;
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