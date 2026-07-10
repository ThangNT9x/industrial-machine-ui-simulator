using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrialMachineSimulator.UI.ViewModels;

public sealed class SorterEngine
{
    public event EventHandler? StateChanged;

    private readonly SorterState _state;

    public Func<int>? GetSorterStepIntervalMs { get; set; }
    public Func<int>? GetPickerUnloadMs { get; set; }
    public Func<bool>? IsMachineRunning { get; set; }

    public Func<bool, Task>? HandleFinishedWorkpieceAsync { get; set; }
    public Func<string, Task>? FinalizeStopAsync { get; set; }

    public SorterEngine(SorterState state)
    {
        _state = state;
        _state.PropertyChanged += (_, _) => RaiseStateChanged();
    }

    private enum CutStageSlot
    {
        None,
        A,
        B
    }

    private enum CutStageState
    {
        Empty,
        WaitingCut,
        Cutting,
        WaitingPick,
        Picking,
        ReadyToOutput,
        Outputting
    }

    private sealed class SorterWorkpiece
    {
        public bool IsOk { get; set; }
    }

    private readonly Queue<SorterWorkpiece> _pendingWorkpieces = new();

    private SorterWorkpiece? _stage1Workpiece;
    private SorterWorkpiece? _stage2Workpiece;
    private SorterWorkpiece? _stage3Workpiece;
    private SorterWorkpiece? _stage4Workpiece;
    private SorterWorkpiece? _stage5Workpiece;
    private SorterWorkpiece? _cutStageAWorkpiece;
    private SorterWorkpiece? _cutStageBWorkpiece;

    private CutStageState _stageAState = CutStageState.Empty;
    private CutStageState _stageBState = CutStageState.Empty;

    private bool _laserBusy;
    private bool _pickerBusy;
    private bool _feed3OutputBusy;

    private CutStageSlot _lastLaserServed = CutStageSlot.None;
    private CutStageSlot _lastPickerServed = CutStageSlot.None;
    private CutStageSlot _lastOutputServed = CutStageSlot.None;

    private int _feed1StopperDownPulseTicks;
    private CancellationTokenSource? _sorterPipelineCts;
    private CancellationTokenSource _runtimeCts = new();


    private readonly int[] _stageACutOrder = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private readonly int[] _stageBCutOrder = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    private int _activeCutNumberStageA;
    private int _activeCutNumberStageB;

    private readonly HashSet<int> _stageACompletedCuts = new();
    private readonly HashSet<int> _stageBCompletedCuts = new();
    private readonly HashSet<int> _stageAPickedCuts = new();
    private readonly HashSet<int> _stageBPickedCuts = new();

    private bool _isStageAUnloadWaiting;
    private bool _isStageBUnloadWaiting;

    private bool _isStopRequested;
    private bool _isCycleStopRequested;
    private bool _isStopFinalizing;

    private bool _isOutTrayBoxProcessing;

    private int SorterStepIntervalMs => GetSorterStepIntervalMs?.Invoke() ?? 300;
    private int PickerUnloadMs => GetPickerUnloadMs?.Invoke() ?? 1500;
    private bool IsRunning => IsMachineRunning?.Invoke() ?? false;

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    // ===== Read-only properties for UI/MainViewModel =====

    public int ActiveCutNumberStageA => _activeCutNumberStageA;
    public int ActiveCutNumberStageB => _activeCutNumberStageB;

    public bool IsStageAUnloadWaiting => _isStageAUnloadWaiting;
    public bool IsStageBUnloadWaiting => _isStageBUnloadWaiting;

    public bool HasCutStageAMaterial => _cutStageAWorkpiece != null;
    public bool HasCutStageBMaterial => _cutStageBWorkpiece != null;

    public bool IsMaterialAtInConveyor => _stage1Workpiece != null;
    public bool IsMaterialAtFeed1 => _stage2Workpiece != null;
    public bool IsMaterialAtFeed2 => _stage3Workpiece != null;
    public bool IsMaterialAtFeed3 => _stage4Workpiece != null;
    public bool IsMaterialAtOutConveyor => _stage5Workpiece != null;

