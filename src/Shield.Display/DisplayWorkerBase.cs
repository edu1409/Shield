using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Lcd;

namespace Shield.Display
{
    public abstract class DisplayWorkerBase<T>(ILogger logger, 
        IDisplayService<T> displayService, 
        ISharedMemoryService sharedMemoryService)
        where T : LcdService<T>
    {
        protected readonly ILogger _logger = logger;
        protected readonly IDisplayService<T> _displayService = displayService;
        protected readonly ISharedMemoryService _sharedMemoryService = sharedMemoryService;

        protected DisplayCursorPosition _cursor = new() { Left = 0, Top = 0 };

        public virtual DisplayBacklightStatus BacklightStatus
        {
            set
            {
                _displayService.BacklightOn = (value == DisplayBacklightStatus.OnByService) || (value == DisplayBacklightStatus.OnByManual);
            }
        }

        public void ControlBacklightSchedule(Common.Domain.Lcd lcd)
        {
            var backlightStatus = _sharedMemoryService.Read(lcd);
            var now = DateTimeOffset.Now;

            //Turns backlight on between 00:00 and 06:00 and between 18:00 and 00:00 if it is off by this service
            if ((now.Hour < 6 || now.Hour >= 18) && (backlightStatus == DisplayBacklightStatus.OffByService))
            {
                BacklightStatus = DisplayBacklightStatus.OnByService;
                _logger.LogInformation($"{GetType().Name}: {Constants.BACKLIGHT_ON_SERVICE}");
            }//Turns backlight off between 06:00 and 18:00 if it is on by this service
            else if ((now.Hour < 18 && now.Hour >= 6) && (backlightStatus == DisplayBacklightStatus.OnByService))
            {
                BacklightStatus = DisplayBacklightStatus.OffByService;
                _logger.LogInformation($"{GetType().Name}: {Constants.BACKLIGHT_OFF_SERVICE}");
            }
            //Backlight turned on/off manually and wasn't changed.
            else if (backlightStatus == DisplayBacklightStatus.OnByManual || backlightStatus == DisplayBacklightStatus.OffByManual)
            {
                BacklightStatus = backlightStatus;
            }
        }

        protected void FatalError(Exception ex)
        {
            _logger.LogCritical(ex, $"{GetType().Name}: {Constants.DISPLAY_FATAL_ERROR}");

            _displayService.Clear();
            _cursor = new() { Left = 0, Top = 0 };
            _displayService.Write(Constants.DISPLAY_FATAL_ERROR, _cursor);
        }
    }
}
