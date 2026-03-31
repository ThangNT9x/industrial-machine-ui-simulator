using System.Windows;
using IndustrialMachineSimulator.UI.ViewModels;
using IndustrialMachineSimulator.UI.Controls;

namespace IndustrialMachineSimulator.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}