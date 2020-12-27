using System;
using System.Windows;
using Infrastructure.SharedResources;

namespace Timer {
    /// <summary> Interaction logic for ResetConditionView.xaml </summary>
    public partial class ResetConditionView  {
        public ResetConditionView(ResetConditionViewModel vm) {
            DataContext = vm;
            InitializeComponent();

            VirtualDesktopExtensions.EnforceIntList(OffDesktops);

            void ToggleOptions(ResetCondition resetConditionModel) {
                switch(resetConditionModel.Type) {
                    case ResetConditionType.Cooldown:
                        CooldownOptions.Visibility = Visibility.Visible;
                        DependencyOptions.Visibility = Visibility.Collapsed;
                        break;
                    case ResetConditionType.Dependency:
                        DependencyOptions.Visibility = Visibility.Visible;
                        CooldownOptions.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resetConditionModel));
                }
            }

            ToggleOptions(vm.Model);
            vm.PropertyChanged += (o,  e) => {
                if(e.PropertyName == nameof(vm.Model)) ToggleOptions(vm.Model);
            };
        }
    }
}