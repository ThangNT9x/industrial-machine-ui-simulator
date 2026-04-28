using System.Windows;
using IndustrialMachineSimulator.UI.ViewModels;

namespace IndustrialMachineSimulator.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, _) =>
        {
            await vm.LoadAlarmHistoryAsync();
        };
    }
}