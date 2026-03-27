using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using IndustrialMachineSimulator.Core.Services;

namespace IndustrialMachineSimulator.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly MachineController _machineController;
    private string _statusText = "Offline";

    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public ICommand InitializeCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    public MainViewModel(MachineController machineController)
    {
        _machineController = machineController;

        InitializeCommand = new RelayCommand(async _ =>
        {
            await _machineController.InitializeAsync();
            StatusText = _machineController.Status.State.ToString();
        });

        StartCommand = new RelayCommand(async _ =>
        {
            await _machineController.StartAsync();
            StatusText = _machineController.Status.State.ToString();
        });

        StopCommand = new RelayCommand(async _ =>
        {
            await _machineController.StopAsync();
            StatusText = _machineController.Status.State.ToString();
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}