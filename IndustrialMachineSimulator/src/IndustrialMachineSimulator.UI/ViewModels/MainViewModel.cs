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

    public string OperatorName { get; set; } = "Operator";
    public string EqpId { get; set; } = "Laser Router 01";
    public string LineName { get; set; } = "TEST";
    public string OsVersion { get; set; } = "0.0.1";
    public int PcbOkCount { get; set; } = 0;
    public int PbaOkCount { get; set; } = 0;
    public int PbaNgCount { get; set; } = 0;

    public string LaserTimeText { get; set; } = "0";
    public string TemperatureText { get; set; } = "0";
    public string HostStatusText {  get; set; } = "Connected";
    public string LaserStatusText { get; set;  } = "Ready";

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