    public bool HasAnyMaterialInSorter =>
        IsMaterialAtInConveyor ||
        IsMaterialAtFeed1 ||
        IsMaterialAtFeed2 ||
        IsMaterialAtFeed3 ||
        IsMaterialAtOutConveyor ||
        HasCutStageAMaterial ||
        HasCutStageBMaterial;

    public bool IsStopRequestedActive => _isStopRequested;
    public bool IsCycleStopRequestedActive => _isCycleStopRequested;
    public bool HasPendingStopCommand => _isStopRequested || _isCycleStopRequested;

    public string PendingStopCommandText =>
        _isStopRequested ? "STOP REQUESTED" :
        _isCycleStopRequested ? "CYCLE STOP REQUESTED" :
        string.Empty;

    public string StopModeText =>
        _isStopRequested ? "STOP REQUEST" :
        _isCycleStopRequested ? "CYCLE STOP REQUEST" :
        "RUN";

    public string LaserBusyText =>
        _laserBusy ? $"Laser Busy ({_lastLaserServed})" : "Laser Idle";

    public string PickerBusyText =>
        _pickerBusy ? $"Picker Busy ({_lastPickerServed})" : "Picker Idle";

    public string OutputBusyText =>
        _feed3OutputBusy ? $"Feed3 Output Busy ({_lastOutputServed})" : "Feed3 Output Idle";

    public string StageAStateText => _stageAState.ToString();
    public string StageBStateText => _stageBState.ToString();

    public string Feed1StopperText => _state.IsFeed1StopperUp ? "UP" : "DOWN";

    public bool IsStageAPicked(int index) => _stageAPickedCuts.Contains(index);
    public bool IsStageBPicked(int index) => _stageBPickedCuts.Contains(index);

    public bool IsStageADone(int index) => _stageACompletedCuts.Contains(index);
    public bool IsStageBDone(int index) => _stageBCompletedCuts.Contains(index);

    public bool IsStageAActive(int index) => _activeCutNumberStageA == index;
    public bool IsStageBActive(int index) => _activeCutNumberStageB == index;

    // ===== Public API =====

    public void StartPipelineLoop()
    {
        if (_sorterPipelineCts != null)
            return;

        if (_runtimeCts.IsCancellationRequested)
        {
            _runtimeCts = new CancellationTokenSource();
        }

        _sorterPipelineCts = new CancellationTokenSource();
        _ = RunSorterPipelineAsync(_sorterPipelineCts.Token);
    }
    public void StopPipelineLoop()
    {
        _sorterPipelineCts?.Cancel();
        _sorterPipelineCts = null;
    }

    public void EmergencyStop()
    {
        // Stop main sorter pipeline immediately
        StopPipelineLoop();

        // Cancel async running resources: laser / picker / output
        _runtimeCts.Cancel();
        _runtimeCts = new CancellationTokenSource();

        // Stop all moving actuators
        SetSorterRunningState(false);

        // Clear running resource flags
        _laserBusy = false;
        _pickerBusy = false;
        _feed3OutputBusy = false;

        // Clear stop/cycle-stop request state
        _isStopRequested = false;
        _isCycleStopRequested = false;
        _isStopFinalizing = false;

        RecoverStageAfterEmergency(CutStageSlot.A);
        RecoverStageAfterEmergency(CutStageSlot.B);

        // Stop visual active actions, but keep material positions
        _activeCutNumberStageA = 0;
        _activeCutNumberStageB = 0;
        _isStageAUnloadWaiting = false;
        _isStageBUnloadWaiting = false;

        _feed1StopperDownPulseTicks = 0;
        _state.IsFeed1StopperUp = true;

        UpdateSorterSensors();
        RaiseStateChanged();

    }

