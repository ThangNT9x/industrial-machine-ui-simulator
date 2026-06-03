using IndustrialMachineSimulator.Core.Entities;

namespace IndustrialMachineSimulator.Core.Interfaces;

public interface IMesClient
{
    MesConnectionState ConnectionState { get; }

    Task ConnectAsync();
    Task DisconnectAsync();
    Task SendAsync(string messageType, string payload);
}