using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialMachineSimulator.Infrastructure.Data;

public class AlarmLogEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}