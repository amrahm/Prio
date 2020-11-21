using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using JetBrains.Annotations;
using Prism.Services.Dialogs;
using Panel = System.Windows.Controls.Panel;
using Point = System.Drawing.Point;
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
                                         object defaultValue = null, bool twoWay = true) {
            var sProp = source.GetType().GetProperty(sourcePropName);
            var tProp = target.GetType().GetProperty(targetPropName);
            if(tProp != null && sProp != null) {
                object sValue = sProp.GetValue(source);
                if(sValue != null) tProp.SetValue(target, sValue);
                else if(defaultValue != null) tProp.SetValue(target, defaultValue);

                if(tProp.GetValue(target) is INotifyPropertyChanged tPropChange)
                    tPropChange.PropertyChanged += (oo, ee) => sProp.SetValue(source, tProp.GetValue(target));
                if(target is INotifyPropertyChanged tChange)
                    tChange.PropertyChanged += (oo, ee) => {
                        sProp.SetValue(source, tProp.GetValue(target));
                    };

                if(twoWay) {
                    if(sProp.GetValue(source) is INotifyPropertyChanged sPropChange)
                        sPropChange.PropertyChanged += (oo, ee) => tProp.SetValue(target, sProp.GetValue(source));
                    if(source is INotifyPropertyChanged sChange)
                        sChange.PropertyChanged += (oo, ee) => tProp.SetValue(target, sProp.GetValue(source));
                }
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
        public static T DeepCopy<T>(this T original) => (T) DeepCopy((object) original);

        private static readonly MethodInfo CloneMethod =
                typeof(object).GetMethod(nameof(MemberwiseClone), BindingFlags.NonPublic | BindingFlags.Instance);

        private static bool IsPrimitive(this Type type) {
            if(type == typeof(string)) return true;
            return type.IsValueType & type.IsPrimitive;
        }

        private static object DeepCopy(this object originalObject) =>
                InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));

        private static object InternalCopy(object originalObject, IDictionary<object, object> visited) {
            if(originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if(IsPrimitive(typeToReflect)) return originalObject;
            if(visited.ContainsKey(originalObject)) return visited[originalObject];
            if(typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if(typeToReflect.IsArray) {
                var arrayType = typeToReflect.GetElementType();
                if(IsPrimitive(arrayType) == false) {
                    Array clonedArray = (Array) cloneObject;
                    clonedArray.ForEach((array, indices) => {
                        Debug.Assert(clonedArray != null, nameof(clonedArray) + " != null");
                        array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices);
                    });
                }
            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited,
                                                               object cloneObject, Type typeToReflect) {
            if(typeToReflect.BaseType == null) return;
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
            CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType,
                       BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject,
                                       Type typeToReflect, BindingFlags bindingFlags =
                                               BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                               BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null) {
            foreach(FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags)) {
                if(filter != null && filter(fieldInfo) == false) continue;
                if(IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
    }

    internal class ReferenceEqualityComparer : EqualityComparer<object> {
        public override bool Equals(object x, object y) {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj) {
            return obj.GetHashCode();
        }
    }

    public static class ArrayExtensions {
        public static void ForEach(this Array array, Action<Array, int[]> action) {
            if(array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.position);
            while(walker.Step());
        }

        private class ArrayTraverse {
            public readonly int[] position;
            private readonly int[] _maxLengths;

            public ArrayTraverse(Array array) {
                _maxLengths = new int[array.Rank];
                for(int i = 0; i < array.Rank; ++i) {
                    _maxLengths[i] = array.GetLength(i) - 1;
                }
                position = new int[array.Rank];
            }

            public bool Step() {
                for(int i = 0; i < position.Length; ++i) {
                    if(position[i] < _maxLengths[i]) {
                        position[i]++;
                        for(int j = 0; j < i; j++) {
                            position[j] = 0;
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }


    public static class WindowHelpers {
        public static Screen CurrentScreen(this Window window) =>
                Screen.FromPoint(new Point((int) window.Left, (int) window.Top));
    }

    public static class DependencyObjectHelpers {
        /// <summary> Finds an Ancestor of type T of a given item on the visual tree. </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter.
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static T TryFindAncestor<T>(this DependencyObject child) where T : class {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            return parentObject switch {
                null => null, // we've reached the end of the tree
                T parent => parent, // check if the parent matches the type we're looking for
                _ => TryFindAncestor<T>(parentObject) //recurse
            };
        }

        public static void SetGlobalZ(this FrameworkElement element, int value) {
            UIElement parent = (UIElement) element.Parent;
            while(parent != null) {
                Panel.SetZIndex(parent, value);
                parent = (UIElement) VisualTreeHelper.GetParent(parent);
            }
        }
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