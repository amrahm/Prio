using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Prism.Services.Dialogs;
using TextBox = System.Windows.Controls.TextBox;

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

    public static class VirtualDesktopExtensions {
        /// <summary> Turns the 1-indexed string of numbers into a 0-indexed Set </summary>
        public static HashSet<int> DesktopStringToSet(string listString) {
            if(string.IsNullOrEmpty(listString)) return new HashSet<int>();
            return Array.ConvertAll(listString.Trim().Trim(',').Replace(" ", "").Split(','), s => int.Parse(s) - 1)
                        .ToHashSet();
        }

        /// <summary> Turns the 0-indexed Set of numbers into a 1-indexed string </summary>
        public static string DesktopSetToString(IEnumerable<int> set) =>
                string.Join(", ", set?.Select(x => x + 1) ?? new HashSet<int>());


        private static readonly Regex Rx = new Regex(@"[^\d,\s]|((?<=,\s),\s?)|(?<!\d),|(?<!,)\s|(?<=\d{2})\d",
                                                     RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void EnforceIntList(TextBox textBox) {
            textBox.TextChanged += (o,  e) => {
                int oldIndex = textBox.CaretIndex;
                string oldValue = textBox.Text;
                string validInput = Rx.Replace(textBox.Text, "");
                textBox.Text = validInput;

                if(!oldValue.Equals(validInput)) textBox.CaretIndex = oldIndex == 0 ? 0 : oldIndex - 1;
            };
            textBox.LostFocus += (o,  e) => textBox.Text = textBox.Text.Trim().Trim(',');
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

    public static class PrismExtensions {
        public static Task<IDialogResult> ShowDialogAsync(this IDialogService dialogService, string name,
                                                          IDialogParameters parameters) {
            var tcs = new TaskCompletionSource<IDialogResult>();

            try {
                dialogService.ShowDialog(name, parameters, result => tcs.SetResult(result));
            } catch(Exception ex) {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }

    public class EnumerationExtension : MarkupExtension {
        private Type _enumType;


        public EnumerationExtension(Type enumType) =>
                EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));

        public Type EnumType {
            get => _enumType;
            private set {
                if(_enumType == value) return;

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if(enumType.IsEnum == false) throw new ArgumentException("Type must be an Enum.");

                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var enumValues = Enum.GetValues(EnumType);

            return (from object enumValue in enumValues
                    select new EnumerationMember {
                        Value = enumValue,
                        Description = GetDescription(enumValue)
                    }).ToArray();
        }

        private string GetDescription(object enumValue) {
            return EnumType.GetField(enumValue.ToString()!)
                           ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                           .FirstOrDefault() is DescriptionAttribute descriptionAttribute ?
                           descriptionAttribute.Description :
                           enumValue.ToString();
        }

        public class EnumerationMember {
            public string Description { [UsedImplicitly] get; set; }
            public object Value { [UsedImplicitly] get; set; }
        }
    }
}