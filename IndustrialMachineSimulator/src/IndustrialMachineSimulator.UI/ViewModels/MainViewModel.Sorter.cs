using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IndustrialMachineSimulator.UI.ViewModels;

public partial class MainViewModel
{
    // ===== Bridge state change from SorterState =====

    private void Sorter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName))
            return;

        OnPropertyChanged(e.PropertyName);

        if (e.PropertyName == nameof(SorterState.IsFeed1StopperUp))
        {
            OnPropertyChanged(nameof(IsFeed1StopperDown));
            OnPropertyChanged(nameof(Feed1StopperText));
        }
    }

    // ===== Bridge state change from SorterEngine =====

    private void SorterEngine_StateChanged(object? sender, EventArgs e)
    {
        NotifySorterMaterialChanged();
        NotifyCutStageSlotsChanged();

        NotifyStageACutChanged();
        NotifyStageBCutChanged();

        NotifyStageADoneChanged();
        NotifyStageBDoneChanged();

        NotifyStageAPickedChanged();
        NotifyStageBPickedChanged();

        NotifySchedulerDebugChanged();
        NotifyStopRequestUiChanged();

        OnPropertyChanged(nameof(ActiveCutNumberStageA));
        OnPropertyChanged(nameof(ActiveCutNumberStageB));
        OnPropertyChanged(nameof(IsStageAUnloadWaiting));
        OnPropertyChanged(nameof(IsStageBUnloadWaiting));
        OnPropertyChanged(nameof(HasAnyMaterialInSorter));
    }

    // ===== Notify helpers =====

    private void NotifyStopRequestUiChanged()
    {
        OnPropertyChanged(nameof(IsStopRequestedActive));
        OnPropertyChanged(nameof(IsCycleStopRequestedActive));
        OnPropertyChanged(nameof(HasPendingStopCommand));
        OnPropertyChanged(nameof(PendingStopCommandText));
        OnPropertyChanged(nameof(StopModeText));
    }

    private void NotifySchedulerDebugChanged()
    {
        OnPropertyChanged(nameof(LaserBusyText));
        OnPropertyChanged(nameof(PickerBusyText));
        OnPropertyChanged(nameof(OutputBusyText));
        OnPropertyChanged(nameof(StageAStateText));
        OnPropertyChanged(nameof(StageBStateText));
        OnPropertyChanged(nameof(Feed1StopperText));
        OnPropertyChanged(nameof(IsFeed1StopperUp));
        OnPropertyChanged(nameof(IsFeed1StopperDown));
    }

    private void NotifySorterMaterialChanged()
    {
        OnPropertyChanged(nameof(IsMaterialAtInConveyor));
        OnPropertyChanged(nameof(IsMaterialAtFeed1));
        OnPropertyChanged(nameof(IsMaterialAtFeed2));
        OnPropertyChanged(nameof(IsMaterialAtFeed3));
        OnPropertyChanged(nameof(IsMaterialAtOutConveyor));
        OnPropertyChanged(nameof(HasAnyMaterialInSorter));
    }

    private void NotifyCutStageSlotsChanged()
    {
        OnPropertyChanged(nameof(HasCutStageAMaterial));
        OnPropertyChanged(nameof(HasCutStageBMaterial));
        OnPropertyChanged(nameof(HasAnyMaterialInSorter));
    }

    private void NotifyStageACutChanged()
    {
        OnPropertyChanged(nameof(ActiveCutNumberStageA));

        OnPropertyChanged(nameof(IsStageAIndex1Active));
        OnPropertyChanged(nameof(IsStageAIndex2Active));
        OnPropertyChanged(nameof(IsStageAIndex3Active));
        OnPropertyChanged(nameof(IsStageAIndex4Active));
        OnPropertyChanged(nameof(IsStageAIndex5Active));
        OnPropertyChanged(nameof(IsStageAIndex6Active));
        OnPropertyChanged(nameof(IsStageAIndex7Active));
        OnPropertyChanged(nameof(IsStageAIndex8Active));
        OnPropertyChanged(nameof(IsStageAIndex9Active));
    }

    private void NotifyStageBCutChanged()
    {
        OnPropertyChanged(nameof(ActiveCutNumberStageB));

        OnPropertyChanged(nameof(IsStageBIndex1Active));
        OnPropertyChanged(nameof(IsStageBIndex2Active));
        OnPropertyChanged(nameof(IsStageBIndex3Active));
        OnPropertyChanged(nameof(IsStageBIndex4Active));
        OnPropertyChanged(nameof(IsStageBIndex5Active));
        OnPropertyChanged(nameof(IsStageBIndex6Active));
        OnPropertyChanged(nameof(IsStageBIndex7Active));
        OnPropertyChanged(nameof(IsStageBIndex8Active));
        OnPropertyChanged(nameof(IsStageBIndex9Active));
    }

    private void NotifyStageADoneChanged()
    {
        OnPropertyChanged(nameof(IsStageAIndex1Done));
        OnPropertyChanged(nameof(IsStageAIndex2Done));
        OnPropertyChanged(nameof(IsStageAIndex3Done));
        OnPropertyChanged(nameof(IsStageAIndex4Done));
        OnPropertyChanged(nameof(IsStageAIndex5Done));
        OnPropertyChanged(nameof(IsStageAIndex6Done));
        OnPropertyChanged(nameof(IsStageAIndex7Done));
        OnPropertyChanged(nameof(IsStageAIndex8Done));
        OnPropertyChanged(nameof(IsStageAIndex9Done));
    }

    private void NotifyStageBDoneChanged()
    {
        OnPropertyChanged(nameof(IsStageBIndex1Done));
        OnPropertyChanged(nameof(IsStageBIndex2Done));
        OnPropertyChanged(nameof(IsStageBIndex3Done));
        OnPropertyChanged(nameof(IsStageBIndex4Done));
        OnPropertyChanged(nameof(IsStageBIndex5Done));
        OnPropertyChanged(nameof(IsStageBIndex6Done));
        OnPropertyChanged(nameof(IsStageBIndex7Done));
        OnPropertyChanged(nameof(IsStageBIndex8Done));
        OnPropertyChanged(nameof(IsStageBIndex9Done));
    }

    private void NotifyStageAPickedChanged()
    {
        OnPropertyChanged(nameof(IsStageAIndex1Picked));
        OnPropertyChanged(nameof(IsStageAIndex2Picked));
        OnPropertyChanged(nameof(IsStageAIndex3Picked));
        OnPropertyChanged(nameof(IsStageAIndex4Picked));
        OnPropertyChanged(nameof(IsStageAIndex5Picked));
        OnPropertyChanged(nameof(IsStageAIndex6Picked));
        OnPropertyChanged(nameof(IsStageAIndex7Picked));
        OnPropertyChanged(nameof(IsStageAIndex8Picked));
        OnPropertyChanged(nameof(IsStageAIndex9Picked));
    }

    private void NotifyStageBPickedChanged()
    {
        OnPropertyChanged(nameof(IsStageBIndex1Picked));
        OnPropertyChanged(nameof(IsStageBIndex2Picked));
        OnPropertyChanged(nameof(IsStageBIndex3Picked));
        OnPropertyChanged(nameof(IsStageBIndex4Picked));
        OnPropertyChanged(nameof(IsStageBIndex5Picked));
        OnPropertyChanged(nameof(IsStageBIndex6Picked));
        OnPropertyChanged(nameof(IsStageBIndex7Picked));
        OnPropertyChanged(nameof(IsStageBIndex8Picked));
        OnPropertyChanged(nameof(IsStageBIndex9Picked));
    }

    // ===== Engine wrappers =====

    private void StartSorterPipelineLoop() => _sorterEngine.StartPipelineLoop();

    private void StopSorterPipelineLoop() => _sorterEngine.StopPipelineLoop();

    private void SetSorterRunningState(bool isRunning) => _sorterEngine.SetSorterRunningState(isRunning);

    private void UpdateSorterSensors() => _sorterEngine.UpdateSorterSensors();

    private void ClearSorterIo() => _sorterEngine.ClearSorterIo();

    private void InjectMaterialAtInput() => _sorterEngine.InjectMaterialAtStage(1);

    private void InjectMaterialAtFeed1() => _sorterEngine.InjectMaterialAtStage(2);

    private void InjectMaterialAtFeed2() => _sorterEngine.InjectMaterialAtStage(3);

    private void InjectMaterialAtFeed3() => _sorterEngine.InjectMaterialAtStage(4);

    private void InjectMaterialAtOutConveyor() => _sorterEngine.InjectMaterialAtStage(5);

    private void ClearMaterialAtStage(int stageNumber) => _sorterEngine.ClearMaterialAtStage(stageNumber);

    private Task ExecuteOutTrayBoxRequestAsync() =>
        _sorterEngine.ExecuteOutTrayBoxRequestAsync(ProcessTrayOutputAsync);

    private void RequestStop() => _sorterEngine.RequestStop();

    private void RequestCycleStop() => _sorterEngine.RequestCycleStop();

    private void ClearStopRequests() => _sorterEngine.ClearStopRequests();

    // ===== SorterState-backed properties =====

    public bool IsInConveyorRunning
    {
        get => Sorter.IsInConveyorRunning;
        set => Sorter.IsInConveyorRunning = value;
    }

    public bool IsFeed1Running
    {
        get => Sorter.IsFeed1Running;
        set => Sorter.IsFeed1Running = value;
    }

    public bool IsFeed2Running
    {
        get => Sorter.IsFeed2Running;
        set => Sorter.IsFeed2Running = value;
    }

    public bool IsFeed3Running
    {
        get => Sorter.IsFeed3Running;
        set => Sorter.IsFeed3Running = value;
    }

    public bool IsOutConveyorRunning
    {
        get => Sorter.IsOutConveyorRunning;
        set => Sorter.IsOutConveyorRunning = value;
    }

    public bool IsInConveyorSensorOn
    {
        get => Sorter.IsInConveyorSensorOn;
        set => Sorter.IsInConveyorSensorOn = value;
    }

    public bool IsFeed1SensorOn
    {
        get => Sorter.IsFeed1SensorOn;
        set => Sorter.IsFeed1SensorOn = value;
    }

    public bool IsFeed2SensorOn
    {
        get => Sorter.IsFeed2SensorOn;
        set => Sorter.IsFeed2SensorOn = value;
    }

    public bool IsFeed3SensorOn
    {
        get => Sorter.IsFeed3SensorOn;
        set => Sorter.IsFeed3SensorOn = value;
    }

    public bool IsOutConveyorSensorOn
    {
        get => Sorter.IsOutConveyorSensorOn;
        set => Sorter.IsOutConveyorSensorOn = value;
    }

    public bool IsInLiftRunning
    {
        get => Sorter.IsInLiftRunning;
        set => Sorter.IsInLiftRunning = value;
    }

    public bool IsOutLiftRunning
    {
        get => Sorter.IsOutLiftRunning;
        set => Sorter.IsOutLiftRunning = value;
    }

    public bool IsNgConveyorRunning
    {
        get => Sorter.IsNgConveyorRunning;
        set => Sorter.IsNgConveyorRunning = value;
    }

    public int CurrentTrayOkCount
    {
        get => Sorter.CurrentTrayOkCount;
        set => Sorter.CurrentTrayOkCount = value;
    }

    public int TrayCapacity
    {
        get => Sorter.TrayCapacity;
        set => Sorter.TrayCapacity = value;
    }

    public int AvailableEmptyTrayCount
    {
        get => Sorter.AvailableEmptyTrayCount;
        set => Sorter.AvailableEmptyTrayCount = value;
    }

    public int ProducedFullTrayCount
    {
        get => Sorter.ProducedFullTrayCount;
        set => Sorter.ProducedFullTrayCount = value;
    }

    public bool HasEmptyTrayLoaded
    {
        get => Sorter.HasEmptyTrayLoaded;
        set => Sorter.HasEmptyTrayLoaded = value;
    }

    public bool IsStageMaterialOkLampOn
    {
        get => Sorter.IsStageMaterialOkLampOn;
        set => Sorter.IsStageMaterialOkLampOn = value;
    }

    public bool IsStageMaterialNgLampOn
    {
        get => Sorter.IsStageMaterialNgLampOn;
        set => Sorter.IsStageMaterialNgLampOn = value;
    }

    public bool IsInputStopActive
    {
        get => Sorter.IsInputStopActive;
        set
        {
            if (Sorter.IsInputStopActive == value) return;
            Sorter.IsInputStopActive = value;

            _ = AddOperationLogAsync(
                "Input",
                value ? "Input stop activated." : "Input stop released.");
        }
    }

    public bool IsOutTrayBoxRequested
    {
        get => Sorter.IsOutTrayBoxRequested;
        set
        {
            if (Sorter.IsOutTrayBoxRequested == value) return;
            Sorter.IsOutTrayBoxRequested = value;

            if (value)
            {
                _ = ExecuteOutTrayBoxRequestAsync();
            }
        }
    }

    public bool IsFeed1StopperUp
    {
        get => Sorter.IsFeed1StopperUp;
        set => Sorter.IsFeed1StopperUp = value;
    }

    public bool IsFeed1StopperDown => Sorter.IsFeed1StopperDown;

    // ===== Engine-backed properties =====

    public int ActiveCutNumberStageA => _sorterEngine.ActiveCutNumberStageA;
    public int ActiveCutNumberStageB => _sorterEngine.ActiveCutNumberStageB;

    public bool IsStageAUnloadWaiting => _sorterEngine.IsStageAUnloadWaiting;
    public bool IsStageBUnloadWaiting => _sorterEngine.IsStageBUnloadWaiting;

    public bool HasCutStageAMaterial => _sorterEngine.HasCutStageAMaterial;
    public bool HasCutStageBMaterial => _sorterEngine.HasCutStageBMaterial;

    public bool IsMaterialAtInConveyor => _sorterEngine.IsMaterialAtInConveyor;
    public bool IsMaterialAtFeed1 => _sorterEngine.IsMaterialAtFeed1;
    public bool IsMaterialAtFeed2 => _sorterEngine.IsMaterialAtFeed2;
    public bool IsMaterialAtFeed3 => _sorterEngine.IsMaterialAtFeed3;
    public bool IsMaterialAtOutConveyor => _sorterEngine.IsMaterialAtOutConveyor;

    public bool HasAnyMaterialInSorter => _sorterEngine.HasAnyMaterialInSorter;

    public bool IsStageAIndex1Picked => _sorterEngine.IsStageAPicked(1);
    public bool IsStageAIndex2Picked => _sorterEngine.IsStageAPicked(2);
    public bool IsStageAIndex3Picked => _sorterEngine.IsStageAPicked(3);
    public bool IsStageAIndex4Picked => _sorterEngine.IsStageAPicked(4);
    public bool IsStageAIndex5Picked => _sorterEngine.IsStageAPicked(5);
    public bool IsStageAIndex6Picked => _sorterEngine.IsStageAPicked(6);
    public bool IsStageAIndex7Picked => _sorterEngine.IsStageAPicked(7);
    public bool IsStageAIndex8Picked => _sorterEngine.IsStageAPicked(8);
    public bool IsStageAIndex9Picked => _sorterEngine.IsStageAPicked(9);

    public bool IsStageBIndex1Picked => _sorterEngine.IsStageBPicked(1);
    public bool IsStageBIndex2Picked => _sorterEngine.IsStageBPicked(2);
    public bool IsStageBIndex3Picked => _sorterEngine.IsStageBPicked(3);
    public bool IsStageBIndex4Picked => _sorterEngine.IsStageBPicked(4);
    public bool IsStageBIndex5Picked => _sorterEngine.IsStageBPicked(5);
    public bool IsStageBIndex6Picked => _sorterEngine.IsStageBPicked(6);
    public bool IsStageBIndex7Picked => _sorterEngine.IsStageBPicked(7);
    public bool IsStageBIndex8Picked => _sorterEngine.IsStageBPicked(8);
    public bool IsStageBIndex9Picked => _sorterEngine.IsStageBPicked(9);

    public bool IsStageAIndex1Done => _sorterEngine.IsStageADone(1);
    public bool IsStageAIndex2Done => _sorterEngine.IsStageADone(2);
    public bool IsStageAIndex3Done => _sorterEngine.IsStageADone(3);
    public bool IsStageAIndex4Done => _sorterEngine.IsStageADone(4);
    public bool IsStageAIndex5Done => _sorterEngine.IsStageADone(5);
    public bool IsStageAIndex6Done => _sorterEngine.IsStageADone(6);
    public bool IsStageAIndex7Done => _sorterEngine.IsStageADone(7);
    public bool IsStageAIndex8Done => _sorterEngine.IsStageADone(8);
    public bool IsStageAIndex9Done => _sorterEngine.IsStageADone(9);

    public bool IsStageBIndex1Done => _sorterEngine.IsStageBDone(1);
    public bool IsStageBIndex2Done => _sorterEngine.IsStageBDone(2);
    public bool IsStageBIndex3Done => _sorterEngine.IsStageBDone(3);
    public bool IsStageBIndex4Done => _sorterEngine.IsStageBDone(4);
    public bool IsStageBIndex5Done => _sorterEngine.IsStageBDone(5);
    public bool IsStageBIndex6Done => _sorterEngine.IsStageBDone(6);
    public bool IsStageBIndex7Done => _sorterEngine.IsStageBDone(7);
    public bool IsStageBIndex8Done => _sorterEngine.IsStageBDone(8);
    public bool IsStageBIndex9Done => _sorterEngine.IsStageBDone(9);

    public bool IsStageAIndex1Active => _sorterEngine.IsStageAActive(1);
    public bool IsStageAIndex2Active => _sorterEngine.IsStageAActive(2);
    public bool IsStageAIndex3Active => _sorterEngine.IsStageAActive(3);
    public bool IsStageAIndex4Active => _sorterEngine.IsStageAActive(4);
    public bool IsStageAIndex5Active => _sorterEngine.IsStageAActive(5);
    public bool IsStageAIndex6Active => _sorterEngine.IsStageAActive(6);
    public bool IsStageAIndex7Active => _sorterEngine.IsStageAActive(7);
    public bool IsStageAIndex8Active => _sorterEngine.IsStageAActive(8);
    public bool IsStageAIndex9Active => _sorterEngine.IsStageAActive(9);

    public bool IsStageBIndex1Active => _sorterEngine.IsStageBActive(1);
    public bool IsStageBIndex2Active => _sorterEngine.IsStageBActive(2);
    public bool IsStageBIndex3Active => _sorterEngine.IsStageBActive(3);
    public bool IsStageBIndex4Active => _sorterEngine.IsStageBActive(4);
    public bool IsStageBIndex5Active => _sorterEngine.IsStageBActive(5);
    public bool IsStageBIndex6Active => _sorterEngine.IsStageBActive(6);
    public bool IsStageBIndex7Active => _sorterEngine.IsStageBActive(7);
    public bool IsStageBIndex8Active => _sorterEngine.IsStageBActive(8);
    public bool IsStageBIndex9Active => _sorterEngine.IsStageBActive(9);

    public string LaserBusyText => _sorterEngine.LaserBusyText;
    public string PickerBusyText => _sorterEngine.PickerBusyText;
    public string OutputBusyText => _sorterEngine.OutputBusyText;

    public string StopModeText => _sorterEngine.StopModeText;
    public bool IsStopRequestedActive => _sorterEngine.IsStopRequestedActive;
    public bool IsCycleStopRequestedActive => _sorterEngine.IsCycleStopRequestedActive;
    public bool HasPendingStopCommand => _sorterEngine.HasPendingStopCommand;
    public string PendingStopCommandText => _sorterEngine.PendingStopCommandText;

    public string StageAStateText => _sorterEngine.StageAStateText;
    public string StageBStateText => _sorterEngine.StageBStateText;
    public string Feed1StopperText => _sorterEngine.Feed1StopperText;
}