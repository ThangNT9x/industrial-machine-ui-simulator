using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;
using IndustrialMachineSimulator.Core.Services;
using IndustrialMachineSimulator.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;
using System.Globalization;

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
    public enum UserRole
    {
        Operator,
        Engineer,
        Developer
    }
    private UserRole _currentRole=UserRole.Operator;
    public UserRole CurrentRole
    {
        get => _currentRole;
        set
        {
            _currentRole = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentRoleText));
            ApplyRolePermissions();
        }
    }

    public ICommand InitializeCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand CycleStopCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand RunStatusBarCommand { get; }
    public bool IsRunButtonInStopMode=>CurrentMachineState==MachineState.Running;
    public ICommand LoginCommand { get; }
    public ICommand ShowHomeCommand { get;  }
    public ICommand ShowMaintCommand { get; }
    public ICommand ShowRecipeCommand { get; }
    public ICommand ShowDatalogCommand { get; }
    public ICommand ShowSetupCommand { get; }
    public ICommand ShowAlarmsCommand { get; }
    public ICommand ShowListCommand { get; }
    public ICommand ShowQuickCommand { get; }
    public ICommand ShowVisionCommand { get; }
    public ICommand ShowErrorCommand { get; }
    public ICommand ShowMESCommand { get; }
    public ICommand ShowPowerCommand { get; }

    public ICommand OpenSimulatorWindowCommand {  get; }

    public ICommand ClearAlarmUiCommand { get; }

    public ICommand ClearOperationUiCommand { get; }

    public ICommand SaveConfigCommand { get; }
    public ICommand CancelConfigEditCommand { get; }
    public ICommand ReloadConfigCommand { get; }

    public string CurrentRoleText => CurrentRole.ToString();
    private MachineState _currentMachineState = MachineState.Offline;
    public MachineState CurrentMachineState
    {
        get => _currentMachineState;
        set
        {
            if(_currentMachineState == value)return;
            _currentMachineState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentMachineStateText));
            OnPropertyChanged(nameof(IsHostGreen));
            OnPropertyChanged(nameof(IsLaserGreen));
            OnPropertyChanged(nameof(IsDiodeGreen));
            OnPropertyChanged(nameof(IsPulsingGreen));
            OnPropertyChanged(nameof(IsAlarmState));
            OnPropertyChanged(nameof(PowerMachineStatusText));
            OnPropertyChanged(nameof(IsTowerRedOn));
            OnPropertyChanged(nameof(IsTowerYellowOn));
            OnPropertyChanged(nameof(IsTowerGreenOn));
            OnPropertyChanged(nameof(IsRunButtonInStopMode));
            UpdateMachineState();
            NotifySetupStateChanged();
        }
    }
    public string CurrentMachineStateText=>CurrentMachineState.ToString();
    private string _osVersion = "0.0.1";
    public string OsVersion
    {
        get => _osVersion;
        set
        {
            _osVersion = value;
            OnPropertyChanged();
        }
    }

    private string _laserTimeText = "2000 / 20000H (100 %)";
    public string LaserTimeText
    {
        get => _laserTimeText;
        set
        {
            _laserTimeText = value;
            OnPropertyChanged();

        }
    }
    private string _appTitle = "SM_S928_ROUTER_LASER_SIMULATOR";
    public string AppTitle
    {
        get => _appTitle;
        set
        {
            _appTitle = value;
            OnPropertyChanged();
        }
    }
    public string EngineerPassword => _machineConfig.EngineerPassword;
    public string DeveloperPassword => _machineConfig.DeveloperPassword;

    private string _engineerPasswordValue = string.Empty;
    public string EngineerPasswordValue
    {
        get => _engineerPasswordValue;
        set
        {
            _engineerPasswordValue = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
        }
    }

    private string _developerPasswordValue = string.Empty;
    public string DeveloperPasswordValue
    {
        get => _developerPasswordValue;
        set
        {
            _developerPasswordValue = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
        }
    }
    private string _editAppTitle = string.Empty;
    public string EditAppTitle
    {
        get => _editAppTitle;
        set
        {
            _editAppTitle = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
        }
    }

    private string _editOsVersion = string.Empty;
    public string EditOsVersion
    {
        get => _editOsVersion;
        set
        {
            _editOsVersion = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
        }
    }

    private string _editLaserTimeText = string.Empty;
    public string EditLaserTimeText
    {
        get => _editLaserTimeText;
        set
        {
            _editLaserTimeText = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
        }
    }
    private CancellationTokenSource? _cycleCts;
    private readonly Random _random = new();
    private bool _isCycleLoopRunning;

    private bool _isPowerMachineOn;
    public bool IsPowerMachineOn
    {
        get => _isPowerMachineOn;
        set
        {
            if(_isPowerMachineOn == value) return;
            bool wasOn = _isPowerMachineOn;
            _isPowerMachineOn = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostGreen));
            OnPropertyChanged(nameof(IsLaserGreen));
            OnPropertyChanged(nameof(IsDiodeGreen));
            OnPropertyChanged(nameof(IsPulsingGreen));
            OnPropertyChanged(nameof(PowerMachineStatusText));
            OnPropertyChanged(nameof(IsAlarmState));
            OnPropertyChanged(nameof(IsPowerOffOverlayVisible));
            if(wasOn &&!value)
            {
                StopCycleLoop();
                if(CurrentMachineState==MachineState.Running||
                    CurrentMachineState==MachineState.Initializing)
                {
                    _ = EnterAlarmAsync("POWER-001", "Power machine turned off during operation.");
                    _ = AddOperationLogAsync("Power", "Power machine turned off.");
                    return;
                }
            }
            if (!wasOn && value)
            {
                _ = AddOperationLogAsync("Power", "Power machine turned on.");
            }
            ApplySimulatorSignals();
        }
    }

    private int _cycleIntervalMs = 3000;
    public int CycleIntervalMs
    {
        get => _cycleIntervalMs;
        set
        {
            _cycleIntervalMs = value;
            OnPropertyChanged();
        }
    }

    private double _cycleOkRate = 0.8;
    public double CycleOkRate
    {
        get => _cycleOkRate;
        set
        {
            _cycleOkRate = value;
            OnPropertyChanged();
        }
    }
    private string _editCycleIntervalMsText = "3000";
    public string EditCycleIntervalMsText
    {
        get => _editCycleIntervalMsText;
        set
        {
            _editCycleIntervalMsText = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
        }
    }

    private string _editCycleOkRateText = "0.8";
    public string EditCycleOkRateText
    {
        get => _editCycleOkRateText;
        set
        {
            _editCycleOkRateText = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
        }
    }
    public bool IsSetupEditLocked => CurrentMachineState == MachineState.Running;

    public bool IsAppTitleValid => !string.IsNullOrWhiteSpace(EditAppTitle);
    public bool IsOsVersionValid => !string.IsNullOrWhiteSpace(EditOsVersion);
    public bool IsLaserTimeValid => !string.IsNullOrWhiteSpace(EditLaserTimeText);
    public bool IsEngineerPasswordValid => !string.IsNullOrWhiteSpace(EngineerPasswordValue);
    public bool IsDeveloperPasswordValid => !string.IsNullOrWhiteSpace(DeveloperPasswordValue);

    public bool IsCycleIntervalValid =>
        int.TryParse(EditCycleIntervalMsText, out var ms) && ms > 0;

    public bool IsCycleOkRateValid =>
        double.TryParse(EditCycleOkRateText, NumberStyles.Float, CultureInfo.InvariantCulture, out var rate)
        && rate >= 0 && rate <= 1;

    public bool IsSetupInputValid =>
        IsAppTitleValid &&
        IsOsVersionValid &&
        IsLaserTimeValid &&
        IsEngineerPasswordValid &&
        IsDeveloperPasswordValid &&
        IsCycleIntervalValid &&
        IsCycleOkRateValid;

    public bool CanSaveSetup => !IsSetupEditLocked && IsSetupInputValid;

    public bool HasSetupValidationError => !IsSetupInputValid;
    public bool IsPowerOffOverlayVisible => !IsPowerMachineOn;


    private bool _isAlarmOn;
    public bool IsAlarmOn
    {
        get => _isAlarmOn;
        set
        {
            if (_isAlarmOn == value) return;
            if (value)
            {
                _ = EnterAlarmAsync("SIMU-ALARM-001", "Manual alarm trigger from Simulator", false);
                return;
            }
            _isAlarmOn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostGreen));
            OnPropertyChanged(nameof(IsLaserGreen));
            OnPropertyChanged(nameof(IsDiodeGreen));
            OnPropertyChanged(nameof(IsPulsingGreen));
            OnPropertyChanged(nameof(PowerMachineStatusText));
            OnPropertyChanged(nameof(AlarmStatusText));
            OnPropertyChanged(nameof(IsAlarmState));
            ApplySimulatorSignals();
        }
    }
    private async Task EnterAlarmAsync(string code, string message, bool showPopup = true)
    {
        StopCycleLoop();
        if (!_isAlarmOn)
        {
            _isAlarmOn = true;
            OnPropertyChanged(nameof(IsAlarmOn));
            OnPropertyChanged(nameof(IsAlarmState));
            OnPropertyChanged(nameof(AlarmStatusText));
        }

        if (CurrentMachineState != MachineState.Alarm)
        {
            CurrentMachineState = MachineState.Alarm;
        }

        await AddAlarmAsync(code, message);

        if (showPopup)
        {
            MessageBox.Show(message);
        }
    }
    private async Task TryStartMachine()
    {
        if (!IsPowerMachineOn)
        {
            MessageBox.Show("Power Machine is Off.");
            return;
        }

        if (IsAlarmOn || CurrentMachineState == MachineState.Alarm)
        {
            MessageBox.Show("Machine is alarm state. Please reset first!");
            return;
        }

        if (!IsLaserOn)
        {
            MessageBox.Show("Laser is Off.");
            return;
        }

        if (!IsFrontDoorClosed || !IsRearDoorClosed)
        {
            MessageBox.Show("Door is open. Cannot start.");
            return;
        }

        if (CurrentMachineState == MachineState.Ready ||
            CurrentMachineState == MachineState.Stopped)
        {
            CurrentMachineState = MachineState.Running;
            NavigateToHome();
            StartCycleLoop();
            await AddOperationLogAsync("Run", "Machine started running.");
        }
  
    }

    private async Task TryStopMachine()
    {
        if (CurrentMachineState == MachineState.Running)
        {
            NavigateToHome();
            StopCycleLoop();
            CurrentMachineState = MachineState.Stopped;
            await AddOperationLogAsync("Run", "Machine stopped.");
        }
    }
   
    private string _hostStatusText = "Disconnected";
    public string HostStatusText
    {
        get=> _hostStatusText;
        set
        {
            _hostStatusText = value;
            OnPropertyChanged();
        }
        
    }
    private string _laserStatusText = "Offline";
    public string LaserStatusText
    {
        get=> _laserStatusText;
        set
        {
            _laserStatusText = value;
            OnPropertyChanged();
        }
    }
    private string _diodeStatusText = "Off";
    public string DiodeStatusText
    {
        get => _diodeStatusText;
        set
        {
            _diodeStatusText= value;
            OnPropertyChanged();
        }
    }
    private string _frontDoorStatusText = "Closed";
    public string FrontDoorStatusText
    {
        get => _frontDoorStatusText;
        set
        {
            _frontDoorStatusText= value;
            OnPropertyChanged();
        }
    }
    public string _rearDoorStatusText = "Closed";
    public string RearDoorStatusText
    {
        get => _rearDoorStatusText;
        set
        {
            _rearDoorStatusText= value;
            OnPropertyChanged();
        }
    }
    private string _pulsingStatusText = "Off";
    public string PulsingStatusText
    {
        get => _pulsingStatusText;
        set
        {
            _pulsingStatusText = value;
            OnPropertyChanged();
        }
    }
    private async Task AddOperationLogAsync(string category, string message)
    {
        var record = new OperationLogRecord
        {
            Timestamp = DateTime.Now,
            Category = category,
            Message = message
        };
        OperationItems.Insert(0, record);

        await _operationLogRepository.AddAsync(record);
        await _operationFileLogger.WriteAsync(record);
    }
    public ObservableCollection<OperationLogRecord> OperationItems { get; } = new();
    public async Task LoadOperationHistoryAsync()
    {
        OperationItems.Clear();
        var items = await _operationLogRepository.GetVisibleAsync();
        foreach (var item in items)
        {
            OperationItems.Add(item);
        }
    }

    private readonly IOperationFileLogger _operationFileLogger;
    public bool IsTowerRedOn => CurrentMachineState == MachineState.Alarm;
    public bool IsTowerYellowOn =>
        CurrentMachineState == MachineState.Standby ||
        CurrentMachineState == MachineState.Initializing ||
        CurrentMachineState == MachineState.Stopped||
        CurrentMachineState==MachineState.Ready;
    public bool IsTowerGreenOn =>CurrentMachineState == MachineState.Running;
    public string PowerMachineStatusText => IsPowerMachineOn ? "On" : "Off";
    public string AlarmStatusText => IsAlarmOn ? "On" : "Off";

    public string MachineStateStatusText => CurrentMachineState.ToString();
    public ObservableCollection<AlarmRecord> AlarmItems { get; } = new();

    private readonly IAlarmFileLogger _alarmFileLogger;

    private readonly IAlarmRepository _alarmRepository;

    private readonly IOperationLogRepository _operationLogRepository;

    private readonly IConfigService _configService;

    private readonly MachineConfig _machineConfig;


    private bool _isLaserOn;
    public bool IsLaserOn
    {
        get => _isLaserOn;
        set
        {
            _isLaserOn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostGreen));
            OnPropertyChanged(nameof(IsLaserGreen));
            OnPropertyChanged(nameof(IsDiodeGreen));
            OnPropertyChanged(nameof(IsPulsingGreen));

            OnPropertyChanged(nameof(IsAlarmState));
            ApplySimulatorSignals();
        }
    }
    private bool _isDiodeOn;
    public bool IsDiodeOn
    {
        get => _isDiodeOn;
        set
        {
            _isDiodeOn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDiodeGreen));
            ApplySimulatorSignals();
        }
    }
    private bool _isPulsingOn;
    public bool IsPulsingOn
    {
        get => _isPulsingOn;
        set
        {
            _isPulsingOn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPulsingGreen));
            ApplySimulatorSignals();
        }
    }
    private bool _isFrontDoorClosed;
    public bool IsFrontDoorClosed
    {
        get => _isFrontDoorClosed;
        set
        {
            if (_isFrontDoorClosed == value) return;

            bool wasClosed = _isFrontDoorClosed;
            _isFrontDoorClosed = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostGreen));
            OnPropertyChanged(nameof(IsLaserGreen));
            OnPropertyChanged(nameof(IsDiodeGreen));
            OnPropertyChanged(nameof(IsPulsingGreen));
            OnPropertyChanged(nameof(IsAlarmState));
            OnPropertyChanged(nameof(IsFrontDoorGreen));

            if (wasClosed && !value)
            {
                IsFrontDoorSafetyReset = false;

                if (CurrentMachineState == MachineState.Running ||
                    CurrentMachineState == MachineState.Initializing)
                {
                    _ = EnterAlarmAsync("DOOR-001","Front door opened during operation.");
                }
            }

            ApplySimulatorSignals();
        }
    }
    private bool _isRearDoorClosed;
    public bool IsRearDoorClosed
    {
        get => _isRearDoorClosed;
        set
        {
            if (_isRearDoorClosed == value) return;

            bool wasClosed = _isRearDoorClosed;
            _isRearDoorClosed = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHostGreen));
            OnPropertyChanged(nameof(IsLaserGreen));
            OnPropertyChanged(nameof(IsDiodeGreen));
            OnPropertyChanged(nameof(IsPulsingGreen));
            OnPropertyChanged(nameof(IsAlarmState));
            OnPropertyChanged(nameof(IsRearDoorGreen));

            if (wasClosed && !value)
            {
                IsRearDoorSafetyReset = false;

                if (CurrentMachineState == MachineState.Running ||
                    CurrentMachineState == MachineState.Initializing)
                {
                    _ = EnterAlarmAsync("DOOR-002","Rear door opened during operation.");
                }
            }

            ApplySimulatorSignals();
        }
    }

    private bool _isFrontDoorSafetyReset;
    public bool IsFrontDoorSafetyReset
    {
        get => _isFrontDoorSafetyReset;
        set
        {
            if (_isFrontDoorSafetyReset == value) return;
            _isFrontDoorSafetyReset = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFrontDoorGreen));
        }
    }

    private bool _isRearDoorSafetyReset;
    public bool IsRearDoorSafetyReset
    {
        get => _isRearDoorSafetyReset;
        set
        {
            if (_isRearDoorSafetyReset == value) return;
            _isRearDoorSafetyReset = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsRearDoorGreen));
        }
    }
    private Views.SimulatorControlWindow? _simulatorWindow;

    public MainViewModel(MachineController machineController,
        IAlarmRepository alarmRepository, 
        IAlarmFileLogger alarmFileLogger,
        IOperationLogRepository operationLogRepository,
        IOperationFileLogger operationFileLogger,
        IConfigService configService)
    {
        _machineController = machineController;
        _alarmRepository = alarmRepository;
        _alarmFileLogger = alarmFileLogger;
        _operationLogRepository = operationLogRepository;
        _operationFileLogger=operationFileLogger;
        _configService = configService;
        _machineConfig = _configService.Load();
        AppTitle = _machineConfig.AppTitle;
        OsVersion = _machineConfig.OsVersion;
        LaserTimeText = _machineConfig.LaserTimeText;
        CycleIntervalMs = _machineConfig.CycleIntervalMs;
        CycleOkRate = _machineConfig.CycleOkRate;
        LoadEditorFromCurrentValues();


        InitializeCommand = new RelayCommand(async _ =>
        {
            await AddOperationLogAsync("Init", "Initialization started.");
            if (!IsPowerMachineOn)
            {
                MessageBox.Show("Power Machine is Off.");
                return;
            }

            if (IsAlarmOn || CurrentMachineState == MachineState.Alarm)
            {
                MessageBox.Show("Machine is alarm state. Please reset first!");
                return;
            }

            if (!IsFrontDoorClosed || !IsRearDoorClosed)
            {
                await EnterAlarmAsync("DOOR-001", "Door is open. Cannot initialize.");
                return;
            }

            if (CurrentMachineState == MachineState.Running)
            {
                MessageBox.Show("Machine is running. Stop the machine before init.");
                return;
            }

            if (CurrentMachineState == MachineState.Initializing)
            {
                MessageBox.Show("Machine is already initializing.");
                return;
            }

            CurrentMachineState = MachineState.Initializing;
            await Task.Delay(800);

            if (!IsFrontDoorClosed || !IsRearDoorClosed)
            {
                await EnterAlarmAsync("DOOR-001", "Door opened during initialization.");
                return;
            }

            IsFrontDoorSafetyReset = IsFrontDoorClosed;
            IsRearDoorSafetyReset = IsRearDoorClosed;
            HasCompletedInitialInit = true;
            CurrentMachineState = MachineState.Ready;
            await AddOperationLogAsync("Init", "Initialization completed successfully.");
        });

        OpenSimulatorWindowCommand = new RelayCommand(_ =>
        {
            if (_simulatorWindow == null || !_simulatorWindow.IsLoaded) 
            {
                _simulatorWindow = new Views.SimulatorControlWindow(this);
                if(Application.Current.MainWindow != null)
                {
                    _simulatorWindow.Owner = Application.Current.MainWindow;
                }
                _simulatorWindow.Closed += (_, _) => _simulatorWindow = null;
                _simulatorWindow.Show();
            }
            else
            {
                if(_simulatorWindow.WindowState==WindowState.Minimized)
                {
                    _simulatorWindow.WindowState= WindowState.Normal;
                }
                _simulatorWindow.Activate();
                _simulatorWindow.Topmost = true;
                _simulatorWindow.Topmost = false;
                _simulatorWindow.Focus();

            }
            return Task.CompletedTask;
        });

        StartCommand = new RelayCommand(_ => TryStartMachine());

        StopCommand = new RelayCommand(_ => TryStopMachine());

        RunStatusBarCommand = new RelayCommand(_ =>
        {
            if (CurrentMachineState == MachineState.Running)
            {
                return TryStopMachine();
            }

            return TryStartMachine();
        });
        CycleStopCommand = new RelayCommand(async _ =>
        {
            if (CurrentMachineState == MachineState.Running)
            {
                NavigateToHome();
                StopCycleLoop();
                CurrentMachineState = MachineState.Stopped;
                await AddOperationLogAsync("Run", "Machine cycle-stopped.");
            }
        });
        ResetCommand = new RelayCommand(async _ =>
        {
            if (!IsPowerMachineOn)
            {
                MessageBox.Show("Power Machine is Off.");
                return;
            }
            await AddOperationLogAsync("Reset", "Machine reset command executed.");
       
            IsAlarmOn = false;
            IsFrontDoorSafetyReset = IsFrontDoorClosed;
            IsRearDoorSafetyReset = IsRearDoorClosed;

            if (!IsFrontDoorClosed || !IsRearDoorClosed)
            {
                if (!IsFrontDoorClosed && !IsRearDoorClosed)
                {
                    MessageBox.Show("Front door and Rear door are still open.");
                }
                else if (!IsFrontDoorClosed)
                {
                    MessageBox.Show("Front door is still open.");
                }
                else if (!IsRearDoorClosed)
                {
                    MessageBox.Show("Rear door is still open.");
                }
            }
            if (!HasCompletedInitialInit)
            {
                CurrentMachineState = MachineState.Standby;
                NavigateToHome();
                return;
            }
            if (CurrentMachineState==MachineState.Stopped||
            CurrentMachineState==MachineState.Alarm)
            {
                CurrentMachineState = MachineState.Ready;
                NavigateToHome();

            }
            
        });
        ClearAlarmUiCommand = new RelayCommand(async _ =>
        {
            await _alarmRepository.ClearVisibleAsync();
            AlarmItems.Clear();
            
        });
        ClearOperationUiCommand = new RelayCommand(async _ =>
        {
            await _operationLogRepository.ClearVisibleAsync();
            OperationItems.Clear();
        });
        SaveConfigCommand = new RelayCommand(_ =>
        {
            if (IsSetupEditLocked)
            {
                MessageBox.Show("Machine is running. Stop the machine before saving setup.");
                return Task.CompletedTask;
            }

            if (!IsSetupInputValid)
            {
                MessageBox.Show("Invalid setup values. Please check highlighted fields.");
                return Task.CompletedTask;
            }

            SaveConfig();
            MessageBox.Show("Configuration saved.");
            return Task.CompletedTask;
        });
        CancelConfigEditCommand = new RelayCommand(_ =>
        {
            LoadEditorFromCurrentValues();
            return Task.CompletedTask;
        });

        ReloadConfigCommand = new RelayCommand(_ =>
        {
            ReloadConfigFromFile();
            MessageBox.Show("Configuration reloaded.");
            return Task.CompletedTask;
        });
        ShowHomeCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Home;
            return Task.CompletedTask;
        }
        );
        ShowMaintCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Maint;
            return Task.CompletedTask;
        }
        );
        ShowRecipeCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Recipe;
            return Task.CompletedTask;
        }
        );
        ShowDatalogCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Datalog;
            return Task.CompletedTask;
        }
        );
        ShowSetupCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Setup;
            return Task.CompletedTask;
        }
        );
        ShowAlarmsCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Alarms;
            return Task.CompletedTask;
        }
        );
        ShowListCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.List;
            return Task.CompletedTask;
        }
        );
        ShowQuickCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Quick;
            return Task.CompletedTask;
        }
        );
        ShowVisionCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Vision;
            return Task.CompletedTask;
        }
        );
        ShowErrorCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Error;
            return Task.CompletedTask;
        }
        );
        ShowMESCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Mes;
            return Task.CompletedTask;
        }
        );
        ShowPowerCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Power;
            return Task.CompletedTask;
        }
        );
        LoginCommand = new RelayCommand (_ =>
        {
            var loginWindow = new LoginWindow(
            CurrentRole.ToString(),
            EngineerPassword,
            DeveloperPassword);
            if (Application.Current.MainWindow!=null)
            {
                loginWindow.Owner = Application.Current.MainWindow;
            }
            bool? result = loginWindow.ShowDialog();
            if(result == true)
            {
                if(loginWindow.IslogoutRequested)
                {
                    CurrentRole = UserRole.Operator;
                }
                else if(!string.IsNullOrEmpty(loginWindow.SelectedRole))
                {
                    if (loginWindow.SelectedRole == "Engineer")
                    {
                        CurrentRole = UserRole.Engineer;
                    }
                    else if (loginWindow.SelectedRole == "Developer")
                    {
                        CurrentRole = UserRole.Developer;
                    }
                }
                
            }
            return Task.CompletedTask;
        });
        CurrentRole = UserRole.Operator;
        ApplyRolePermissions();
        CurrentMachineState = MachineState.Offline;
        IsPowerMachineOn = false;
        IsAlarmOn = false;
        IsLaserOn = true;
        IsFrontDoorSafetyReset = false;
        IsRearDoorSafetyReset = false;
        IsPulsingOn = true;
        IsDiodeOn = true;
        IsRearDoorClosed = true; 
        IsFrontDoorClosed = true;
        HasCompletedInitialInit = false;
        UpdateMachineState();


    }

    public bool IsLaserHealthy => CurrentMachineState != MachineState.Alarm && IsLaserOn;
    public bool IsAlarmState => CurrentMachineState == MachineState.Alarm;
    public bool IsHostGreen => IsPowerMachineOn && CurrentMachineState != MachineState.Offline;
    public bool IsLaserGreen=>IsLaserOn && CurrentMachineState != MachineState.Alarm;
    public bool IsDiodeGreen=>IsDiodeOn && CurrentMachineState != MachineState.Offline;
    public bool IsPulsingGreen=>IsPulsingOn && CurrentMachineState != MachineState.Offline;


    public bool IsFrontDoorGreen => IsFrontDoorClosed && IsFrontDoorSafetyReset;
    public bool IsRearDoorGreen => IsRearDoorClosed && IsRearDoorSafetyReset;



    #region cac nut nhan

    private int _pcbOkCount = 0;
    public int PcbOkCount
    {
        get => _pcbOkCount;
        set
        {
            _pcbOkCount = value;
            OnPropertyChanged();
        }
    }
    private int _pbaOkCount = 0;
    public int PbaOkCount
    {
        get => _pbaOkCount;
        set
        {
            _pbaOkCount = value;
            OnPropertyChanged();
        }
    }
    private int _pbaNgCount = 0;
    public int PbaNgCount
    {
        get => _pbaNgCount;
        set
        {
            _pbaNgCount = value;
            OnPropertyChanged();
        }
    }
    private bool _canStart = true;
    public bool CanStart
    {
        get => _canStart;
        set
        {
            _canStart = value;
            OnPropertyChanged();
        }
    }

    private bool _canStop = true;
    public bool CanStop
    {
        get => _canStop;
        set
        {
            _canStop = value;
            OnPropertyChanged();
        }
    }

    private bool _canCycleStop = true;
    public bool CanCycleStop
    {
        get => _canCycleStop;
        set
        {
            _canCycleStop= value;
            OnPropertyChanged();
        }
    }

    private bool _canInit = true;
    public bool CanInit
    {
        get => _canInit;
        set
        {
            _canInit = value;
            OnPropertyChanged();
        }
    }

    private bool _canReset = true;
    public bool CanReset
    {
        get => _canReset;
        set
        {
            _canReset = value;
            OnPropertyChanged();
        }
    }

    private bool _canHome = true;
    public bool CanHome
    {
        get => _canHome;
        set
        {
            _canHome = value;
            OnPropertyChanged();
        }
    }

    private bool _canMaint = false;
    public bool CanMaint
    {
        get => _canMaint;
        set
        {
            _canMaint = value;
            OnPropertyChanged();
        }
    }
    private bool _canRecipe = false;
    public bool CanRecipe
    {
        get => _canRecipe;
        set
        {
            _canRecipe = value;
            OnPropertyChanged();
        }
    }
    private bool _canDatalog = true;
    public bool CanDatalog
    {
        get => _canDatalog;
        set
        {
            _canDatalog = value;
            OnPropertyChanged();
        }
    }
    private bool _canSetup = false;
    public bool CanSetup
    {
        get => _canSetup;
        set
        {
            _canSetup = value;
            OnPropertyChanged();
        }
    }
    private bool _canAlarms = true;
    public bool CanAlarms
    {
        get => _canAlarms;
        set
        {
            _canAlarms = value;
            OnPropertyChanged();
        }
    }
    private bool _canList = false;
    public bool CanList
    {
        get => _canList;
        set
        {
            _canList = value;
            OnPropertyChanged();
        }
    }

    private bool _canQuick = true;
    public bool CanQuick
    {
        get => _canQuick;
        set
        {
            _canQuick = value;
            OnPropertyChanged();
        }
    }

    private bool _canVision = false;
    public bool CanVision
    {
        get => _canVision;
        set
        {
            _canVision = value;
            OnPropertyChanged();
        }
    }

    private bool _canError = true;
    public bool CanError
    {
        get => _canError;
        set
        {
            _canError = value;
            OnPropertyChanged();
        }
    }

    private bool _canMes = false;
    public bool CanMes
    {
        get => _canMes;
        set
        {
            _canMes = value;
            OnPropertyChanged();
        }
    }

    private bool _canPower = false;
    public bool CanPower
    {
        get => _canPower;
        set
        {
            _canPower = value;
            OnPropertyChanged();
        }
    }

    private bool _hasCompletedInitialInit;
    public bool HasCompletedInitialInit
    {
        get => _hasCompletedInitialInit;
        set
        {
            _hasCompletedInitialInit = value;
            OnPropertyChanged();
            UpdateMachineState();
        }
    }
    public void ApplyRolePermissions()
    {
        switch (CurrentRole)
        {
            case UserRole.Operator:


                CanHome = true;
                CanMaint = false;
                CanRecipe = false;
                CanDatalog = true;
                CanSetup = false;
                CanAlarms = true;
                CanList = false;
                CanQuick = true;
                CanVision = false;
                CanError = true;
                CanMes = false;
                CanPower = false;
                break;
            case UserRole.Engineer:


                CanHome = true;
                CanMaint = true;
                CanRecipe = true;
                CanDatalog = true;
                CanSetup = true;
                CanAlarms = true;
                CanList = true;
                CanQuick = true;
                CanVision = true;
                CanError = true;
                CanMes = true;
                CanPower = true;
                break;

            case UserRole.Developer:


                CanHome = true;
                CanMaint = true;
                CanRecipe = true;
                CanDatalog = true;
                CanSetup = true;
                CanAlarms = true;
                CanList = true;
                CanQuick = true;
                CanVision = true;
                CanError = true;
                CanMes = true;
                CanPower = true;
                break;
        }
    }
    #endregion

    private void UpdateMachineState()
    {
        switch(CurrentMachineState)
        {
            case MachineState.Offline:
                StatusText = "Offline";
                HostStatusText = "Disconnected";
                LaserStatusText = "offline";

                CanInit = IsPowerMachineOn;
                CanStart = false;
                CanStop = false;
                CanReset = false;
                CanCycleStop = false;
                break;
            case MachineState.Initializing:
                StatusText = "Initializing";
                HostStatusText = "Connecting...";
                LaserStatusText = "Initializing...";

                CanInit = false;
                CanStart = false;
                CanStop= false;
                CanCycleStop= false;
                CanReset= false;
                break;
            case MachineState.Ready:
                StatusText = "Ready";
                HostStatusText = "Connected";
                LaserStatusText = "Ready";

                CanInit= false;
                CanStart= true;
                CanStop = false;
                CanCycleStop = false;
                CanReset = true;
                break;
            case MachineState.Running:
                StatusText = "Running";
                HostStatusText = "Connected";
                LaserStatusText = "Running";

                CanInit = false;
                CanStart = false;
                CanStop = true;
                CanCycleStop = true;
                CanReset = false;
                break;
            case MachineState.Stopped:
                StatusText = "stopped";
                HostStatusText = "Connected";
                LaserStatusText = "Stopped";

                CanInit = false;
                CanStart = true;
                CanStop = false;
                CanCycleStop = false;
                CanReset = true;
                break;
            case MachineState.Alarm:
                StatusText = "Alarm";
                HostStatusText = "Connected";
                LaserStatusText = "Alarm";

                CanInit = IsPowerMachineOn;
                CanStart = false;
                CanStop = false;
                CanCycleStop = false;
                CanReset = true;
                break;
            case MachineState.Standby:
                StatusText = "Standby";
                HostStatusText = "Connected";
                LaserStatusText = "Standby";

                CanInit = true;
                CanStart = false;
                CanStop = false;
                CanCycleStop = false;
                CanReset = false;
                break;

            




        }
        if (!HasCompletedInitialInit)
        {
            if (!IsPowerMachineOn)
            {
                CanInit = false;
                CanReset = false;
                CanStart = false;
                CanStop = false;
                CanCycleStop = false;
            }
            else
            {
                CanInit = true;
                CanReset = true;
                CanStart = false;
                CanStop = false;
                CanCycleStop = false;
            }
        }
        else
        {
            CanInit = IsPowerMachineOn;
        }
    }

    private void ApplySimulatorSignals()
    {
        DiodeStatusText = IsDiodeOn ? "On" : "Off";
        FrontDoorStatusText = IsFrontDoorClosed ? "Closed" : "Open";
        RearDoorStatusText = IsRearDoorClosed ? "Closed" : "Open";
        PulsingStatusText = IsPulsingOn ? "On" : "Off";

       
        
        if (!IsPowerMachineOn)
        {
            CurrentMachineState = MachineState.Offline;
            return;
        }
        if (IsAlarmOn)
        {
            CurrentMachineState = MachineState.Alarm;
            return;
        }
        if (!IsLaserOn)
        {
            LaserStatusText = "Off";
            if (CurrentMachineState == MachineState.Running ||
                CurrentMachineState == MachineState.Initializing ||
                CurrentMachineState == MachineState.Ready) 
            {
                _ =EnterAlarmAsync("LASER-001", "Laser turned off during machine operation.");
                return;
            }
            return;
        }
        
        if(CurrentMachineState == MachineState.Offline)
        {
            CurrentMachineState = MachineState.Standby;
        }
    }

    private AppPage _currentPage= AppPage.Home;
    public AppPage CurrentPage
    {
        get => _currentPage;
        set
        {
            _currentPage=value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHomePage));
            OnPropertyChanged(nameof(IsMaintPage));
            OnPropertyChanged(nameof(IsRecipePage));
            OnPropertyChanged(nameof(IsDatalogPage));
            OnPropertyChanged(nameof(IsSetupPage));
            OnPropertyChanged(nameof(IsAlarmsPage));
            OnPropertyChanged(nameof(IsListPage));
            OnPropertyChanged(nameof(IsQuickPage));
            OnPropertyChanged(nameof(IsVisionPage));
            OnPropertyChanged(nameof(IsErrorPage));
            OnPropertyChanged(nameof(IsMESPage));
            OnPropertyChanged(nameof(IsPowerPage));

        }
    }
    public bool IsHomePage=>CurrentPage==AppPage.Home;
    public bool IsMaintPage => CurrentPage == AppPage.Maint;
    public bool IsRecipePage => CurrentPage == AppPage.Recipe;
    public bool IsDatalogPage => CurrentPage == AppPage.Datalog;
    public bool IsSetupPage => CurrentPage == AppPage.Setup;
    public bool IsAlarmsPage => CurrentPage == AppPage.Alarms;

    public bool IsListPage => CurrentPage == AppPage.List;
    public bool IsQuickPage => CurrentPage == AppPage.Quick;
    public bool IsVisionPage => CurrentPage == AppPage.Vision;
    public bool IsErrorPage => CurrentPage == AppPage.Error;
    public bool IsMESPage => CurrentPage == AppPage.Mes;
    public bool IsPowerPage => CurrentPage == AppPage.Power;

    public async Task LoadAlarmHistoryAsync()
    {
        AlarmItems.Clear();

        var items = await _alarmRepository.GetVisibleAsync();
        foreach (var item in items)
        {
            AlarmItems.Add(item);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task AddAlarmAsync(string code, string message, string severity = "Error")
    {
        var record = new AlarmRecord
        {
            Timestamp = DateTime.Now,
            Code = code,
            Severity = severity,
            Message = message
        };

        AlarmItems.Insert(0, record);
        await _alarmRepository.AddAsync(record);
        await _alarmFileLogger.WriteAsync(record);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private void SaveConfig()
    {
        if (!int.TryParse(EditCycleIntervalMsText, out var cycleIntervalMs) || cycleIntervalMs <= 0)
        {
            MessageBox.Show("Cycle Interval must be a positive integer.");
            return;
        }

        if (!double.TryParse(EditCycleOkRateText, NumberStyles.Float, CultureInfo.InvariantCulture, out var cycleOkRate) ||
            cycleOkRate < 0 || cycleOkRate > 1)
        {
            MessageBox.Show("Cycle OK Rate must be a number between 0 and 1.");
            return;
        }

        AppTitle = EditAppTitle;
        OsVersion = EditOsVersion;
        LaserTimeText = EditLaserTimeText;
        CycleIntervalMs = cycleIntervalMs;
        CycleOkRate = cycleOkRate;

        _machineConfig.AppTitle = EditAppTitle;
        _machineConfig.OsVersion = EditOsVersion;
        _machineConfig.LaserTimeText = EditLaserTimeText;
        _machineConfig.EngineerPassword = EngineerPasswordValue;
        _machineConfig.DeveloperPassword = DeveloperPasswordValue;
        _machineConfig.CycleIntervalMs = cycleIntervalMs;
        _machineConfig.CycleOkRate = cycleOkRate;

        _configService.Save(_machineConfig);
        LoadEditorFromCurrentValues();
    }
    private void StartCycleLoop()
    {
        if (_isCycleLoopRunning) return;

        _cycleCts = new CancellationTokenSource();
        _isCycleLoopRunning = true;

        _ = RunCycleLoopAsync(_cycleCts.Token);
    }
    private void StopCycleLoop()
    {
        _cycleCts?.Cancel();
        _cycleCts = null;
        _isCycleLoopRunning = false;
    }
    private async Task RunCycleLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && CurrentMachineState == MachineState.Running)
            {
                await Task.Delay(CycleIntervalMs, token);
                

                if (token.IsCancellationRequested || CurrentMachineState != MachineState.Running)
                    break;

                PcbOkCount += 1;

                bool isOk = _random.NextDouble() < CycleOkRate;

                if (isOk)
                {
                    PbaOkCount += 1;
                    await AddOperationLogAsync("Cycle", "Cycle completed: OK");
                }
                else
                {
                    PbaNgCount += 1;
                    await AddOperationLogAsync("Cycle", "Cycle completed: NG");
                }
            }
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            _isCycleLoopRunning = false;
        }
    }
    private void LoadEditorFromCurrentValues()
    {
        EditAppTitle = AppTitle;
        EditOsVersion = OsVersion;
        EditLaserTimeText = LaserTimeText;
        EngineerPasswordValue = _machineConfig.EngineerPassword;
        DeveloperPasswordValue = _machineConfig.DeveloperPassword;
        EditCycleIntervalMsText = CycleIntervalMs.ToString();
        EditCycleOkRateText = CycleOkRate.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private void ReloadConfigFromFile()
    {
        var latestConfig = _configService.Load();

        _machineConfig.AppTitle = latestConfig.AppTitle;
        _machineConfig.OsVersion = latestConfig.OsVersion;
        _machineConfig.LaserTimeText = latestConfig.LaserTimeText;
        _machineConfig.EngineerPassword = latestConfig.EngineerPassword;
        _machineConfig.DeveloperPassword = latestConfig.DeveloperPassword;
        _machineConfig.CycleIntervalMs = latestConfig.CycleIntervalMs;
        _machineConfig.CycleOkRate = latestConfig.CycleOkRate;

        AppTitle = _machineConfig.AppTitle;
        OsVersion = _machineConfig.OsVersion;
        LaserTimeText = _machineConfig.LaserTimeText;
        CycleIntervalMs = _machineConfig.CycleIntervalMs;
        CycleOkRate = _machineConfig.CycleOkRate;

        LoadEditorFromCurrentValues();
    }
    private void NavigateToHome()
    {
        CurrentPage = AppPage.Home;
    }
    private void NotifySetupStateChanged()
    {
        OnPropertyChanged(nameof(IsSetupEditLocked));
        OnPropertyChanged(nameof(IsAppTitleValid));
        OnPropertyChanged(nameof(IsOsVersionValid));
        OnPropertyChanged(nameof(IsLaserTimeValid));
        OnPropertyChanged(nameof(IsEngineerPasswordValid));
        OnPropertyChanged(nameof(IsDeveloperPasswordValid));
        OnPropertyChanged(nameof(IsCycleIntervalValid));
        OnPropertyChanged(nameof(IsCycleOkRateValid));
        OnPropertyChanged(nameof(IsSetupInputValid));
        OnPropertyChanged(nameof(CanSaveSetup));
        OnPropertyChanged(nameof(HasSetupValidationError));
    }
}