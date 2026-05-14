using System.Text;
using IndustrialMachineSimulator.Core.Entities;
using IndustrialMachineSimulator.Core.Interfaces;

namespace IndustrialMachineSimulator.Infrastructure.Logging;

public class OperationFileLogger : IOperationFileLogger
{
    private readonly string _logFolderPath;

    public OperationFileLogger()
    {
        _logFolderPath = Path.Combine(AppContext.BaseDirectory, "Logs", "Operation");
        Directory.CreateDirectory(_logFolderPath);
    }

    public async Task WriteAsync(OperationLogRecord record)
    {
        Directory.CreateDirectory(_logFolderPath);

        var filePath = Path.Combine(
            _logFolderPath,
            $"{record.Timestamp:yyyy-MM-dd}_operation.log");

        var line =
            $"{record.Timestamp:yyyy-MM-dd HH:mm:ss} | {record.Category} | {record.Message}{Environment.NewLine}";

        await File.AppendAllTextAsync(filePath, line, Encoding.UTF8);
    }
}