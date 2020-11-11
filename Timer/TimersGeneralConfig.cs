using System;
using System.Collections.ObjectModel;
using Infrastructure.SharedResources;
using Newtonsoft.Json;

namespace Timer {
    [Serializable]
    public class TimersGeneralConfig : NotifyPropertyChanged {
        public ShortcutDefinition ShowHideTimersShortcut { get; set; }
        public ShortcutDefinition KeepTimersOnTopShortcut { get; set; }
        public ShortcutDefinition MoveTimersBehindShortcut { get; set; }
        public VisibilityState DefaultVisibilityState { get; set; }

        [JsonProperty(ItemConverterType = typeof(TimerConverter))]
        public ObservableCollection<ITimer> Timers { get; } = new ObservableCollection<ITimer>();
    }

    public enum VisibilityState { KeepOnTop, MoveBehind, Hidden }

    internal class TimerConverter : JsonConverter<ITimer> {
        public override ITimer ReadJson(JsonReader reader, Type objectType, ITimer existingValue, bool hasExistingValue,
            JsonSerializer serializer) =>
            new TimerModel(serializer.Deserialize<TimerConfig>(reader));

        public override void WriteJson(JsonWriter writer, ITimer value, JsonSerializer serializer) =>
            serializer.Serialize(writer, value.Config);
    }
}