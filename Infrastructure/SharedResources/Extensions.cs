﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Prism.Dialogs;
using Panel = System.Windows.Controls.Panel;
using Color = System.Drawing.Color;
using ComboBox = System.Windows.Controls.ComboBox;
using MColor = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

// ReSharper disable UnusedMember.Global

namespace Infrastructure.SharedResources {
    public static class NotificationBubbler {
        /// <summary>
        /// Bubble up changes from within the object, e.g.: <para />
        /// public TimerConfig Config {
        ///     get => _config;
        ///     set => NotificationBubbler.BubbleSetter(ref _config, value, (_, _) => this.OnPropertyChanged());
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

    public static class DialogServiceExtensions {
        // public static Task<IDialogResult> ShowDialogAsync(this IDialogService dialogService, string name,
        //                                                   IDialogParameters parameters) {
        //     TaskCompletionSource<IDialogResult> tcs = new();
        //
        //     try {
        //         dialogService.ShowDialog(name, parameters, tcs.SetResult);
        //     } catch(Exception ex) {
        //         tcs.SetException(ex);
        //     }
        //     return tcs.Task;
        // }

        public static Task<IDialogResult> ShowAsync(this IDialogService dialogService, string name,
                                                    IDialogParameters parameters) {
            TaskCompletionSource<IDialogResult> tcs = new();

            try {
                dialogService.Show(name, parameters, tcs.SetResult);
            } catch(Exception ex) {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }

    public static class BindingHelpers {
        public static void ManualBinding(object source, string sourcePropName, object target, string targetPropName,
                                         object defaultValue = null, bool twoWay = true, Action callback = null) {
            var sProp = source.GetType().GetProperty(sourcePropName);
            var tProp = target.GetType().GetProperty(targetPropName);
            if(tProp != null && sProp != null) {
                object sValue = sProp.GetValue(source);
                if(sValue != null) tProp.SetValue(target, sValue);
                else if(defaultValue != null) tProp.SetValue(target, defaultValue);

                void UpdateSource() {
                    sProp.SetValue(source, tProp.GetValue(target));
                    callback?.Invoke();
                }

                if(tProp.GetValue(target) is INotifyPropertyChanged tPropChange)
                    tPropChange.PropertyChanged += (_, _) => UpdateSource();
                if(target is INotifyPropertyChanged tChange)
                    tChange.PropertyChanged += (_, _) => UpdateSource();

                void UpdateTarget() {
                    tProp.SetValue(target, sProp.GetValue(source));
                    callback?.Invoke();
                }

                if(twoWay) {
                    if(sProp.GetValue(source) is INotifyPropertyChanged sPropChange)
                        sPropChange.PropertyChanged += (_, _) => UpdateTarget();
                    if(source is INotifyPropertyChanged sChange)
                        sChange.PropertyChanged += (_, _) => tProp.SetValue(target, sProp.GetValue(source));
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
    }

    public static class EnumerableExtensions {
        public static void ForEach<T>(this IEnumerable<T> value, Action<T> action) {
            foreach(T item in value) action(item);
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
            if(source == null) {
                return default;
            }

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


        public static void MoveWindowInBounds(this Window window) {
            double dpiScaling = GetDpiFactor(window);

            Rectangle wA = window.CurrentScreen().WorkingArea;

            window.Left += Math.Max(wA.Left * dpiScaling - window.Left, 0);
            window.Left += Math.Min(wA.Right * dpiScaling - (window.Left + window.ActualWidth), 0);
            window.Top += Math.Max(wA.Top * dpiScaling - window.Top, 0);
            window.Top += Math.Min(wA.Bottom * dpiScaling - (window.Top + window.ActualHeight), 0);
        }


        public static void CenterOnScreen(this Window window) => CenterOnScreen(window, window.CurrentScreen().WorkingArea);

        public static void CenterOnScreen(this Window window, Rectangle screen) {
            double dpiScaling = GetDpiFactor(window);

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = (screen.Width * dpiScaling - window.ActualWidth) / 2 + screen.Left * dpiScaling;
            window.Top = (screen.Height * dpiScaling - window.ActualHeight) / 2 + screen.Top * dpiScaling;
        }

        public static double GetDpiFactor(Window window) => 1f / VisualTreeHelper.GetDpi(window).DpiScaleX;
    }

    public static class UIHelpers {
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

        /// <summary> Checks if a point (that is specified relative to this element) is contained within </summary>
        public static bool ContainsRelativePoint(this FrameworkElement element, Point point) =>
            point is {X: > 0, Y: > 0} &&
            point.X < element.ActualWidth && point.Y < element.ActualHeight;

        /// <summary> Checks if the mouse is contained within </summary>
        public static bool ContainsMouse(this FrameworkElement element) =>
            element.ContainsRelativePoint(Mouse.GetPosition(element));
    }

    public class EnumerationExtension : MarkupExtension {
        private readonly Type _enumType;


        public EnumerationExtension(Type enumType) =>
            EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));

        public Type EnumType {
            get => _enumType;
            private init {
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
            public string Description { [UsedImplicitly] get; init; }
            public object Value { [UsedImplicitly] get; init; }
            public override string ToString() => Description;
        }
    }

    public static class ColorUtil {
        /// <summary>
        /// Convert HSV to RGB
        /// h is from 0-360
        /// s,v values are 0-1
        /// r,g,b values are 0-255
        /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
        /// </summary>
        public static Color HsvToRgb(double h, double s, double v, int a = 255) {
            // ######################################################################
            // T. Nathan Mundhenk
            // mundhenk@usc.edu
            // C/C++ Macro HSV to RGB

            h = (h % 360 + 360) % 360;
            double r, g, b;
            if(v <= 0) r = g = b = 0;
            else if(s <= 0) r = g = b = v;
            else {
                double hf = h / 60.0;
                int i = (int) Math.Floor(hf);
                double f = hf - i;
                double pv = v * (1 - s);
                double qv = v * (1 - s * f);
                double tv = v * (1 - s * (1 - f));
                switch(i) {
                    case 0: // Red is the dominant color
                        r = v;
                        g = tv;
                        b = pv;
                        break;
                    case 1: // Green is the dominant color
                        r = qv;
                        g = v;
                        b = pv;
                        break;
                    case 2:
                        r = pv;
                        g = v;
                        b = tv;
                        break;
                    case 3: // Blue is the dominant color
                        r = pv;
                        g = qv;
                        b = v;
                        break;
                    case 4:
                        r = tv;
                        g = pv;
                        b = v;
                        break;
                    case 5: // Red is the dominant color
                        r = v;
                        g = pv;
                        b = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
                    case 6:
                        r = v;
                        g = tv;
                        b = pv;
                        break;
                    case -1:
                        r = v;
                        g = pv;
                        b = qv;
                        break;

                    default:
                        r = g = b = v; // Just pretend its black/white
                        break;
                }
            }
            return Color.FromArgb(a, Clamp(r * 255.0), Clamp(g * 255.0), Clamp(b * 255.0));
        }

        public static void ToHsv(this Color color, out double h, out double s, out double v) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            h = color.GetHue();
            s = max == 0 ? 0 : 1.0 - 1.0 * min / max;
            v = max / 255.0;
        }

        /// <summary> Clamp a value to 0-255 </summary>
        private static int Clamp(double i) => (int) (i < 0 ? 0 : i > 255 ? 255 : i);

        public static MColor ToMediaColor(this Color color) {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ToDrawingColor(this MColor color) {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color Rotate(this Color color, int degrees) {
            color.ToHsv(out double h, out double s, out double v);
            return HsvToRgb(h + degrees, s, v);
        }

        public static MColor Rotate(this MColor color, int degrees) => color.ToDrawingColor().Rotate(degrees).ToMediaColor();

        public static Color FromHex(string colorcode) {
            colorcode = colorcode.TrimStart('#');
            if(colorcode.Length == 6)
                return Color.FromArgb(255, int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber),
                                      int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber),
                                      int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber));
            return Color.FromArgb(int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber),
                                  int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber),
                                  int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber),
                                  int.Parse(colorcode.Substring(6, 2), NumberStyles.HexNumber));
        }
    }

