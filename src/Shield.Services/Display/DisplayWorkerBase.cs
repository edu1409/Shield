using Shield.Common;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Lcd;

namespace Shield.Services.Display
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

        public virtual ServiceStatus BacklightStatus
        {
            set
            {
                _displayService.BacklightOn = value == ServiceStatus.OnByService || value == ServiceStatus.OnByManual;
            }
        }

        public abstract void Execute();
        public abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual void Update(ServiceStatus newStatus) 
        {
            BacklightStatus = newStatus;
        }

        public void ControlBacklightSchedule(SharedMemoryByte lcd)
        {
            var backlightStatus = _sharedMemoryService.Read(lcd);
            var now = DateTimeOffset.Now;

            //Turns backlight on between 00:00 and 06:00 and between 18:00 and 00:00 if it is off by this service
            if ((now.Hour < 6 || now.Hour >= 18) && backlightStatus == ServiceStatus.OffByService)
            {
                BacklightStatus = ServiceStatus.OnByService;
                _logger.LogInformation($"{GetType().Name}: {Constants.LCD_BACKLIGHT_ON_SERVICE}");
            }//Turns backlight off between 06:00 and 18:00 if it is on by this service
            else if (now.Hour < 18 && now.Hour >= 6 && backlightStatus == ServiceStatus.OnByService)
            {
                BacklightStatus = ServiceStatus.OffByService;
                _logger.LogInformation($"{GetType().Name}: {Constants.LCD_BACKLIGHT_OFF_SERVICE}");
            }
            //Backlight turned on/off manually and wasn't changed.
            else if (backlightStatus == ServiceStatus.OnByManual || backlightStatus == ServiceStatus.OffByManual)
            {
                BacklightStatus = backlightStatus;
            }
        }

        protected void LCD16x2Banner(SharedMemoryByte sharedMemoryByte, CancellationToken stoppingToken = default)
        {
            string line1Text = Constants.LCD16x2_TITLE;
            string line2Text = Constants.LCD16x2_SUBTITLE;

            int retries = 0;

            try
            {
                //using (var mutex = Util.StartMutex())
                //{
                //    ControlBacklightSchedule(sharedMemoryByte);

                //    mutex.ReleaseMutex();
                //}

                for (int i = 1; i <= 12; i++)
                {
                    _displayService.Write(line1Text[^i..], new DisplayCursorPosition(0, 0));
                    _displayService.Write(line2Text[..i], new DisplayCursorPosition(16 - i, 1));

                    Task.Delay(150, stoppingToken).Wait(stoppingToken);
                }

                for (int i = 1; i <= 15; i++)
                {
                    _displayService.Write(line1Text.PadLeft(line1Text.Length + i), new DisplayCursorPosition(0, 0));

                    if (i <= 4) _displayService.Write(line2Text.PadRight(line1Text.Length + i), new DisplayCursorPosition(4 - i, 1));
                    else _displayService.Write(line2Text[(i - 4)..].PadRight(line2Text.Length), new DisplayCursorPosition(0, 1));

                    Task.Delay(150, stoppingToken).Wait(stoppingToken);
                }

                _displayService.Clear();

                Task.Delay(150, stoppingToken).Wait(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("End of service!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{GetType().Name}: {Constants.LCD_ERROR_STATE}");
                retries++;
                //Ends application after 3 consecutive exceptions
                if (retries > 3)
                {
                    FatalError(ex);
                    throw;
                }
            }
        }

        protected void FatalError(Exception ex)
        {
            _logger.LogCritical(ex, $"{GetType().Name}: {Constants.LCD_FATAL_ERROR}");

            _displayService.Clear();
            _cursor = new() { Left = 0, Top = 0 };
            _displayService.Write(Constants.LCD_FATAL_ERROR, _cursor);
        }
    }
}
