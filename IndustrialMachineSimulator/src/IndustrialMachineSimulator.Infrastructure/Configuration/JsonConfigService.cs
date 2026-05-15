using System.Text.Json;
using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Configuration;

public class JsonConfigService : IConfigService
{
    private readonly string _configFolderPath;
    private readonly string _configFilePath;

    public JsonConfigService()
    {
        _configFolderPath = Path.Combine(AppContext.BaseDirectory, "Config");
        _configFilePath = Path.Combine(_configFolderPath, "machineconfig.json");

        Directory.CreateDirectory(_configFolderPath);
    }

    public MachineConfig Load()
    {
        if (!File.Exists(_configFilePath))
        {
            var defaultConfig = new MachineConfig();
            Save(defaultConfig);
            return defaultConfig;
        }

        var json = File.ReadAllText(_configFilePath);
        var config = JsonSerializer.Deserialize<MachineConfig>(json);

        return config ?? new MachineConfig();
    }

    public void Save(MachineConfig config)
    {
        Directory.CreateDirectory(_configFolderPath);

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_configFilePath, json);
    }
}