using IndustrialMachineSimulator.Core.Entities;
using System.Collections.ObjectModel;
using System.Threading;

namespace IndustrialMachineSimulator.UI.ViewModels;

public partial class MainViewModel
{
    //2 enum sorter
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

    //SorterWorkpiece
    private sealed class SorterWorkpiece
    {
        public bool IsOk { get; set; }
    }

    //field material pipeline
    private readonly Queue<SorterWorkpiece> _pendingWorkpieces = new();

    private SorterWorkpiece? _stage1Workpiece;
    private SorterWorkpiece? _stage2Workpiece;
    private SorterWorkpiece? _stage3Workpiece;
    private SorterWorkpiece? _stage4Workpiece;
    private SorterWorkpiece? _stage5Workpiece;
    private SorterWorkpiece? _cutStageAWorkpiece;
    private SorterWorkpiece? _cutStageBWorkpiece;

    //field scheduler/resource
    private CutStageState _stageAState = CutStageState.Empty;
    private CutStageState _stageBState = CutStageState.Empty;

    private bool _laserBusy;
    private bool _pickerBusy;
    private bool _feed3OutputBusy;

    private CutStageSlot _lastLaserServed = CutStageSlot.None;
    private CutStageSlot _lastPickerServed = CutStageSlot.None;
    private CutStageSlot _lastOutputServed = CutStageSlot.None;

    //field stopper và pipeline loop
    private int _feed1StopperDownPulseTicks;
    private CancellationTokenSource? _sorterPipelineCts;
    private readonly SorterIoState _sorterIo = new();


    //field stage visual
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

    //field stop-request backend
    private bool _isStopRequested;
    private bool _isCycleStopRequested;
    private bool _isStopFinalizing;


    //field tray/sorter runtime phụ
    private bool _isInLiftRunning;
    private bool _isOutLiftRunning;
    private bool _isNgConveyorRunning;
    private int _currentTrayOkCount;
    private int _trayCapacity = 20;
    private int _availableEmptyTrayCount = 100;
    private int _producedFullTrayCount;
    private bool _hasEmptyTrayLoaded = true;
    private bool _isInputStopActive;
    private bool _isOutTrayBoxRequested;
    private bool _isOutTrayBoxProcessing;
    private bool _isStageMaterialOkLampOn;
    private bool _isStageMaterialNgLampOn;

}