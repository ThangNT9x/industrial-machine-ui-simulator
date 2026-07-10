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
using System.Threading;
namespace IndustrialMachineSimulator.UI.ViewModels;
using System.Windows.Media;


public partial class MainViewModel : INotifyPropertyChanged
{
    private readonly MachineController _machineController;
    private string _statusText = "Offline";
    private readonly SorterEngine _sorterEngine;

    public SorterState Sorter { get; } = new();
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
    public ICommand CancelInitializeCommand { get; }

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
    public ICommand ShowIoCommand { get; }
    public ICommand ShowMESCommand { get; }
    public ICommand ShowPowerCommand { get; }

    public ICommand OpenSimulatorWindowCommand {  get; }

    public ICommand ClearAlarmUiCommand { get; }

    public ICommand ClearOperationUiCommand { get; }

    public ICommand SaveConfigCommand { get; }
    public ICommand CancelConfigEditCommand { get; }
    public ICommand ReloadConfigCommand { get; }
    public ICommand ApplyRecipeCommand { get; }
    public ICommand CancelRecipeEditCommand { get; }
    public ICommand ReloadRecipeCommand { get; }
    public ICommand LoadSelectedRecipeCommand { get; }
    public ICommand SaveRecipeCommand { get; }
    public ICommand DeleteRecipeCommand { get; }
    public ICommand ClearSorterIoCommand { get; }
    public ICommand InjectMaterialAtInputCommand { get; }
    public ICommand InjectMaterialAtFeed1Command { get; }
    public ICommand InjectMaterialAtFeed2Command { get; }
    public ICommand InjectMaterialAtFeed3Command { get; }
    public ICommand InjectMaterialAtOutConveyorCommand { get; }
    public ICommand ClearMaterialAtInputCommand { get; }
    public ICommand ClearMaterialAtFeed1Command { get; }
    public ICommand ClearMaterialAtFeed2Command { get; }
    public ICommand ClearMaterialAtFeed3Command { get; }
    public ICommand ClearMaterialAtOutConveyorCommand { get; }

    public ICommand ConnectMesCommand { get; }
    public ICommand DisconnectMesCommand { get; }
    public ICommand ClearMesUiCommand { get; }




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
            OnPropertyChanged(nameof(AlarmBannerText));
            OnPropertyChanged(nameof(AlarmBannerVisibility));
            OnPropertyChanged(nameof(AlarmFrameThickness));
            OnPropertyChanged(nameof(AlarmFrameBrush));
            UpdateMachineState();
            NotifySetupStateChanged();
            NotifyRecipeStateChanged();
            RefreshRunPermission();
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
    private CancellationTokenSource? _initCts;

    private bool _isInitializationOverlayVisible;
    public bool IsInitializationOverlayVisible
    {
        get => _isInitializationOverlayVisible;
        set
        {
            if (_isInitializationOverlayVisible == value) return;
            _isInitializationOverlayVisible = value;
            OnPropertyChanged();
        }
    }

