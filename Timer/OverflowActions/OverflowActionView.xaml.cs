namespace Timer {
    /// <summary> Interaction logic for ResetConditionView.xaml </summary>
    public partial class OverflowActionView  {
        public OverflowActionView(OverflowActionViewModel vm) {
            DataContext = vm;
            InitializeComponent();
        }
    }
}