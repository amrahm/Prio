#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Infrastructure.SharedResources.ArrayExtensions;

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
            object? defaultValue = null) {
            var sProp = source.GetType().GetProperty(sourcePropName);
            var tProp = target.GetType().GetProperty(targetPropName);
            if(tProp != null && sProp != null) {
                object? sValue = sProp.GetValue(source);
                if(sValue != null)
                    tProp.SetValue(target, sValue);
                else if(defaultValue != null)
                    tProp.SetValue(target, defaultValue);

                if (tProp.GetValue(target) is INotifyPropertyChanged tPropChange)
                    tPropChange.PropertyChanged += (oo, ee) => sProp.SetValue(source, tProp.GetValue(target));
                if (target is INotifyPropertyChanged tChange)
                    tChange.PropertyChanged += (oo, ee) => sProp.SetValue(source, tProp.GetValue(target));
            }
        }
    }

    public static class ObjectExtensions {
        private static readonly MethodInfo CloneMethod =
            typeof(object).GetMethod(nameof(MemberwiseClone), BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type) {
            if(type == typeof(string)) return true;
            return type.IsValueType & type.IsPrimitive;
        }

        public static object DeepCopy(this object originalObject) =>
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
                    clonedArray.ForEach((array, indices) =>
                                            array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }
            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited,
            object cloneObject, Type typeToReflect) {
            if(typeToReflect.BaseType != null) {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType,
                           BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject,
            IReflect typeToReflect,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                        BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null) {
            foreach(FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags)) {
                if(filter != null && filter(fieldInfo) == false) continue;
                if(IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        public static T DeepCopy<T>(this T original) {
            return (T) DeepCopy((object) original);
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<object> {
        public override bool Equals(object x, object y) => ReferenceEquals(x, y);
        public override int GetHashCode(object obj) => obj.GetHashCode();
    }

    namespace ArrayExtensions {
        public static class ArrayExtensions {
            public static void ForEach(this Array array, Action<Array, int[]> action) {
                if(array.LongLength == 0) return;
                ArrayTraverse walker = new ArrayTraverse(array);
                do action(array, walker.position);
                while(walker.Step());
            }
        }

        internal class ArrayTraverse {
            public readonly int[] position;
            private readonly int[] _maxLengths;

            public ArrayTraverse(Array array) {
                _maxLengths = new int[array.Rank];
                for(int i = 0; i < array.Rank; ++i) _maxLengths[i] = array.GetLength(i) - 1;
                position = new int[array.Rank];
            }

            public bool Step() {
                for(int i = 0; i < position.Length; ++i) {
                    if(position[i] < _maxLengths[i]) {
                        position[i]++;
                        for(int j = 0; j < i; j++) position[j] = 0;
                        return true;
                    }
                }
                return false;
            }
        }
    }


    public static class WindowHelpers {
        public static Screen CurrentScreen(this Window window) {
            return Screen.FromPoint(new System.Drawing.Point((int) window.Left, (int) window.Top));
        }
    }
}