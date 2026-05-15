using IndustrialMachineSimulator.Core.Entities;

namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IConfigService
{
    MachineConfig Load();
    void Save(MachineConfig config);
}