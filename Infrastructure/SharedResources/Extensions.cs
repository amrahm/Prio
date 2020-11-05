using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Infrastructure.SharedResources {
    public static class NotificationBubbler {
        /// <summary>
        /// Bubble up changes from within the object, e.g.: <para />
        /// public TimerConfig Config {
        ///     get => _config;
        ///     set => NotificationBubbler.BubbleSetter(ref _config, value, (o, e) => this.OnPropertyChanged());
        /// }
        /// </summary>
        /// <param name="obj"> object to be set </param>
        /// <param name="value"> new value for object </param>
        /// <param name="thisChanged"> Lets you specify what OnPropertyChanged method should be called </param>
        public static void BubbleSetter<T>(ref T obj, T value, PropertyChangedEventHandler thisChanged)
            where T : class, INotifyPropertyChanged {
            if(obj != value) {
                // Clean-up old event handler:
                if(obj != null) obj.PropertyChanged -= thisChanged;

                obj = value;

                if(obj != null) obj.PropertyChanged += thisChanged;
            }
        }
    }

    public static class BindingHelpers {
        public static void ManualBinding(object source, string sourcePropName, object target, string targetPropName,
            object defaultValue = null) {
            var sProp = source.GetType().GetProperty(sourcePropName);
            var tProp = target.GetType().GetProperty(targetPropName);
            if(tProp != null && sProp != null) {
                object sValue = sProp.GetValue(source);
                if(sValue != null) tProp.SetValue(target, sValue);
                else if(defaultValue != null) tProp.SetValue(target, defaultValue);

                if(tProp.GetValue(target) is INotifyPropertyChanged tPropChange)
                    tPropChange.PropertyChanged += (oo, ee) => sProp.SetValue(source, tProp.GetValue(target));
                if(target is INotifyPropertyChanged tChange)
                    tChange.PropertyChanged += (oo, ee) => sProp.SetValue(source, tProp.GetValue(target));
            }
        }
    }

    public static class ObjectExtensions {
        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T DeepCopy<T>(this T source) {
            // Don't serialize a null object, simply return the default for that object
            if(source == null) return default;

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }


    public static class WindowHelpers {
        public static Screen CurrentScreen(this Window window) =>
            Screen.FromPoint(new System.Drawing.Point((int) window.Left, (int) window.Top));
    }
}