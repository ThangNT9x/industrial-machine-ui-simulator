using System.Text;
using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Logging;

public class AlarmFileLogger : IAlarmFileLogger
{
    private readonly string _logFolderPath;

    public AlarmFileLogger()
    {
        _logFolderPath = Path.Combine(AppContext.BaseDirectory, "Logs", "Alarm");
        Directory.CreateDirectory(_logFolderPath);
    }

    public async Task WriteAsync(AlarmRecord record)
    {
        Directory.CreateDirectory(_logFolderPath);

        var filePath = Path.Combine(
            _logFolderPath,
            $"{record.Timestamp:yyyy-MM-dd}_alarm.log");

        var line =
            $"{record.Timestamp:yyyy-MM-dd HH:mm:ss} | {record.Code} | {record.Severity} | {record.Message}{Environment.NewLine}";

        await File.AppendAllTextAsync(filePath, line, Encoding.UTF8);
    }
}