using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
using IndustrialMachineSimulator.Core.Services;
using System.Windows;
using IndustrialMachineSimulator.UI;
using IndustrialMachineSimulator.Core.Entities;

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

    public string CurrentRoleText => CurrentRole.ToString();
    private MachineState _currentMachineState = MachineState.Offline;
    public MachineState CurrentMachineState
    {
        get => _currentMachineState;
        set
        {
            _currentMachineState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentMachineStateText));
            UpdateMachineState();
        }
    }
    public string CurrentMachineStateText=>CurrentMachineState.ToString();
    public string OsVersion { get; set; } = "0.0.1";

    private bool _isPowerMachineOn;
    public bool IsPowerMachineOn
    {
        get => _isPowerMachineOn;
        set
        {
            _isPowerMachineOn = value;
            OnPropertyChanged();
            ApplySimulatorSignals();
        }
    }
    private bool _isAlarmOn;
    public bool IsAlarmOn
    {
        get => _isAlarmOn;
        set
        {
            _isAlarmOn = value;
            OnPropertyChanged();
            ApplySimulatorSignals();
        }
    }
    public string LaserTimeText { get; set; } = "2000 / 20000H (100 %)";
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

    public MainViewModel(MachineController machineController)
    {
        _machineController = machineController;

        InitializeCommand = new RelayCommand(async _ =>
        {
            if (!IsPowerMachineOn) return;
            if (IsAlarmOn) return;
            if (CurrentMachineState!=MachineState.Standby) return;
            CurrentMachineState = MachineState.Initializing;
            await Task.Delay(800);
            CurrentMachineState = MachineState.Ready;
        });

        OpenSimulatorWindowCommand = new RelayCommand(_ =>
        {
            var simulatorWindow = new Views.SimulatorControlWindow(this);
            if(Application.Current.MainWindow!=null)
            {
                simulatorWindow.Owner=Application.Current.MainWindow;
            }
            simulatorWindow.Show();
            return Task.CompletedTask;
        });

        StartCommand = new RelayCommand(_ =>
        {
            if (!IsPowerMachineOn) return Task.CompletedTask;
            if (IsAlarmOn) return Task.CompletedTask;
            if(CurrentMachineState == MachineState.Ready||
            CurrentMachineState==MachineState.Stopped)
            {
                CurrentMachineState = MachineState.Running;
            }
            return Task.CompletedTask;
        });

        StopCommand = new RelayCommand(_ =>
        {
           if(CurrentMachineState==MachineState.Running)
            {
                CurrentMachineState = MachineState.Stopped;
            }
            return Task.CompletedTask;
        });
        CycleStopCommand = new RelayCommand(_ =>
        {
            if (CurrentMachineState == MachineState.Running)
            {
                CurrentMachineState = MachineState.Stopped;
            }
            return Task.CompletedTask;
        });
        ResetCommand = new RelayCommand(_ =>
        {
            if (!IsPowerMachineOn) return Task.CompletedTask;
            IsAlarmOn = false;
            if(CurrentMachineState==MachineState.Stopped||
            CurrentMachineState==MachineState.Alarm)
            {
                CurrentMachineState = MachineState.Ready;
            }
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
            var loginWindow=new LoginWindow(CurrentRole.ToString());
            if(Application.Current.MainWindow!=null)
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
    }


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

                CanInit = false;
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

                CanInit = false;
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
    }

    private void ApplySimulatorSignals()
    {
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}