    public void PrepareRestartAfterAlarm()
    {
        StopPipelineLoop();

        SetSorterRunningState(false);

        _laserBusy = false;
        _pickerBusy = false;
        _feed3OutputBusy = false;

        _isStopRequested = false;
        _isCycleStopRequested = false;
        _isStopFinalizing = false;

        RecoverStageAfterEmergency(CutStageSlot.A);
        RecoverStageAfterEmergency(CutStageSlot.B);

        _feed1StopperDownPulseTicks = 0;
        _state.IsFeed1StopperUp = true;

        UpdateSorterSensors();
        RaiseStateChanged();
    }

    private void RecoverStageAfterEmergency(CutStageSlot slot)
    {
        var workpiece = GetCutStageWorkpiece(slot);

        SetActiveCut(slot, 0);
        SetStageUnloadWaiting(slot, false);

        // Nếu stage không còn material thì đưa về Empty
        if (workpiece == null)
        {
            ResetStageVisual(slot);
            SetStageState(slot, CutStageState.Empty);
            return;
        }

        var state = GetStageState(slot);

        switch (state)
        {
            case CutStageState.Cutting:
                // Cắt dở thì cho cắt lại từ đầu sau khi reset
                ResetStageVisual(slot);
                SetStageState(slot, CutStageState.WaitingCut);
                break;

            case CutStageState.Picking:
                // Picker dở thì cho picker lại sau khi reset
                SetStageState(slot, CutStageState.WaitingPick);
                break;

            case CutStageState.Outputting:
                // Output dở thì cho output lại sau khi reset
                SetStageState(slot, CutStageState.ReadyToOutput);
                break;

            case CutStageState.Empty:
                // Có material nhưng state Empty là bất thường, đưa về WaitingCut
                ResetStageVisual(slot);
                SetStageState(slot, CutStageState.WaitingCut);
                break;

            default:
                // WaitingCut / WaitingPick / ReadyToOutput giữ nguyên được
                break;
        }
    }

    public void SetSorterRunningState(bool isRunning)
    {
        _state.IsInConveyorRunning = isRunning;
        _state.IsFeed1Running = isRunning;
        _state.IsFeed2Running = isRunning;
        _state.IsFeed3Running = isRunning;
        _state.IsOutConveyorRunning = isRunning;

        if (!isRunning)
        {
            _state.IsInLiftRunning = false;
            _state.IsOutLiftRunning = false;
            _state.IsNgConveyorRunning = false;
        }
    }

    public void RequestStop()
    {
        if (_isStopRequested)
            return;

        _isStopRequested = true;
        _isCycleStopRequested = false;
        RaiseStateChanged();
    }

    public void RequestCycleStop()
    {
        if (_isCycleStopRequested)
            return;

        _isCycleStopRequested = true;
        _isStopRequested = false;
        RaiseStateChanged();
    }

    public void ClearStopRequests()
    {
        _isStopRequested = false;
        _isCycleStopRequested = false;
        RaiseStateChanged();
    }

    public bool EnqueueWorkpiece(bool isOk)
    {
        // Không nhận thêm PCB khi Input Stop hoặc đang dừng
        if (_state.IsInputStopActive || _isStopRequested || _isCycleStopRequested)
            return false;

        // Không tạo queue ẩn. Chỉ nhận PCB nếu vị trí input thật sự trống
        if (_stage1Workpiece != null)
            return false;

        _stage1Workpiece = new SorterWorkpiece { IsOk = isOk };

        UpdateSorterSensors();
        RaiseStateChanged();

        return true;
    }

    public void InjectMaterialAtStage(int stageNumber, bool isOk = true)
    {
        var workpiece = new SorterWorkpiece { IsOk = isOk };

        switch (stageNumber)
        {
            case 1:
                if (_stage1Workpiece == null) _stage1Workpiece = workpiece;
                break;
            case 2:
                if (_stage2Workpiece == null) _stage2Workpiece = workpiece;
                break;
            case 3:
                if (_stage3Workpiece == null) _stage3Workpiece = workpiece;
                break;
            case 4:
                if (_stage4Workpiece == null) _stage4Workpiece = workpiece;
                break;
            case 5:
                if (_stage5Workpiece == null) _stage5Workpiece = workpiece;
                break;
        }

        UpdateSorterSensors();
        RaiseStateChanged();
    }

