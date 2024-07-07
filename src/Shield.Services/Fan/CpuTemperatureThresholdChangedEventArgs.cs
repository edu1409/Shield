namespace Shield.Services.Fan
{
    public class CpuTemperatureThresholdChangedEventArgs(CpuTemperatureThreshold threshold) : EventArgs
    {
        public CpuTemperatureThreshold Threshold { get; private set; } = threshold;
    }
}
