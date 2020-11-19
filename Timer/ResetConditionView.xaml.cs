using System;
using System.Windows;
using Infrastructure.SharedResources;

namespace Timer {
    /// <summary> Interaction logic for ResetConditionView.xaml </summary>
    public partial class ResetConditionView  {
        public ResetConditionView() {
            void ToggleOptions(ResetConditionViewModel resetConditionViewModel) {
                switch(resetConditionViewModel.Type) {
                    case ResetConditionType.Cooldown:
                        CooldownOptions.Visibility = Visibility.Visible;
                        DependencyOptions.Visibility = Visibility.Collapsed;
                        break;
                    case ResetConditionType.Dependency:
                        DependencyOptions.Visibility = Visibility.Visible;
                        CooldownOptions.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            InitializeComponent();
            VirtualDesktopExtensions.EnforceIntList(OffDesktops);

            ResetConditionViewModel vm = (ResetConditionViewModel) DataContext;
            ToggleOptions(vm);
            vm.PropertyChanged += (o,  e) => {
                if(e.PropertyName == nameof(vm.Type)) ToggleOptions(vm);
            };
        }
    }
}