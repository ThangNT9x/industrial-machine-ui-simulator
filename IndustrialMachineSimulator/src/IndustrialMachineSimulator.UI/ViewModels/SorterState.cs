using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IndustrialMachineSimulator.UI.ViewModels;

public sealed class SorterState : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private bool _isInConveyorRunning;
    public bool IsInConveyorRunning
    {
        get => _isInConveyorRunning;
        set => SetProperty(ref _isInConveyorRunning, value);
    }

    private bool _isFeed1Running;
    public bool IsFeed1Running
    {
        get => _isFeed1Running;
        set => SetProperty(ref _isFeed1Running, value);
    }

    private bool _isFeed2Running;
    public bool IsFeed2Running
    {
        get => _isFeed2Running;
        set => SetProperty(ref _isFeed2Running, value);
    }

    private bool _isFeed3Running;
    public bool IsFeed3Running
    {
        get => _isFeed3Running;
        set => SetProperty(ref _isFeed3Running, value);
    }

    private bool _isOutConveyorRunning;
    public bool IsOutConveyorRunning
    {
        get => _isOutConveyorRunning;
        set => SetProperty(ref _isOutConveyorRunning, value);
    }

    private bool _isInConveyorSensorOn;
    public bool IsInConveyorSensorOn
    {
        get => _isInConveyorSensorOn;
        set => SetProperty(ref _isInConveyorSensorOn, value);
    }

    private bool _isFeed1SensorOn;
    public bool IsFeed1SensorOn
    {
        get => _isFeed1SensorOn;
        set => SetProperty(ref _isFeed1SensorOn, value);
    }

    private bool _isFeed2SensorOn;
    public bool IsFeed2SensorOn
    {
        get => _isFeed2SensorOn;
        set => SetProperty(ref _isFeed2SensorOn, value);
    }

    private bool _isFeed3SensorOn;
    public bool IsFeed3SensorOn
    {
        get => _isFeed3SensorOn;
        set => SetProperty(ref _isFeed3SensorOn, value);
    }

    private bool _isOutConveyorSensorOn;
    public bool IsOutConveyorSensorOn
    {
        get => _isOutConveyorSensorOn;
        set => SetProperty(ref _isOutConveyorSensorOn, value);
    }

    private bool _isInLiftRunning;
    public bool IsInLiftRunning
    {
        get => _isInLiftRunning;
        set => SetProperty(ref _isInLiftRunning, value);
    }

    private bool _isOutLiftRunning;
    public bool IsOutLiftRunning
    {
        get => _isOutLiftRunning;
        set => SetProperty(ref _isOutLiftRunning, value);
    }

    private bool _isNgConveyorRunning;
    public bool IsNgConveyorRunning
    {
        get => _isNgConveyorRunning;
        set => SetProperty(ref _isNgConveyorRunning, value);
    }

    private int _currentTrayOkCount;
    public int CurrentTrayOkCount
    {
        get => _currentTrayOkCount;
        set => SetProperty(ref _currentTrayOkCount, value);
    }

    private int _trayCapacity = 20;
    public int TrayCapacity
    {
        get => _trayCapacity;
        set => SetProperty(ref _trayCapacity, value);
    }

    private int _availableEmptyTrayCount = 100;
    public int AvailableEmptyTrayCount
    {
        get => _availableEmptyTrayCount;
        set => SetProperty(ref _availableEmptyTrayCount, value);
    }

    private int _producedFullTrayCount;
    public int ProducedFullTrayCount
    {
        get => _producedFullTrayCount;
        set => SetProperty(ref _producedFullTrayCount, value);
    }

    private bool _hasEmptyTrayLoaded = true;
    public bool HasEmptyTrayLoaded
    {
        get => _hasEmptyTrayLoaded;
        set => SetProperty(ref _hasEmptyTrayLoaded, value);
    }

    private bool _isStageMaterialOkLampOn;
    public bool IsStageMaterialOkLampOn
    {
        get => _isStageMaterialOkLampOn;
        set => SetProperty(ref _isStageMaterialOkLampOn, value);
    }

    private bool _isStageMaterialNgLampOn;
    public bool IsStageMaterialNgLampOn
    {
        get => _isStageMaterialNgLampOn;
        set => SetProperty(ref _isStageMaterialNgLampOn, value);
    }

    private bool _isInputStopActive;
    public bool IsInputStopActive
    {
        get => _isInputStopActive;
        set => SetProperty(ref _isInputStopActive, value);
    }

    private bool _isOutTrayBoxRequested;
    public bool IsOutTrayBoxRequested
    {
        get => _isOutTrayBoxRequested;
        set => SetProperty(ref _isOutTrayBoxRequested, value);
    }

    private bool _isFeed1StopperUp = true;
    public bool IsFeed1StopperUp
    {
        get => _isFeed1StopperUp;
        set
        {
            if (SetProperty(ref _isFeed1StopperUp, value))
            {
                OnPropertyChanged(nameof(IsFeed1StopperDown));
            }
        }
    }

    public bool IsFeed1StopperDown => !IsFeed1StopperUp;
}