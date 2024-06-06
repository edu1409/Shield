using Shield.Common.Domain;
using Shield.Common.Interfaces;
using UnitsNet;

namespace Shield.Display
{
    public class DisplayWorker(ILogger<DisplayWorker> logger,
        IDisplayService displayService, 
        IClimateSensorService climateSensorService,
        ISharedMemoryService sharedMemoryService) : IDisplayWorker
    {
        private readonly ILogger<DisplayWorker> _logger = logger;
        private readonly IDisplayService _displayService = displayService;
        private readonly IClimateSensorService _climateSensorService = climateSensorService;
        private readonly ISharedMemoryService _sharedMemoryService = sharedMemoryService;

        private DisplayCursorPosition _cursor = new() { Left = 0, Top = 0 };

        public DisplayBacklightStatus BacklightStatus 
        {
            set 
            {
                _displayService.BacklightOn = (value == DisplayBacklightStatus.OnByService) || (value == DisplayBacklightStatus.OnByManual);
                _sharedMemoryService.Write(value);
            }
        }

        public async Task WelcomeAsync(CancellationToken cancellationToken = default)
        {
            var message = Constants.DISPLAY_WELCOME;

            BacklightStatus = DisplayBacklightStatus.OnByService;
            _cursor = new() { Left = 0, Top = 0 };

            for (int i = 0; i < message.Length; i++)
            {
                _cursor.Left = i;
                _displayService.Write(message.ElementAt(i).ToString(), _cursor);
                await Task.Delay(100, cancellationToken);
            }

            await Task.Delay(1000, cancellationToken);

            _displayService.Clear();
            await Task.Delay(500, cancellationToken);

            _cursor.Left = 0;
            _displayService.Write(message, _cursor);
            await Task.Delay(2000, cancellationToken);

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

        public async Task UpdateClimateInformationAsync(CancellationToken cancellationToken = default)
        {
            //get values from sensor
            var sensorReading = await _climateSensorService.ReadAsync(cancellationToken);

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

        public void ControlBacklightSchedule()
        {
            var backlightStatus = _sharedMemoryService.Read();
            var now = DateTimeOffset.Now;

            //Turns backlight on between 00:00 and 06:00 and between 18:00 and 00:00 if it is off by this service
            if ((now.Hour < 6 || now.Hour >= 18) && (backlightStatus == DisplayBacklightStatus.OffByService))
            {
                BacklightStatus = DisplayBacklightStatus.OnByService;
                _logger.LogInformation(Constants.BACKLIGHT_ON_SERVICE);
            }//Turns backlight off between 06:00 and 18:00 if it is on by this service
            else if ((now.Hour < 18 && now.Hour >= 6) && (backlightStatus == DisplayBacklightStatus.OnByService))
            {
                BacklightStatus = DisplayBacklightStatus.OffByService;
                _logger.LogInformation(Constants.BACKLIGHT_OFF_SERVICE);
            }
            //Backlight turned on/off manually and wasn't changed.
            else if (backlightStatus == DisplayBacklightStatus.OnByManual || backlightStatus == DisplayBacklightStatus.OffByManual)
            {
                BacklightStatus = backlightStatus;
            }
        }

        public void FatalError(Exception ex)
        {
            _logger.LogCritical(ex, Constants.DISPLAY_FATAL_ERROR);

            _displayService.Clear();
            _cursor = new() { Left = 0, Top = 0 };
            _displayService.Write(Constants.DISPLAY_FATAL_ERROR, _cursor);
        }
    }
}
