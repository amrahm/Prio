using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Infrastructure.SharedResources {
    public class TextBoxExtensions {
        public static bool GetEnforceInt(DependencyObject obj) => (bool) obj.GetValue(EnforceIntProperty);
        public static void SetEnforceInt(DependencyObject obj, bool value) => obj.SetValue(EnforceIntProperty, value);

        public static readonly DependencyProperty EnforceIntProperty =
                DependencyProperty.RegisterAttached(nameof(EnforceInt), typeof(bool), typeof(TextBoxExtensions),
                                                    new PropertyMetadata(false, EnforceInt));

        private static readonly Regex IntRx = new(@"[^\d]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static void EnforceInt(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is not TextBox textBox) return;
            textBox.TextChanged += (_,  _) => {
                int oldIndex = textBox.CaretIndex;
                string oldValue = textBox.Text;
                string validInput = IntRx.Replace(textBox.Text, "");
                textBox.Text = validInput;

                if(!oldValue.Equals(validInput)) textBox.CaretIndex = oldIndex == 0 ? 0 : oldIndex - 1;
            };
        }

        public static bool GetEnforceIntList(DependencyObject obj) => (bool) obj.GetValue(EnforceIntListProperty);
        public static void SetEnforceIntList(DependencyObject obj, bool value) =>
                obj.SetValue(EnforceIntListProperty, value);

        public static readonly DependencyProperty EnforceIntListProperty =
                DependencyProperty.RegisterAttached(nameof(EnforceIntList), typeof(bool), typeof(TextBoxExtensions),
                                                    new PropertyMetadata(false, EnforceIntList));

        private static readonly Regex IntListRx = new(@"[^\d,\s]|((?<=,\s),\s?)|(?<!\d),|(?<!,)\s|(?<=\d{2})\d",
                                                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static void EnforceIntList(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is not TextBox textBox) return;
            textBox.TextChanged += (_,  _) => {
                int oldIndex = textBox.CaretIndex;
                string oldValue = textBox.Text;
                string validInput = IntListRx.Replace(textBox.Text, "");
                textBox.Text = validInput;

                if(!oldValue.Equals(validInput)) textBox.CaretIndex = oldIndex == 0 ? 0 : oldIndex - 1;
            };
        }

        public static bool GetEnforceDouble(DependencyObject obj) => (bool) obj.GetValue(EnforceDoubleProperty);
        public static void SetEnforceDouble(DependencyObject obj, bool value) => obj.SetValue(EnforceDoubleProperty, value);

        public static readonly DependencyProperty EnforceDoubleProperty =
                DependencyProperty.RegisterAttached(nameof(EnforceDouble), typeof(bool), typeof(TextBoxExtensions),
                                                    new PropertyMetadata(false, EnforceDouble));

        private static readonly Regex DoubleRx = new(@"[^\d.]|(?=<\d*\.\d*)\.",
                                                     RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static void EnforceDouble(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if(d is not TextBox textBox) return;
            textBox.TextChanged += (_,  _) => {
                int oldIndex = textBox.CaretIndex;
                string oldValue = textBox.Text;
                string validInput = DoubleRx.Replace(textBox.Text, "");
                textBox.Text = validInput;

                if(!oldValue.Equals(validInput)) textBox.CaretIndex = oldIndex == 0 ? 0 : oldIndex - 1;
            };
        }
    }
}