    private int _initProgressPercent;
    public int InitProgressPercent
    {
        get => _initProgressPercent;
        set
        {
            if (_initProgressPercent == value) return;
            _initProgressPercent = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(InitProgressText));
        }
    }

    public string InitProgressText => $"Initialization {InitProgressPercent}%";


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
                StopRunTimeLoop();
                StopSorterPipelineLoop();
                SetSorterRunningState(false);
                UpdateSorterSensors();
                NotifySorterMaterialChanged();

                _runStartTime = null;
                _cycleStartTime = null;
                if (CurrentMachineState==MachineState.Running||
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
            RefreshRunPermission();
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
            OnPropertyChanged(nameof(CurrentCycleIntervalText));
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
            OnPropertyChanged(nameof(CurrentCycleOkRateText));
        }
    }

    private readonly IMesClient _mesClient;

    public ObservableCollection<MesMessageRecord> MesItems { get; } = new();

    private MesConnectionState _mesConnectionState = MesConnectionState.Disconnected;
    public MesConnectionState MesConnectionState
    {
        get => _mesConnectionState;
        set
        {
            _mesConnectionState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MesConnectionStatusText));
            OnPropertyChanged(nameof(IsMesConnected));
            OnPropertyChanged(nameof(MesStatusDotColor));
        }
    }

    public string MesConnectionStatusText => MesConnectionState.ToString();
    public bool IsMesConnected => MesConnectionState == MesConnectionState.Connected;

    private bool _isMesAutoSendCycleResult = true;
    public bool IsMesAutoSendCycleResult
    {
        get => _isMesAutoSendCycleResult;
        set
        {
            _isMesAutoSendCycleResult = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MesMessageRecord> HomeMesItems { get; } = new();

    private string _lastMesTxText = "-";
    public string LastMesTxText
    {
        get => _lastMesTxText;
        set
        {
            _lastMesTxText = value;
            OnPropertyChanged();
        }
    }

    private string _lastMesRxText = "-";
    public string LastMesRxText
    {
        get => _lastMesRxText;
        set
        {
            _lastMesRxText = value;
            OnPropertyChanged();
        }
    }

    public string MesStatusDotColor =>
    MesConnectionState == MesConnectionState.Connected ? "#16A34A" :
    MesConnectionState == MesConnectionState.Connecting ? "#EAB308" :
    "#9CA3AF";

    private bool _isIoManualMode;
    public bool IsIoManualMode
    {
        get => _isIoManualMode;
        set
        {
            if (_isIoManualMode == value) return;
            _isIoManualMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IoModeText));
        }
    }

    public string IoModeText => IsIoManualMode ? "Manual IO Mode" : "Auto IO Mode";

    public string CurrentCycleIntervalText => $"{CycleIntervalMs} ms";
    public string CurrentCycleOkRateText => $"{CycleOkRate:P0}";
    private string _editCycleIntervalMsText = "3000";
    public string EditCycleIntervalMsText
    {
        get => _editCycleIntervalMsText;
        set
        {
            _editCycleIntervalMsText = value;
            OnPropertyChanged();
            NotifySetupStateChanged();
            OnPropertyChanged(nameof(CurrentCycleIntervalText));
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
            OnPropertyChanged(nameof(CurrentCycleOkRateText));
        }
    }
    private string _currentRecipeName = "Default Recipe";
    public string CurrentRecipeName
    {
        get => _currentRecipeName;
        set
        {
            _currentRecipeName = value;
            OnPropertyChanged();
        }
    }

    private string _currentProductModel = "MODEL-001";
    public string CurrentProductModel
    {
        get => _currentProductModel;
        set
        {
            _currentProductModel = value;
            OnPropertyChanged();
        }
    }

    private bool _enableNgSimulation = true;
    public bool EnableNgSimulation
    {
        get => _enableNgSimulation;
        set
        {
            _enableNgSimulation = value;
            OnPropertyChanged();
        }
    }
    private string _editRecipeName = "Default Recipe";
    public string EditRecipeName
    {
        get => _editRecipeName;
        set
        {
            _editRecipeName = value;
            OnPropertyChanged();
            NotifyRecipeStateChanged();
        }
    }

    private string _editProductModel = "MODEL-001";
    public string EditProductModel
    {
        get => _editProductModel;
        set
        {
            _editProductModel = value;
            OnPropertyChanged();
            NotifyRecipeStateChanged();
        }
    }

    private string _editRecipeCycleIntervalMsText = "3000";
    public string EditRecipeCycleIntervalMsText
    {
        get => _editRecipeCycleIntervalMsText;
        set
        {
            _editRecipeCycleIntervalMsText = value;
            OnPropertyChanged();
            NotifyRecipeStateChanged();
        }
    }

    private string _editRecipeCycleOkRateText = "0.8";
    public string EditRecipeCycleOkRateText
    {
        get => _editRecipeCycleOkRateText;
        set
        {
            _editRecipeCycleOkRateText = value;
            OnPropertyChanged();
            NotifyRecipeStateChanged();
        }
    }

    private bool _editEnableNgSimulation = true;
    public bool EditEnableNgSimulation
    {
        get => _editEnableNgSimulation;
        set
        {
            _editEnableNgSimulation = value;
            OnPropertyChanged();
            NotifyRecipeStateChanged();
        }
    }
    private string _editRecipeSorterStepIntervalMsText = "300";
    public string EditRecipeSorterStepIntervalMsText
    {
        get => _editRecipeSorterStepIntervalMsText;
        set
        {
            _editRecipeSorterStepIntervalMsText = value;
            OnPropertyChanged();
            NotifyRecipeStateChanged();
        }
    }

    private string _editRecipeInfeedSpacingMsText = "1000";
    public string EditRecipeInfeedSpacingMsText
    {
        get => _editRecipeInfeedSpacingMsText;
        set
        {
            _editRecipeInfeedSpacingMsText = value;
            OnPropertyChanged();
            NotifyRecipeStateChanged();
        }
    }
    public bool IsRecipeSorterStepIntervalValid =>
    int.TryParse(EditRecipeSorterStepIntervalMsText, out var ms) && ms > 0;

    public bool IsRecipeInfeedSpacingValid =>
    int.TryParse(EditRecipeInfeedSpacingMsText, out var ms) && ms > 0;


    private string _runTimeText = "00:00:00";
    public string RunTimeText
    {
        get => _runTimeText;
        set
        {
            _runTimeText = value;
            OnPropertyChanged();
        }
    }

    private string _cycleTimeText = "00:00:00";
    public string CycleTimeText
    {
        get => _cycleTimeText;
        set
        {
            _cycleTimeText = value;
            OnPropertyChanged();
        }
    }

    private int _outCount = 0;
    public int OutCount
    {
        get => _outCount;
        set
        {
            _outCount = value;
            OnPropertyChanged();
        }
    }
    private DateTime? _runStartTime;
    private DateTime? _cycleStartTime;
    private CancellationTokenSource? _runTimeCts;

    private int _sorterStepIntervalMs = 300;
    public int SorterStepIntervalMs
    {
        get => _sorterStepIntervalMs;
        set
        {
            _sorterStepIntervalMs = value;
            OnPropertyChanged();
        }
    }

    private int _infeedSpacingMs = 1000;
    public int InfeedSpacingMs
    {
        get => _infeedSpacingMs;
        set
        {
            _infeedSpacingMs = value;
            OnPropertyChanged();
        }
    }
    private int _pickerUnloadMs = 1500;
    public int PickerUnloadMs
    {
        get => _pickerUnloadMs;
        set
        {
            if (_pickerUnloadMs == value) return;
            _pickerUnloadMs = value;
            OnPropertyChanged();
        }
    }

    public bool IsSetupEditLocked => CurrentMachineState == MachineState.Running;
    public bool IsRecipeEditLocked => CurrentMachineState == MachineState.Running;


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
    public bool IsRecipeNameValid => !string.IsNullOrWhiteSpace(EditRecipeName);
    public bool IsProductModelValid => !string.IsNullOrWhiteSpace(EditProductModel);

    public bool IsRecipeCycleIntervalValid =>
        int.TryParse(EditRecipeCycleIntervalMsText, out var ms) && ms > 0;

    public bool IsRecipeCycleOkRateValid =>
        double.TryParse(EditRecipeCycleOkRateText, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var rate)
        && rate >= 0 && rate <= 1;

    public bool IsRecipeInputValid =>
    IsRecipeNameValid &&
    IsProductModelValid &&
    IsRecipeCycleIntervalValid &&
    IsRecipeCycleOkRateValid &&
    IsRecipeSorterStepIntervalValid &&
    IsRecipeInfeedSpacingValid;

    public bool CanApplyRecipe => !IsRecipeEditLocked && IsRecipeInputValid;
    public bool HasRecipeValidationError => !IsRecipeInputValid;
    public ObservableCollection<RecipeItem> RecipeItems { get; } = new();

    private RecipeItem? _selectedRecipeItem;
    public RecipeItem? SelectedRecipeItem
    {
        get => _selectedRecipeItem;
        set
        {
            _selectedRecipeItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedRecipe));
            NotifyRecipeStateChanged();
        }
    }

    public bool HasSelectedRecipe => SelectedRecipeItem != null;
    public bool CanSaveRecipe => !IsRecipeEditLocked && IsRecipeInputValid;
    public bool CanDeleteRecipe => !IsRecipeEditLocked && HasSelectedRecipe;

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

    private string _lastAlarmCode = string.Empty;
    public string LastAlarmCode
    {
        get => _lastAlarmCode;
        set
        {
            _lastAlarmCode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AlarmBannerText));
        }
    }

    private string _lastAlarmMessage = string.Empty;
    public string LastAlarmMessage
    {
        get => _lastAlarmMessage;
        set
        {
            _lastAlarmMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AlarmBannerText));
        }
    }

    public string AlarmBannerText =>
        IsAlarmState
            ? $"⚠ MACHINE ALARM  |  {LastAlarmCode}  |  {LastAlarmMessage}"
            : string.Empty;

    public Visibility AlarmBannerVisibility =>
        IsAlarmState ? Visibility.Visible : Visibility.Collapsed;

    public Thickness AlarmFrameThickness =>
        IsAlarmState ? new Thickness(6) : new Thickness(0);

    public Brush AlarmFrameBrush =>
        IsAlarmState ? Brushes.Red : Brushes.Transparent;


    private async Task EnterAlarmAsync(string code, string message, bool showPopup = true)
    {
        StopCycleLoop();
        StopRunTimeLoop();

        // Alarm/Fault = hard stop, not soft stop
        _sorterEngine.EmergencyStop();

        UpdateSorterSensors();
        NotifySorterMaterialChanged();
        NotifyStopRequestUiChanged();
        NotifySchedulerDebugChanged();

        _runStartTime = null;
        _cycleStartTime = null;
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
        await SendMesMessageAsync("AlarmRaised", $"Code={code};Message={message}");

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
        if (!CanRunByStatusLights)
        {
            MessageBox.Show("Cannot start. Host, Laser, Diode, Pulsing, Front Door and Rear Door must all be green.");
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
            _runStartTime = DateTime.Now;
            _cycleStartTime = DateTime.Now;

            ClearStopRequests();
            NotifyStopRequestUiChanged();
            NotifySchedulerDebugChanged();

            StartRunTimeLoop();
            SetSorterRunningState(true);
            StartSorterPipelineLoop();
            NavigateToHome();
            StartCycleLoop();

            await AddOperationLogAsync("Run", "Machine started running.");
            await SendMesMessageAsync(
                "StartJob",
                $"Recipe={CurrentRecipeName};Model={CurrentProductModel}");
        }
    }

    private async Task TryStopMachine()
    {
        if (CurrentMachineState != MachineState.Running)
            return;

        if (IsStopRequestedActive)
            return;

        NavigateToHome();
        StopCycleLoop();

        RequestStop();
        NotifyStopRequestUiChanged();
        NotifySchedulerDebugChanged();

        await AddOperationLogAsync("Run", "Stop requested.");
    }

    private async Task RunInitializeAsync()
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

        if (!IsLaserOn)
        {
            await EnterAlarmAsync("LASER-INIT-001", "Laser is off. Cannot initialize.");
            return;
        }

        if (!IsDiodeOn)
        {
            await EnterAlarmAsync("DIODE-INIT-001", "Diode is off. Cannot initialize.");
            return;
        }

        if (!IsPulsingOn)
        {
            await EnterAlarmAsync("PULSE-INIT-001", "Pulsing signal is off. Cannot initialize.");
            return;
        }

        if (HasAnyMaterialInSorter)
        {
            MessageBox.Show("Material still exists in machine. Cannot initialize.");
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

        _initCts?.Cancel();
        _initCts = new CancellationTokenSource();
        var token = _initCts.Token;

        try
        {
            InitProgressPercent = 0;
            IsInitializationOverlayVisible = true;

            CurrentMachineState = MachineState.Initializing;

            const int totalMs = 10000;
            const int stepMs = 100;
            const int totalSteps = totalMs / stepMs;

            for (int step = 0; step <= totalSteps; step++)
            {
                token.ThrowIfCancellationRequested();

                InitProgressPercent = step * 100 / totalSteps;

                if (!IsPowerMachineOn)
                {
                    await EnterAlarmAsync("POWER-INIT-001", "Power turned off during initialization.");
                    return;
                }

                if (!IsFrontDoorClosed || !IsRearDoorClosed)
                {
                    await EnterAlarmAsync("DOOR-001", "Door opened during initialization.");
                    return;
                }

                if (!IsLaserOn)
                {
                    await EnterAlarmAsync("LASER-INIT-001", "Laser turned off during initialization.");
                    return;
                }

                if (!IsDiodeOn)
                {
                    await EnterAlarmAsync("DIODE-INIT-001", "Diode turned off during initialization.");
                    return;
                }

                if (!IsPulsingOn)
                {
                    await EnterAlarmAsync("PULSE-INIT-001", "Pulsing signal turned off during initialization.");
                    return;
                }

                if (step < totalSteps)
                {
                    await Task.Delay(stepMs, token);
                }
            }

            IsFrontDoorSafetyReset = IsFrontDoorClosed;
            IsRearDoorSafetyReset = IsRearDoorClosed;

            HasCompletedInitialInit = true;
            CurrentMachineState = MachineState.Ready;

            await AddOperationLogAsync("Init", "Initialization completed successfully.");
        }
        catch (OperationCanceledException)
        {
            CurrentMachineState = HasCompletedInitialInit
                ? MachineState.Ready
                : MachineState.Standby;

            await AddOperationLogAsync("Init", "Initialization canceled by operator.");
        }
        finally
        {
            IsInitializationOverlayVisible = false;
            InitProgressPercent = 0;

            _initCts?.Dispose();
            _initCts = null;
        }
    }

    private void CancelInitialization()
    {
        if (CurrentMachineState != MachineState.Initializing)
            return;

        _initCts?.Cancel();
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
            RefreshRunPermission();
        }
    }
    private bool _isDiodeOn;
    public bool IsDiodeOn
    {
        get => _isDiodeOn;
        set
        {
            if (_isDiodeOn == value) return;

            _isDiodeOn = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDiodeGreen));
            OnPropertyChanged(nameof(IsAlarmState));

            ApplySimulatorSignals();
            RefreshRunPermission();
        }
    }

    private bool _isPulsingOn;
    public bool IsPulsingOn
    {
        get => _isPulsingOn;
        set
        {
            if (_isPulsingOn == value) return;

            _isPulsingOn = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPulsingGreen));
            OnPropertyChanged(nameof(IsAlarmState));

            ApplySimulatorSignals();
            RefreshRunPermission();
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
            RefreshRunPermission();
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
            RefreshRunPermission();
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
            RefreshRunPermission();
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
            RefreshRunPermission();
        }
    }
    private Views.SimulatorControlWindow? _simulatorWindow;

    public MainViewModel(MachineController machineController,
        IAlarmRepository alarmRepository, 
        IAlarmFileLogger alarmFileLogger,
        IOperationLogRepository operationLogRepository,
        IOperationFileLogger operationFileLogger,
        IConfigService configService,
        IMesClient mesClient)
    {
        _machineController = machineController;
        _alarmRepository = alarmRepository;
        _alarmFileLogger = alarmFileLogger;
        _operationLogRepository = operationLogRepository;
        _operationFileLogger=operationFileLogger;
        _configService = configService;
        _mesClient = mesClient;
        _machineConfig = _configService.Load();
        Sorter.PropertyChanged += Sorter_PropertyChanged;
        AppTitle = _machineConfig.AppTitle;
        OsVersion = _machineConfig.OsVersion;
        LaserTimeText = _machineConfig.LaserTimeText;
        CycleIntervalMs = _machineConfig.CycleIntervalMs;
        CycleOkRate = _machineConfig.CycleOkRate;
        SorterStepIntervalMs = _machineConfig.SorterStepIntervalMs;
        InfeedSpacingMs = _machineConfig.InfeedSpacingMs;
        LoadEditorFromCurrentValues();
    
        CurrentRecipeName = _machineConfig.CurrentRecipeName;
        CurrentProductModel = _machineConfig.CurrentProductModel;
        EnableNgSimulation = _machineConfig.EnableNgSimulation;
        LoadRecipeEditorFromCurrentValues();
        EnsureRecipeListInitialized();
        LoadRecipeListFromConfig();

        InitializeCommand = new RelayCommand(async _ =>
        {
            await RunInitializeAsync();
        });

        CancelInitializeCommand = new RelayCommand(_ =>
        {
            CancelInitialization();
            return Task.CompletedTask;
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
        ApplyRecipeCommand = new RelayCommand(async _ =>
        {
            await ApplyRecipeAsync();
        });

        CancelRecipeEditCommand = new RelayCommand(_ =>
        {
            LoadRecipeEditorFromCurrentValues();
            return Task.CompletedTask;
        });

        ReloadRecipeCommand = new RelayCommand(_ =>
        {
            LoadRecipeEditorFromCurrentValues();
            MessageBox.Show("Recipe editor reloaded.");
            return Task.CompletedTask;
        });
        LoadSelectedRecipeCommand = new RelayCommand(_ =>
        {
            LoadSelectedRecipeIntoEditor();
            return Task.CompletedTask;
        });

        SaveRecipeCommand = new RelayCommand(_ =>
        {
            SaveRecipeToList();
            return Task.CompletedTask;
        });

        DeleteRecipeCommand = new RelayCommand(_ =>
        {
            DeleteSelectedRecipe();
            return Task.CompletedTask;
        });
        ClearSorterIoCommand = new RelayCommand(_ =>
        {
            ClearSorterIo();
            return Task.CompletedTask;
        });

        InjectMaterialAtInputCommand = new RelayCommand(_ =>
        {
            InjectMaterialAtInput();
            return Task.CompletedTask;
        });
        InjectMaterialAtFeed1Command = new RelayCommand(_ =>
        {
            InjectMaterialAtFeed1();
            return Task.CompletedTask;
        });

        InjectMaterialAtFeed2Command = new RelayCommand(_ =>
        {
            InjectMaterialAtFeed2();
            return Task.CompletedTask;
        });

        InjectMaterialAtFeed3Command = new RelayCommand(_ =>
        {
            InjectMaterialAtFeed3();
            return Task.CompletedTask;
        });

        InjectMaterialAtOutConveyorCommand = new RelayCommand(_ =>
        {
            InjectMaterialAtOutConveyor();
            return Task.CompletedTask;
        });
        ClearMaterialAtInputCommand = new RelayCommand(_ =>
        {
            ClearMaterialAtStage(1);
            return Task.CompletedTask;
        });

        ClearMaterialAtFeed1Command = new RelayCommand(_ =>
        {
            ClearMaterialAtStage(2);
            return Task.CompletedTask;
        });

        ClearMaterialAtFeed2Command = new RelayCommand(_ =>
        {
            ClearMaterialAtStage(3);
            return Task.CompletedTask;
        });

        ClearMaterialAtFeed3Command = new RelayCommand(_ =>
        {
            ClearMaterialAtStage(4);
            return Task.CompletedTask;
        });

        ClearMaterialAtOutConveyorCommand = new RelayCommand(_ =>
        {
            ClearMaterialAtStage(5);
            return Task.CompletedTask;
        });

        ConnectMesCommand = new RelayCommand(async _ =>
        {
            if (MesConnectionState != MesConnectionState.Disconnected)
                return;

            MesConnectionState = MesConnectionState.Connecting;
            AddMesMessage("SYS", "Connect", "Connecting to MES...");

            await _mesClient.ConnectAsync();
            MesConnectionState = _mesClient.ConnectionState;

            AddMesMessage("RX", "ConnectAck", "MES connection established.");
        });

        DisconnectMesCommand = new RelayCommand(async _ =>
        {
            await _mesClient.DisconnectAsync();
            MesConnectionState = _mesClient.ConnectionState;
            AddMesMessage("SYS", "Disconnect", "MES disconnected.");
        });

        ClearMesUiCommand = new RelayCommand(_ =>
        {
            MesItems.Clear();
            HomeMesItems.Clear();
            return Task.CompletedTask;
        });
        _sorterEngine = new SorterEngine(Sorter)
        {
            GetSorterStepIntervalMs = () => SorterStepIntervalMs,
            GetPickerUnloadMs = () => PickerUnloadMs,
            IsMachineRunning = () => CurrentMachineState == MachineState.Running,
            HandleFinishedWorkpieceAsync = async isOk => await HandleFinishedWorkpieceAsync(isOk),
            FinalizeStopAsync = FinalizeMachineStopAsync
        };

        _sorterEngine.StateChanged += SorterEngine_StateChanged;
        
        StartCommand = new RelayCommand(_ => TryStartMachine());

        StopCommand = new RelayCommand(async _ =>
        {
            if (CurrentMachineState != MachineState.Running)
                return;

            if (IsStopRequestedActive)
                return;

            NavigateToHome();
            StopCycleLoop();

            RequestStop();
            NotifyStopRequestUiChanged();
            NotifySchedulerDebugChanged();

            await AddOperationLogAsync("Run", "Stop requested.");
        });

        RunStatusBarCommand = new RelayCommand(async _ =>
        {
            if (CurrentMachineState == MachineState.Running)
            {
                if (IsStopRequestedActive)
                    return;

                NavigateToHome();
                StopCycleLoop();

                RequestStop();
                NotifyStopRequestUiChanged();
                NotifySchedulerDebugChanged();

                await AddOperationLogAsync("Run", "Stop requested from status bar.");
                return;
            }

            await TryStartMachine();
        });

        CycleStopCommand = new RelayCommand(async _ =>
        {
            if (CurrentMachineState != MachineState.Running)
                return;

            if (IsCycleStopRequestedActive)
                return;

            NavigateToHome();
            StopCycleLoop();

            RequestCycleStop();
            NotifyStopRequestUiChanged();
            NotifySchedulerDebugChanged();

            await AddOperationLogAsync("Run", "Cycle stop requested.");
        });

        ResetCommand = new RelayCommand(async _ =>
        {
            if (!IsPowerMachineOn)
            {
                MessageBox.Show("Power Machine is Off.");
                return;
            }
            await AddOperationLogAsync("Reset", "Machine reset command executed.");
       

            LastAlarmCode = string.Empty;
            LastAlarmMessage = string.Empty;
            NotifyAlarmUiChanged();
            bool wasAlarm = CurrentMachineState == MachineState.Alarm || IsAlarmOn;

            IsAlarmOn = false;
            IsFrontDoorSafetyReset = IsFrontDoorClosed;
            IsRearDoorSafetyReset = IsRearDoorClosed;

            if (wasAlarm)
            {
                _sorterEngine.PrepareRestartAfterAlarm();

                NotifySorterMaterialChanged();
                NotifySchedulerDebugChanged();
                NotifyCutStageSlotsChanged();
                NotifyStopRequestUiChanged();
            }

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
        ShowIoCommand = new RelayCommand(_ =>
        {
            CurrentPage = AppPage.Io;
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
    public bool CanRunByStatusLights =>
    IsPowerMachineOn &&
    IsHostGreen &&
    IsLaserGreen &&
    IsDiodeGreen &&
    IsPulsingGreen &&
    IsFrontDoorGreen &&
    IsRearDoorGreen &&
    !IsAlarmOn &&
    CurrentMachineState != MachineState.Alarm;



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

    private bool _canIo = true;
    public bool CanIo
    {
        get => _canIo;
        set
        {
            _canIo = value;
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
                CanIo = true;
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
                CanIo = true;
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
                CanIo = true;
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
                CanStart= CanRunByStatusLights;
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
                CanStart = CanRunByStatusLights;
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
            RefreshRunPermission();
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
        if (!IsDiodeOn)
        {
            DiodeStatusText = "Off";

            if (CurrentMachineState == MachineState.Running ||
                CurrentMachineState == MachineState.Initializing ||
                CurrentMachineState == MachineState.Ready)
            {
                _ = EnterAlarmAsync("DIODE-001", "Diode turned off during machine operation.");
                return;
            }

            return;
        }

        if (!IsPulsingOn)
        {
            PulsingStatusText = "Off";

            if (CurrentMachineState == MachineState.Running ||
                CurrentMachineState == MachineState.Initializing ||
                CurrentMachineState == MachineState.Ready)
            {
                _ = EnterAlarmAsync("PULSE-001", "Pulsing signal turned off during machine operation.");
                return;
            }

            return;
        }

        if (CurrentMachineState == MachineState.Offline)
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
            OnPropertyChanged(nameof(IsIoPage));
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
    public bool IsIoPage => CurrentPage == AppPage.Io;
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
                _cycleStartTime = DateTime.Now;

                await Task.Delay(InfeedSpacingMs, token);

                if (token.IsCancellationRequested || CurrentMachineState != MachineState.Running)
                    break;

                if (_cycleStartTime.HasValue)
                {
                    var cycleElapsed = DateTime.Now - _cycleStartTime.Value;
                    CycleTimeText = cycleElapsed.ToString(@"hh\:mm\:ss");
                }

                if (IsInputStopActive)
                {
                    continue;
                }

                bool isOk = !EnableNgSimulation || _random.NextDouble() < CycleOkRate;

                bool accepted = _sorterEngine.EnqueueWorkpiece(isOk);

                if (accepted)
                {
                    PcbOkCount += 1;
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
        NotifyRecipeStateChanged();
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
    private void NotifyRecipeStateChanged()
    {
        OnPropertyChanged(nameof(IsRecipeEditLocked));
        OnPropertyChanged(nameof(IsRecipeNameValid));
        OnPropertyChanged(nameof(IsProductModelValid));
        OnPropertyChanged(nameof(IsRecipeCycleIntervalValid));
        OnPropertyChanged(nameof(IsRecipeCycleOkRateValid));
        OnPropertyChanged(nameof(IsRecipeInputValid));
        OnPropertyChanged(nameof(CanApplyRecipe));
        OnPropertyChanged(nameof(HasRecipeValidationError));
        OnPropertyChanged(nameof(CanSaveRecipe));
        OnPropertyChanged(nameof(CanDeleteRecipe));
        OnPropertyChanged(nameof(HasSelectedRecipe));
        OnPropertyChanged(nameof(IsRecipeSorterStepIntervalValid));
        OnPropertyChanged(nameof(IsRecipeInfeedSpacingValid));
    }
    private void LoadRecipeEditorFromCurrentValues()
    {
        EditRecipeName = CurrentRecipeName;
        EditProductModel = CurrentProductModel;
        EditRecipeCycleIntervalMsText = CycleIntervalMs.ToString();
        EditRecipeCycleOkRateText = CycleOkRate.ToString(System.Globalization.CultureInfo.InvariantCulture);
        EditRecipeSorterStepIntervalMsText = SorterStepIntervalMs.ToString();
        EditRecipeInfeedSpacingMsText = InfeedSpacingMs.ToString();
        EditEnableNgSimulation = EnableNgSimulation;
    }
    private async Task ApplyRecipeAsync()
    {
        if (IsRecipeEditLocked)
        {
            MessageBox.Show("Machine is running. Stop the machine before applying recipe.");
            return;
        }

        if (!IsRecipeInputValid)
        {
            MessageBox.Show("Invalid recipe values. Please check highlighted fields.");
            return;
        }

        var cycleInterval = int.Parse(EditRecipeCycleIntervalMsText);
        var cycleOkRate = double.Parse(
            EditRecipeCycleOkRateText,
            System.Globalization.CultureInfo.InvariantCulture);
        var sorterStepInterval = int.Parse(EditRecipeSorterStepIntervalMsText);
        var infeedSpacing = int.Parse(EditRecipeInfeedSpacingMsText);

        CurrentRecipeName = EditRecipeName;
        CurrentProductModel = EditProductModel;
        CycleIntervalMs = cycleInterval;
        CycleOkRate = cycleOkRate;
        EnableNgSimulation = EditEnableNgSimulation;
        SorterStepIntervalMs = sorterStepInterval;
        InfeedSpacingMs = infeedSpacing;
        _machineConfig.CurrentRecipeName = CurrentRecipeName;
        _machineConfig.CurrentProductModel = CurrentProductModel;
        _machineConfig.CycleIntervalMs = CycleIntervalMs;
        _machineConfig.CycleOkRate = CycleOkRate;
        _machineConfig.EnableNgSimulation = EnableNgSimulation;
        _machineConfig.SorterStepIntervalMs = SorterStepIntervalMs;
        _machineConfig.InfeedSpacingMs = InfeedSpacingMs;

        _configService.Save(_machineConfig);

        await AddOperationLogAsync("Recipe",
            $"Recipe applied: {CurrentRecipeName} / {CurrentProductModel}");
        MessageBox.Show("Save Config Successed!");
        await SendMesMessageAsync(
            "RecipeLoaded",
            $"Recipe={CurrentRecipeName};Model={CurrentProductModel}");
    }
    private void EnsureRecipeListInitialized()
    {
        if (_machineConfig.Recipes == null)
        {
            _machineConfig.Recipes = new List<RecipeItem>();
        }

        if (_machineConfig.Recipes.Count == 0)
        {
            _machineConfig.Recipes.Add(new RecipeItem
            {
                RecipeName = CurrentRecipeName,
                ProductModel = CurrentProductModel,
                CycleIntervalMs = CycleIntervalMs,
                CycleOkRate = CycleOkRate,
                SorterStepIntervalMs = SorterStepIntervalMs,
                InfeedSpacingMs = InfeedSpacingMs,
                EnableNgSimulation = EnableNgSimulation
            });

            _configService.Save(_machineConfig);
        }
    }
    private void LoadRecipeListFromConfig()
    {
        RecipeItems.Clear();

        foreach (var recipe in _machineConfig.Recipes.OrderBy(x => x.RecipeName))
        {
            RecipeItems.Add(recipe);
        }
    }
    private void LoadSelectedRecipeIntoEditor()
    {
        if (SelectedRecipeItem == null)
            return;

        EditRecipeName = SelectedRecipeItem.RecipeName;
        EditProductModel = SelectedRecipeItem.ProductModel;
        EditRecipeCycleIntervalMsText = SelectedRecipeItem.CycleIntervalMs.ToString();
        EditRecipeCycleOkRateText = SelectedRecipeItem.CycleOkRate.ToString(System.Globalization.CultureInfo.InvariantCulture);
        EditRecipeSorterStepIntervalMsText = SelectedRecipeItem.SorterStepIntervalMs.ToString();
        EditRecipeInfeedSpacingMsText = SelectedRecipeItem.InfeedSpacingMs.ToString();
        EditEnableNgSimulation = SelectedRecipeItem.EnableNgSimulation;
    }
    private void SaveRecipeToList()
    {
        if (IsRecipeEditLocked)
        {
            MessageBox.Show("Machine is running. Stop the machine before saving recipe.");
            return;
        }

        if (!IsRecipeInputValid)
        {
            MessageBox.Show("Invalid recipe values. Please check highlighted fields.");
            return;
        }

        int cycleInterval = int.Parse(EditRecipeCycleIntervalMsText);
        double cycleOkRate = double.Parse(EditRecipeCycleOkRateText, CultureInfo.InvariantCulture);
        int sorterStepInterval = int.Parse(EditRecipeSorterStepIntervalMsText);
        int infeedSpacing = int.Parse(EditRecipeInfeedSpacingMsText);

        var existing = _machineConfig.Recipes
            .FirstOrDefault(x => x.RecipeName.Equals(EditRecipeName, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            _machineConfig.Recipes.Add(new RecipeItem
            {
                RecipeName = EditRecipeName,
                ProductModel = EditProductModel,
                CycleIntervalMs = cycleInterval,
                CycleOkRate = cycleOkRate,
                SorterStepIntervalMs = sorterStepInterval,
                InfeedSpacingMs = infeedSpacing,
                EnableNgSimulation = EditEnableNgSimulation
            });
        }
        else
        {
            existing.ProductModel = EditProductModel;
            existing.CycleIntervalMs = cycleInterval;
            existing.CycleOkRate = cycleOkRate;
            existing.SorterStepIntervalMs = sorterStepInterval;
            existing.InfeedSpacingMs = infeedSpacing;
            existing.EnableNgSimulation = EditEnableNgSimulation;
        }

        _configService.Save(_machineConfig);
        LoadRecipeListFromConfig();

        SelectedRecipeItem = RecipeItems
            .FirstOrDefault(x => x.RecipeName.Equals(EditRecipeName, StringComparison.OrdinalIgnoreCase));

        MessageBox.Show("Recipe saved.");
    }
    private void DeleteSelectedRecipe()
    {
        if (IsRecipeEditLocked)
        {
            MessageBox.Show("Machine is running. Stop the machine before deleting recipe.");
            return;
        }

        if (SelectedRecipeItem == null)
        {
            MessageBox.Show("Please select a recipe.");
            return;
        }

        var recipeToDelete = _machineConfig.Recipes
            .FirstOrDefault(x => x.RecipeName.Equals(SelectedRecipeItem.RecipeName, StringComparison.OrdinalIgnoreCase));

        if (recipeToDelete == null)
            return;

        _machineConfig.Recipes.Remove(recipeToDelete);
        _configService.Save(_machineConfig);
        LoadRecipeListFromConfig();

        SelectedRecipeItem = null;

        MessageBox.Show("Recipe deleted.");
    }
    private void StartRunTimeLoop()
    {
        _runTimeCts = new CancellationTokenSource();
        _ = UpdateRunTimeAsync(_runTimeCts.Token);
    }

    private void StopRunTimeLoop()
    {
        _runTimeCts?.Cancel();
        _runTimeCts = null;
    }

    private async Task UpdateRunTimeAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && _runStartTime.HasValue)
            {
                var elapsed = DateTime.Now - _runStartTime.Value;
                RunTimeText = elapsed.ToString(@"hh\:mm\:ss");
                await Task.Delay(500, token);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }
    
    private async Task HandleFinishedWorkpieceAsync(bool isOk)
    {
        if (isOk)
        {
            PbaOkCount += 1;
            CurrentTrayOkCount += 1;

            await AddOperationLogAsync(
                "Cycle",
                $"Cycle completed: OK | Recipe={CurrentRecipeName} | Model={CurrentProductModel}");

            _ = FlashStageMaterialOkAsync();

            if (CurrentTrayOkCount >= TrayCapacity)
            {
                await ProcessTrayOutputAsync(false);
            }
        }
        else
        {
            PbaNgCount += 1;

            await AddOperationLogAsync(
                "Cycle",
                $"Cycle completed: NG | Recipe={CurrentRecipeName} | Model={CurrentProductModel}");

            _ = FlashStageMaterialNgAsync();
            _ = RunNgConveyorAsync();
        }

        OutCount = PbaOkCount + PbaNgCount;
    }
    private async Task ProcessFullTrayAsync()
    {
        await RunOutLiftAsync();
        ProducedFullTrayCount += 1;
        CurrentTrayOkCount = 0;
        HasEmptyTrayLoaded = false;

        await AddOperationLogAsync(
            "Tray",
            $"Full tray output completed. FullTrays={ProducedFullTrayCount}");

        if (AvailableEmptyTrayCount > 0)
        {
            await RunInLiftAsync();
            AvailableEmptyTrayCount -= 1;
            HasEmptyTrayLoaded = true;

            await AddOperationLogAsync(
                "Tray",
                $"Empty tray loaded. RemainingEmptyTrays={AvailableEmptyTrayCount}");
        }
        else
        {
            await AddOperationLogAsync(
                "Tray",
                "No empty tray available for in-lift loading.");
        }
    }

    private void AddMesMessage(string direction, string messageType, string payload)
    {
        MesItems.Insert(0, new MesMessageRecord
        {
            Timestamp = DateTime.Now,
            Direction = direction,
            MessageType = messageType,
            Payload = payload
        });
        RefreshHomeMesItems();
        if (direction == "TX")
            LastMesTxText = $"{DateTime.Now:HH:mm:ss} | {messageType}";
        else if (direction == "RX")
            LastMesRxText = $"{DateTime.Now:HH:mm:ss} | {messageType}";
    }
    private async Task SendMesMessageAsync(string messageType, string payload)
    {
        if (!IsMesConnected)
            return;

        AddMesMessage("TX", messageType, payload);
        await _mesClient.SendAsync(messageType, payload);
    }

    private void RefreshHomeMesItems()
    {
        HomeMesItems.Clear();

        foreach (var item in MesItems.Take(6))
        {
            HomeMesItems.Add(item);
        }
    }
    private void ResetLiftAndTrayRuntime()
    {
        IsInLiftRunning = false;
        IsOutLiftRunning = false;
        IsNgConveyorRunning = false;

        IsStageMaterialOkLampOn = false;
        IsStageMaterialNgLampOn = false;

        CurrentTrayOkCount = 0;
        ProducedFullTrayCount = 0;

        TrayCapacity = 50;
        AvailableEmptyTrayCount = 200;
        HasEmptyTrayLoaded = true;
    }
    private async Task FlashStageMaterialOkAsync()
    {
        IsStageMaterialOkLampOn = true;
        await Task.Delay(300);
        IsStageMaterialOkLampOn = false;
    }
    private async Task FlashStageMaterialNgAsync()
    {
        IsStageMaterialNgLampOn = true;
        await Task.Delay(300);
        IsStageMaterialNgLampOn = false;
    }
    private async Task RunNgConveyorAsync()
    {
        IsNgConveyorRunning = true;
        await Task.Delay(2000);
        IsNgConveyorRunning = false;
    }
    private async Task RunOutLiftAsync()
    {
        IsOutLiftRunning = true;
        await Task.Delay(3000);
        IsOutLiftRunning = false;
    }
    private async Task RunInLiftAsync()
    {
        IsInLiftRunning = true;
        await Task.Delay(5000);
        IsInLiftRunning = false;
    }
    private async Task ProcessTrayOutputAsync(bool isForcedByOutTrayBox)
    {
        if (CurrentTrayOkCount <= 0)
        {
            await AddOperationLogAsync("Tray", "Tray output request ignored because current tray is empty.");
            return;
        }

        await RunOutLiftAsync();

        ProducedFullTrayCount += 1;

        await AddOperationLogAsync(
            "Tray",
            isForcedByOutTrayBox
                ? $"Tray manually output. TrayLoad={CurrentTrayOkCount}"
                : $"Full tray output completed. TrayLoad={CurrentTrayOkCount}");

        CurrentTrayOkCount = 0;
        HasEmptyTrayLoaded = false;

        if (AvailableEmptyTrayCount > 0)
        {
            await RunInLiftAsync();
            AvailableEmptyTrayCount -= 1;
            HasEmptyTrayLoaded = true;

            await AddOperationLogAsync(
                "Tray",
                $"Empty tray loaded. RemainingEmptyTrays={AvailableEmptyTrayCount}");
        }
        else
        {
            await AddOperationLogAsync(
                "Tray",
                "No empty tray available for in-lift loading.");
        }
    }

    private async Task FinalizeMachineStopAsync(string message)
    {
        StopCycleLoop();
        StopRunTimeLoop();

        _runStartTime = null;
        _cycleStartTime = null;

        SetSorterRunningState(false);
        CurrentMachineState = MachineState.Stopped;

        NotifyStopRequestUiChanged();
        NotifySchedulerDebugChanged();
        NotifySorterMaterialChanged();

        await AddOperationLogAsync("Run", message);
    }


    private void NotifyCutStageMaterialChanged()
    {
        OnPropertyChanged(nameof(HasCutStageAMaterial));
        OnPropertyChanged(nameof(HasCutStageBMaterial));
    }
    private void NotifyAlarmUiChanged()
    {
        OnPropertyChanged(nameof(IsAlarmState));
        OnPropertyChanged(nameof(AlarmBannerText));
        OnPropertyChanged(nameof(AlarmBannerVisibility));
        OnPropertyChanged(nameof(AlarmFrameThickness));
        OnPropertyChanged(nameof(AlarmFrameBrush));
        OnPropertyChanged(nameof(IsTowerRedOn));
        OnPropertyChanged(nameof(IsTowerYellowOn));
        OnPropertyChanged(nameof(IsTowerGreenOn));
    }
    private void RefreshRunPermission()
    {
        OnPropertyChanged(nameof(CanRunByStatusLights));

        if (CurrentMachineState == MachineState.Ready ||
            CurrentMachineState == MachineState.Stopped)
        {
            CanStart = CanRunByStatusLights;
        }
    }
}