    // ReSharper disable once UnusedType.Global
    public static class D {
        public static void Log(object o) => Debug.WriteLine(o?.ToString());
    }

    public static class ComboBoxAutoWidthBehavior {
        public static readonly DependencyProperty ComboBoxAutoWidthProperty =
            DependencyProperty.RegisterAttached(
                "ComboBoxAutoWidth",
                typeof(bool),
                typeof(ComboBoxAutoWidthBehavior),
                new UIPropertyMetadata(false, OnComboBoxAutoWidthPropertyChanged)
            );

        public static bool GetComboBoxAutoWidth(DependencyObject obj) {
            return (bool) obj.GetValue(ComboBoxAutoWidthProperty);
        }

        public static void SetComboBoxAutoWidth(DependencyObject obj, bool value) {
            obj.SetValue(ComboBoxAutoWidthProperty, value);
        }

        private static void OnComboBoxAutoWidthPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e) {
            if(dpo is ComboBox comboBox) {
                if((bool) e.NewValue) {
                    comboBox.Loaded += OnComboBoxLoaded;
                    comboBox.DropDownOpened += OnComboBoxOpened;
                    comboBox.DropDownClosed += OnComboBoxClosed;
                } else {
                    comboBox.Loaded -= OnComboBoxLoaded;
                    comboBox.DropDownOpened -= OnComboBoxOpened;
                    comboBox.DropDownClosed -= OnComboBoxClosed;
                }
            }
        }

        private static void OnComboBoxLoaded(object sender, EventArgs eventArgs) {
            ComboBox comboBox = (ComboBox) sender;
            comboBox.SetMaxWidthFromItems();
        }

        private static void OnComboBoxOpened(object sender, EventArgs eventArgs) {
            ComboBox comboBox = (ComboBox) sender;
            comboBox.Width = comboBox.MaxWidth;
        }

        private static void OnComboBoxClosed(object sender, EventArgs eventArgs) => ((ComboBox) sender).Width = double.NaN;
    }

    public static class ComboBoxExtensionMethods {
        public static void SetMaxWidthFromItems(this ComboBox combo) {
            double idealWidth = combo.MinWidth;
            string longestItem = combo.Items.Cast<object>().Select(x => x.ToString()).DefaultIfEmpty("")
                .Max(x => (x?.Length, x)).x;
            if(longestItem is {Length: > 0}) {
                string tmpTxt = combo.Text;
                combo.Text = longestItem;
                Thickness tmpMarg = combo.Margin;
                combo.Margin = new Thickness(0);
                combo.UpdateLayout();

                combo.Width = double.NaN;
                combo.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                idealWidth = Math.Max(idealWidth, combo.DesiredSize.Width);

                combo.Text = tmpTxt;
                combo.Margin = tmpMarg;
            }

            combo.MaxWidth = idealWidth;
        }
    }
}
