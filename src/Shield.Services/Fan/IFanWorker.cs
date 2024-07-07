using Shield.Common.Interfaces;

namespace Shield.Services.Fan
{
    public interface IFanWorker : IWorkerService
    {
        event EventHandler<CpuTemperatureThresholdChangedEventArgs>? CpuTemperatureThresholdChanged;
        CpuTemperatureThreshold LastCpuTemperatureThreshold { get; set; }
        double DutyCycle { set; }
    }
}
