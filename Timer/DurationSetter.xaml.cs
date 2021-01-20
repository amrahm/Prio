using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Infrastructure.SharedResources;

namespace Timer {
    /// <summary> Interaction logic for DurationSetter.xaml </summary>
    public partial class DurationSetter : INotifyPropertyWithDependencies {
        public event PropertyChangedEventHandler PropertyChanged;
        public PropertyChangedEventHandler GetPropertyChangedEventHandler => PropertyChanged;
        public Dictionary<string, List<string>> DependencyMap { get; set; }

        #region Duration DP

        /// <summary> Gets or sets the Duration </summary>
        public TimeSpan Duration {
            get => (TimeSpan) GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        /// <summary> Identifies the Duration dependency property </summary>
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
            nameof(Duration), typeof(TimeSpan), typeof(DurationSetter),
            new PropertyMetadata(TimeSpan.Zero, DurationPropertyChange));

        private static void DurationPropertyChange(DependencyObject dpo, DependencyPropertyChangedEventArgs args) =>
                ((DurationSetter) dpo).OnPropertyChanged(nameof(Duration));

        #endregion

        #region HMS DP

        /// <summary> Determines if colons or h,m,s is shown </summary>
        public bool ShowHms {
            get => (bool) GetValue(ShowHmsProperty);
            set => SetValue(ShowHmsProperty, value);
        }

        /// <summary> Identifies the Duration dependency property </summary>
        public static readonly DependencyProperty ShowHmsProperty = DependencyProperty.Register(
            nameof(ShowHms), typeof(bool), typeof(DurationSetter),
            new PropertyMetadata(false));

        #endregion

        [DependsOnProperty(nameof(Duration))]
        public int Hours {
            get => (int) Duration.TotalHours;
            set {
                Duration = new TimeSpan(value, Minutes, Seconds);
                if(Math.Abs(Duration.TotalHours) < 0.0001) Minutes = 1;
            }
        }

        [DependsOnProperty(nameof(Duration))]
        public int Minutes {
            get => Duration.Minutes;
            set => Duration = new TimeSpan(Hours, value, Seconds);
        }

        [DependsOnProperty(nameof(Duration))]
        public int Seconds {
            get => Duration.Seconds;
            set => Duration = new TimeSpan(Hours, Minutes, value);
        }

        public DurationSetter() {
            this.InitializeDependencyMap();
            InitializeComponent();
        }
    }
}