    public void ClearMaterialAtStage(int stageNumber)
    {
        switch (stageNumber)
        {
            case 1:
                _stage1Workpiece = null;
                break;
            case 2:
                _stage2Workpiece = null;
                break;
            case 3:
                _stage3Workpiece = null;
                break;
            case 4:
                _stage4Workpiece = null;
                break;
            case 5:
                _stage5Workpiece = null;
                break;
        }

        UpdateSorterSensors();
        RaiseStateChanged();
    }

    public void ClearSorterIo()
    {
        SetSorterRunningState(false);

        _state.IsInConveyorSensorOn = false;
        _state.IsFeed1SensorOn = false;
        _state.IsFeed2SensorOn = false;
        _state.IsFeed3SensorOn = false;
        _state.IsOutConveyorSensorOn = false;

        _stage1Workpiece = null;
        _stage2Workpiece = null;
        _stage3Workpiece = null;
        _stage4Workpiece = null;
        _stage5Workpiece = null;
        _cutStageAWorkpiece = null;
        _cutStageBWorkpiece = null;

        _pendingWorkpieces.Clear();

        _stageAState = CutStageState.Empty;
        _stageBState = CutStageState.Empty;

        _laserBusy = false;
        _pickerBusy = false;
        _feed3OutputBusy = false;

        _lastLaserServed = CutStageSlot.None;
        _lastPickerServed = CutStageSlot.None;
        _lastOutputServed = CutStageSlot.None;

        _feed1StopperDownPulseTicks = 0;
        _state.IsFeed1StopperUp = true;

        ResetStageVisual(CutStageSlot.A);
        ResetStageVisual(CutStageSlot.B);

        ClearStopRequests();
        RaiseStateChanged();
    }

    public void UpdateSorterSensors()
    {
        _state.IsInConveyorSensorOn = IsMaterialAtInConveyor;
        _state.IsFeed1SensorOn = IsMaterialAtFeed1;
        _state.IsFeed2SensorOn = IsMaterialAtFeed2;
        _state.IsFeed3SensorOn = IsMaterialAtFeed3;
        _state.IsOutConveyorSensorOn = IsMaterialAtOutConveyor;
    }

    public async Task ExecuteOutTrayBoxRequestAsync(Func<bool, Task> processTrayOutputAsync)
    {
        if (_isOutTrayBoxProcessing)
            return;

        _isOutTrayBoxProcessing = true;

        try
        {
            await processTrayOutputAsync(true);
        }
        finally
        {
            _state.IsOutTrayBoxRequested = false;
            _isOutTrayBoxProcessing = false;
            RaiseStateChanged();
        }
    }

    // ===== Pipeline =====

