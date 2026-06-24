using System.Windows;

namespace Simulator
{
    public partial class MainWindow : Window
    {
        private readonly PumpSystemViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new PumpSystemViewModel();
            DataContext = _vm;
            Loaded += (s, e) => _vm.Start();
        }

        private void OnTogglePower(object sender, RoutedEventArgs e)
        {
            _vm.ToggleSystemPower();
        }

        private void OnStopAll(object sender, RoutedEventArgs e)
        {
            _vm.StopAllPumps();
        }

        private void OnToggleB2(object sender, RoutedEventArgs e)
        {
            _vm.ToggleB2Fault();
        }

        private void OnTogglePump(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is int idx)
            {
                _vm.ToggleAvailability(idx);
            }
        }
    }
}
