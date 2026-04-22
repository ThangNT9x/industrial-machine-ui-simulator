
using System.Windows;
using IndustrialMachineSimulator.UI.ViewModels;

namespace IndustrialMachineSimulator.UI.Views
{
    /// <summary>
    /// Interaction logic for SimulatorControlWindow.xaml
    /// </summary>
    public partial class SimulatorControlWindow : Window
    {
        public SimulatorControlWindow( MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
