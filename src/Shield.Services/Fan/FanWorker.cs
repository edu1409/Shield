using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Fan;

namespace Shield.Services.Fan
{
    /*
Parameters
Temp. (ºC)      Duty Cycle (%)  Threshold
< 40                    0           None
>= 40 and < 50         25           Low
>= 50 and < 60         50           Medium
>= 60 and < 70         75           High
>= 70                 100           Highest
*/

    public abstract class FanWorker<T>(ILogger logger,
        IFanService<T> fanService,
        ISharedMemoryService sharedMemoryService) where T : FanService<T>
    {
        protected readonly ILogger _logger = logger;
        protected readonly IFanService<T> _fanService = fanService;
        protected readonly ISharedMemoryService _sharedMemoryService = sharedMemoryService;

        public event EventHandler<CpuTemperatureThresholdChangedEventArgs>? CpuTemperatureThresholdChanged;
        public CpuTemperatureThreshold LastCpuTemperatureThreshold { get; set; } = CpuTemperatureThreshold.None;

        public virtual ServiceStatus FanStatus
        {
            get
            {
                return GetStatus();
            }
            set
            {
                _fanService.On = value == ServiceStatus.OnByService || value == ServiceStatus.OnByManual;
            }
        }

        public virtual double DutyCycle 
        { 
            set 
            {
                _fanService.DutyCycle = value;
            } 
        }

        public virtual void Update(ServiceStatus newStatus)
        {
            FanStatus = newStatus;
        }

        public virtual void ResetStatus()
        {
            FanStatus = ServiceStatus.OnByService;
        }



        public abstract ServiceStatus GetStatus();

        public virtual void Execute() => ExecuteAsync().Wait();

        public virtual async Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            this.CpuTemperatureThresholdChanged += FanWorker_CpuTemperatureThresholdChanged;
            
            _fanService.Start();
            
            FanStatus = ServiceStatus.OnByService;

            // 5 seconds cycles
            while (!stoppingToken.IsCancellationRequested)
            {
                // Automatic control
                if (FanStatus == ServiceStatus.OnByService)
                {
                    var temperature = await GetCpuTemperatureAsync(stoppingToken);
                    var temperatureThreshold = GetCpuTemperatureThreshold(temperature);

                    // Threshold has been changed. Triggers an event to change fan velocity.
                    if (temperatureThreshold != LastCpuTemperatureThreshold)
                    {
                        var args = new CpuTemperatureThresholdChangedEventArgs(temperatureThreshold);
                        CpuTemperatureThresholdChanged.Invoke(this, args);
                    }

                    //store read value to compare with next reading
                    LastCpuTemperatureThreshold = temperatureThreshold;
                }
                // Turned on manually => DutyCycle 100%
                else if (FanStatus == ServiceStatus.OnByManual)
                {
                    _fanService.On = true;
                   //_fanService.DutyCycle = 1;
                }
                else if (FanStatus == ServiceStatus.OffByService || FanStatus == ServiceStatus.OffByManual)
                {
                    _fanService.Stop();
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        protected async Task<double> GetCpuTemperatureAsync(CancellationToken stoppingToken)
        {
            string tempPath = "/sys/class/thermal/thermal_zone0/temp";
            string tempStr = await File.ReadAllTextAsync(tempPath, stoppingToken);

            if (double.TryParse(tempStr, out double tempMilliCelsius))
            {
                return tempMilliCelsius / 1000;
            }
            else
            {
                return double.MinValue;
            }
        }

        private void FanWorker_CpuTemperatureThresholdChanged(object? sender, CpuTemperatureThresholdChangedEventArgs e)
        {
            _fanService.DutyCycle = GetDutyCycleValue(e.Threshold);
        }

        private static CpuTemperatureThreshold GetCpuTemperatureThreshold(double temperature)
        {
            if (temperature < (int)CpuTemperatureThreshold.None) return CpuTemperatureThreshold.None;
            else if (temperature < (int)CpuTemperatureThreshold.Low) return CpuTemperatureThreshold.Low;
            else if (temperature < (int)CpuTemperatureThreshold.Medium) return CpuTemperatureThreshold.Medium;
            else if (temperature <= (int)CpuTemperatureThreshold.High) return CpuTemperatureThreshold.High;
            else return CpuTemperatureThreshold.Highest;
        }

        private static double GetDutyCycleValue(CpuTemperatureThreshold threshold) => threshold switch
        {
            CpuTemperatureThreshold.Low => 0.25,
            CpuTemperatureThreshold.Medium => 0.50,
            CpuTemperatureThreshold.High => 0.75,
            CpuTemperatureThreshold.Highest => 1,
            _ => 0,
        };

    }
}
