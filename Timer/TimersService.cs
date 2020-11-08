using System;
using System.Collections.ObjectModel;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Newtonsoft.Json;

namespace Timer {
    [Serializable]
    public class TimersService {
        [NonSerialized]
        public static readonly TimersService Singleton =
            Settings.LoadSettings<TimersService>(ModuleNames.TIMER) ?? new TimersService();

        public TimersGeneralConfig GeneralConfig { get; } = new TimersGeneralConfig();

        [JsonProperty(ItemConverterType = typeof(TimerConverter))]
        public ObservableCollection<ITimer> Timers { get; } = new ObservableCollection<ITimer>();

        public void SaveSettings() => Settings.SaveSettings(this, ModuleNames.TIMER);


        private class TimerConverter : JsonConverter<ITimer> {
            public override ITimer ReadJson(JsonReader reader, Type objectType, ITimer existingValue, bool hasExistingValue,
                JsonSerializer serializer) =>
                new TimerModel(serializer.Deserialize<TimerConfig>(reader));

            public override void WriteJson(JsonWriter writer, ITimer value, JsonSerializer serializer) =>
                serializer.Serialize(writer, value.Config);
        }
    }
}