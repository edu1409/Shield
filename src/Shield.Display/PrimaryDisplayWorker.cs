using Shield.Common;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Lcd;
using UnitsNet;

namespace Shield.Display
{
    public class PrimaryDisplayWorker(ILogger<PrimaryDisplayWorker> logger,
        IDisplayService<Lcd20x4> displayService,
        IClimateSensorService climateSensorService,
        ISharedMemoryService sharedMemoryService) 
        : DisplayWorkerBase<Lcd20x4>(logger, displayService, sharedMemoryService), IPrimaryDisplayWorker
    {
        private readonly IClimateSensorService _climateSensorService = climateSensorService;

        public override DisplayBacklightStatus BacklightStatus
        {
            set
            {
                base.BacklightStatus = value;
                _sharedMemoryService.Write(value, Common.Domain.Lcd.Primary);
            }
        }

        public void Execute()
        {
            int climateWait = 0, retries = 0;

            Welcome();

            while (true)
            {
                try
                {
                    //Block process to control possible concurrency with Shield.Display.Backline execution
                    using (var mutex = Util.StartMutex())
                    {
                        UpdateTime();

                        //Update climate information each 15 minutes
                        if (climateWait % 15 == 0)
                        {
                            UpdateClimateInformation();
                            climateWait = 0;
                        }

                        ControlBacklightSchedule(Common.Domain.Lcd.Primary);

                        mutex.ReleaseMutex();
                    }

                    Task.Delay(TimeSpan.FromMinutes(1)).Wait();

                    retries = 0;
                    climateWait++;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("End of service!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{GetType().Name}: {Constants.DISPLAY_ERROR_STATE}");
                    retries++;
                    //Ends application after 3 consecutive exceptions
                    if (retries >= 3)
                    {
                        FatalError(ex);
                        throw;
                    }
                }
            }
        }

        public void Welcome()
        {
            var message = Constants.DISPLAY_WELCOME;

            BacklightStatus = DisplayBacklightStatus.OnByService;
            _cursor = new() { Left = 0, Top = 0 };

            for (int i = 0; i < message.Length; i++)
            {
                _cursor.Left = i;
                _displayService.Write(message.ElementAt(i).ToString(), _cursor);
                Task.Delay(100).Wait();
            }

            Task.Delay(1000).Wait();

            _displayService.Clear();

            Task.Delay(500).Wait();

            _cursor.Left = 0;
            _displayService.Write(message, _cursor);

            Task.Delay(2000).Wait();

            _displayService.Clear();

            BacklightStatus = DisplayBacklightStatus.OffByService;
        }

        public void UpdateTime()
        {
            _cursor = new() { Left = 0, Top = 0 };
            _displayService.Write(Constants.DISPLAY_TITLE, _cursor);
            _cursor.Top = 1;
            _displayService.Write($"{DateTimeOffset.Now:dd/MM/yyyy HH:mm}", _cursor);

            _logger.LogInformation(Constants.DISPLAY_DATE_TIME_UPDATED);
        }

        public void UpdateClimateInformation()
        {
            //get values from sensor
            var sensorReading = _climateSensorService.ReadAsync().Result;

            //Clear temperature and humidity lines
            _cursor = new() { Left = 0, Top = 3 };
            _displayService.Write("".PadRight(20), _cursor);
            _cursor.Top = 2;
            _displayService.Write("".PadRight(20), _cursor); 

            //await _displayService.SpinerAsync(new() { Left = 0, Top = 2 }, 1500);

            //then write new values
            _displayService.Write($"Temp: {(sensorReading.Temperature.Kelvins.Equals(0) ? Constants.DISPLAY_ERROR_STATE
                : sensorReading.Temperature.DegreesCelsius.ToString("0.#") + (char)1 + "C")}", _cursor);

            _cursor.Top = 3;
            _displayService.Write($"Umid: {(sensorReading.RelativeHumidity.Equals(RelativeHumidity.Zero, RelativeHumidity.Zero) ? Constants.DISPLAY_ERROR_STATE
                : sensorReading.RelativeHumidity.Percent.ToString("#.##") + "%")}", _cursor);

            _logger.LogInformation(Constants.DISPLAY_CLIMATIC_INFO_UPDATED);
        }
    }
}