    private async Task RunSorterPipelineAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && IsRunning)
            {
                AdvanceSorterPipeline();
                await Task.Delay(SorterStepIntervalMs, token);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void AdvanceSorterPipeline()
    {
        if (_stage5Workpiece != null)
        {
            bool finishedResult = _stage5Workpiece.IsOk;
            _stage5Workpiece = null;

            if (HandleFinishedWorkpieceAsync != null)
            {
                _ = HandleFinishedWorkpieceAsync(finishedResult);
            }
        }

        if (_stage5Workpiece == null && _stage4Workpiece != null)
        {
            _stage5Workpiece = _stage4Workpiece;
            _stage4Workpiece = null;
        }

        RunCutStageSchedulerTick();

        if (CanFeed1MoveToFeed2Buffer())
        {
            PulseFeed1StopperDown();
            _stage3Workpiece = _stage2Workpiece;
            _stage2Workpiece = null;
        }

        if (_stage2Workpiece == null && _stage1Workpiece != null)
        {
            _stage2Workpiece = _stage1Workpiece;
            _stage1Workpiece = null;
        }

        UpdateFeed1StopperState();
        UpdateSorterSensors();
        RaiseStateChanged();
        TryFinalizeRequestedStop();
    }

    private void RunCutStageSchedulerTick()
    {
        TryLoadFeed2BufferToCutStage();
        TryStartLaserScheduler();
        TryStartPickerScheduler();
        TryStartOutputScheduler();
    }

    // ===== Scheduler =====

    private void TryLoadFeed2BufferToCutStage()
    {
        if (_isStopRequested)
            return;

        if (_stage3Workpiece == null)
            return;

        if (IsStageEmpty(CutStageSlot.A) && GetStageState(CutStageSlot.A) == CutStageState.Empty)
        {
            SetCutStageWorkpiece(CutStageSlot.A, _stage3Workpiece);
            _stage3Workpiece = null;
            SetStageState(CutStageSlot.A, CutStageState.WaitingCut);
            RaiseStateChanged();
            return;
        }

        if (IsStageEmpty(CutStageSlot.B) && GetStageState(CutStageSlot.B) == CutStageState.Empty)
        {
            SetCutStageWorkpiece(CutStageSlot.B, _stage3Workpiece);
            _stage3Workpiece = null;
            SetStageState(CutStageSlot.B, CutStageState.WaitingCut);
            RaiseStateChanged();
        }
    }

    private void TryStartLaserScheduler()
    {
        if (_laserBusy)
            return;

        var next = ChooseRoundRobin(
            GetStageState(CutStageSlot.A) == CutStageState.WaitingCut,
            GetStageState(CutStageSlot.B) == CutStageState.WaitingCut,
            _lastLaserServed);

        if (next == CutStageSlot.None)
            return;

        _laserBusy = true;
        _lastLaserServed = next;
        _ = RunLaserCutAsync(next, _runtimeCts.Token);
        RaiseStateChanged();
    }

    private void TryStartPickerScheduler()
    {
        if (_isStopRequested)
            return;

        if (_pickerBusy)
            return;

        var next = ChooseRoundRobin(
            GetStageState(CutStageSlot.A) == CutStageState.WaitingPick,
            GetStageState(CutStageSlot.B) == CutStageState.WaitingPick,
            _lastPickerServed);

        if (next == CutStageSlot.None)
            return;

        _pickerBusy = true;
        _lastPickerServed = next;
        _ = RunPickerUnloadAsync(next, _runtimeCts.Token);
        RaiseStateChanged();
    }

    private void TryStartOutputScheduler()
    {
        if (_feed3OutputBusy)
            return;

        if (_stage4Workpiece != null)
            return;

        var next = ChooseRoundRobin(
            GetStageState(CutStageSlot.A) == CutStageState.ReadyToOutput,
            GetStageState(CutStageSlot.B) == CutStageState.ReadyToOutput,
            _lastOutputServed);

        if (next == CutStageSlot.None)
            return;

        _feed3OutputBusy = true;
        _lastOutputServed = next;
        _ = RunOutputToFeed3Async(next, _runtimeCts.Token);
        RaiseStateChanged();
    }

    // ===== Runtime async =====

    private async Task RunLaserCutAsync(CutStageSlot slot, CancellationToken token)
    {
        var order = slot == CutStageSlot.A ? _stageACutOrder : _stageBCutOrder;

        try
        {
            token.ThrowIfCancellationRequested();

            ResetStageVisual(slot);
            SetStageState(slot, CutStageState.Cutting);

            foreach (var cutNumber in order)
            {
                token.ThrowIfCancellationRequested();

                SetActiveCut(slot, cutNumber);
                await Task.Delay(SorterStepIntervalMs, token);

                token.ThrowIfCancellationRequested();

                MarkCutCompleted(slot, cutNumber);

                SetActiveCut(slot, 0);
                await Task.Delay(40, token);
            }

            token.ThrowIfCancellationRequested();

            SetStageState(slot, CutStageState.WaitingPick);
        }
        catch (OperationCanceledException)
        {
            // Emergency stop / alarm: do not continue cutting
        }
        finally
        {
            SetActiveCut(slot, 0);
            _laserBusy = false;
            RaiseStateChanged();
        }
    }

    private async Task RunPickerUnloadAsync(CutStageSlot slot, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            SetStageState(slot, CutStageState.Picking);
            SetStageUnloadWaiting(slot, true);

            await Task.Delay(PickerUnloadMs, token);

            token.ThrowIfCancellationRequested();

            MarkAllPicked(slot);
            SetStageUnloadWaiting(slot, false);

            SetStageState(slot, CutStageState.ReadyToOutput);
        }
        catch (OperationCanceledException)
        {
            // Emergency stop / alarm: do not continue picker unload
        }
        finally
        {
            SetStageUnloadWaiting(slot, false);
            _pickerBusy = false;
            RaiseStateChanged();
        }
    }

    private async Task RunOutputToFeed3Async(CutStageSlot slot, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            SetStageState(slot, CutStageState.Outputting);

            await Task.Delay(Math.Max(150, SorterStepIntervalMs / 2), token);

            token.ThrowIfCancellationRequested();

            _stage4Workpiece = GetCutStageWorkpiece(slot);
            SetCutStageWorkpiece(slot, null);

            ResetStageVisual(slot);
            SetStageState(slot, CutStageState.Empty);

            RaiseStateChanged();
        }
        catch (OperationCanceledException)
        {
            // Emergency stop / alarm: do not move material to output
        }
        finally
        {
            _feed3OutputBusy = false;
            RaiseStateChanged();
        }
    }

    // ===== Stopper / stop finalize =====

    private bool CanFeed1MoveToFeed2Buffer()
    {
        if (_stage2Workpiece == null) return false;
        if (_stage3Workpiece != null) return false;
        if (_isStopRequested) return false;

        return true;
    }

    private void PulseFeed1StopperDown()
    {
        _feed1StopperDownPulseTicks = 1;
    }

    private void UpdateFeed1StopperState()
    {
        if (_feed1StopperDownPulseTicks > 0)
        {
            _state.IsFeed1StopperUp = false;
            _feed1StopperDownPulseTicks--;
            return;
        }

        _state.IsFeed1StopperUp = true;
    }

    private bool HasActiveSorterProcess()
    {
        return _laserBusy ||
               _pickerBusy ||
               _feed3OutputBusy ||
               _stageAState == CutStageState.Cutting ||
               _stageAState == CutStageState.Picking ||
               _stageAState == CutStageState.Outputting ||
               _stageBState == CutStageState.Cutting ||
               _stageBState == CutStageState.Picking ||
               _stageBState == CutStageState.Outputting;
    }

    private bool IsSorterFullyDrained()
    {
        return _pendingWorkpieces.Count == 0 &&
               _stage1Workpiece == null &&
               _stage2Workpiece == null &&
               _stage3Workpiece == null &&
               _stage4Workpiece == null &&
               _stage5Workpiece == null &&
               !HasCutStageAMaterial &&
               !HasCutStageBMaterial;
    }

    private async Task FinalizeRequestedStopAsync(string message)
    {
        if (_isStopFinalizing)
            return;

        _isStopFinalizing = true;

        try
        {
            StopPipelineLoop();

            _isStopRequested = false;
            _isCycleStopRequested = false;
            RaiseStateChanged();

            if (FinalizeStopAsync != null)
            {
                await FinalizeStopAsync(message);
            }
        }
        finally
        {
            _isStopFinalizing = false;
        }
    }

    private void TryFinalizeRequestedStop()
    {
        if (_isStopRequested)
        {
            bool canFinalize =
                !HasActiveSorterProcess() &&
                _stage4Workpiece == null &&
                _stage5Workpiece == null;

            if (canFinalize)
            {
                _ = FinalizeRequestedStopAsync("Machine stopped.");
            }

            return;
        }

        if (_isCycleStopRequested)
        {
            bool canFinalize =
                !HasActiveSorterProcess() &&
                IsSorterFullyDrained();

            if (canFinalize)
            {
                _ = FinalizeRequestedStopAsync("Machine cycle-stopped.");
            }
        }
    }

    // ===== Internal helpers =====

    private CutStageState GetStageState(CutStageSlot slot) =>
        slot == CutStageSlot.A ? _stageAState :
        slot == CutStageSlot.B ? _stageBState :
        CutStageState.Empty;

    private void SetStageState(CutStageSlot slot, CutStageState state)
    {
        if (slot == CutStageSlot.A)
            _stageAState = state;
        else if (slot == CutStageSlot.B)
            _stageBState = state;

        RaiseStateChanged();
    }

    private SorterWorkpiece? GetCutStageWorkpiece(CutStageSlot slot) =>
        slot == CutStageSlot.A ? _cutStageAWorkpiece :
        slot == CutStageSlot.B ? _cutStageBWorkpiece :
        null;

    private void SetCutStageWorkpiece(CutStageSlot slot, SorterWorkpiece? workpiece)
    {
        if (slot == CutStageSlot.A)
            _cutStageAWorkpiece = workpiece;
        else if (slot == CutStageSlot.B)
            _cutStageBWorkpiece = workpiece;
    }

    private bool IsStageEmpty(CutStageSlot slot) => GetCutStageWorkpiece(slot) == null;

    private CutStageSlot ChooseRoundRobin(bool aEligible, bool bEligible, CutStageSlot lastServed)
    {
        if (aEligible && bEligible)
            return lastServed == CutStageSlot.A ? CutStageSlot.B : CutStageSlot.A;

        if (aEligible) return CutStageSlot.A;
        if (bEligible) return CutStageSlot.B;

        return CutStageSlot.None;
    }

    private void SetStageUnloadWaiting(CutStageSlot slot, bool value)
    {
        if (slot == CutStageSlot.A)
            _isStageAUnloadWaiting = value;
        else if (slot == CutStageSlot.B)
            _isStageBUnloadWaiting = value;

        RaiseStateChanged();
    }

    private void ResetStageVisual(CutStageSlot slot)
    {
        if (slot == CutStageSlot.A)
        {
            _activeCutNumberStageA = 0;
            _stageACompletedCuts.Clear();
            _stageAPickedCuts.Clear();
            _isStageAUnloadWaiting = false;
        }
        else if (slot == CutStageSlot.B)
        {
            _activeCutNumberStageB = 0;
            _stageBCompletedCuts.Clear();
            _stageBPickedCuts.Clear();
            _isStageBUnloadWaiting = false;
        }

        RaiseStateChanged();
    }

    private void MarkCutCompleted(CutStageSlot slot, int cutNumber)
    {
        if (slot == CutStageSlot.A)
        {
            _stageACompletedCuts.Add(cutNumber);
        }
        else if (slot == CutStageSlot.B)
        {
            _stageBCompletedCuts.Add(cutNumber);
        }

        RaiseStateChanged();
    }

    private void MarkAllPicked(CutStageSlot slot)
    {
        var order = slot == CutStageSlot.A ? _stageACutOrder : _stageBCutOrder;

        if (slot == CutStageSlot.A)
        {
            _stageAPickedCuts.Clear();
            foreach (var n in order) _stageAPickedCuts.Add(n);
            _stageACompletedCuts.Clear();
        }
        else if (slot == CutStageSlot.B)
        {
            _stageBPickedCuts.Clear();
            foreach (var n in order) _stageBPickedCuts.Add(n);
            _stageBCompletedCuts.Clear();
        }

        RaiseStateChanged();
    }

    private void SetActiveCut(CutStageSlot slot, int cutNumber)
    {
        if (slot == CutStageSlot.A)
        {
            _activeCutNumberStageA = cutNumber;
        }
        else if (slot == CutStageSlot.B)
        {
            _activeCutNumberStageB = cutNumber;
        }

        RaiseStateChanged();
    }
}