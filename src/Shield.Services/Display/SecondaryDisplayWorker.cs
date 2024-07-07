using Shield.Common;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Lcd;

namespace Shield.Services.Display
{
    public class SecondaryDisplayWorker(ILogger<SecondaryDisplayWorker> logger,
        IDisplayService<Lcd16x2> displayService,
        ISharedMemoryService sharedMemoryService)
        : DisplayWorker<Lcd16x2>(logger, displayService, sharedMemoryService), ISecondaryDisplayWorker
    {
        public override ServiceStatus BacklightStatus
        {
            set
            {
                base.BacklightStatus = value;
                _sharedMemoryService.Write(SharedMemoryByte.SecondaryDisplayStatus, value);
            }
        }
        public override void Execute()
        {
            ExecuteAsync().Wait();
        }

        public override async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
            string line1Text = "-- SHIELD --";
            string line2Text = "Media Server";

            int retries = 0;

            await Task.Run(() => 
            { 
                BacklightStatus = ServiceStatus.OffByService; 
            }, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var mutex = Util.StartMutex())
                    {
                        ControlBacklightSchedule(SharedMemoryByte.SecondaryDisplayStatus);

                        mutex.ReleaseMutex();
                    }

                    for (int i = 1; i <= 12; i++)
                    {
                        _displayService.Write(line1Text[^i..], new DisplayCursorPosition(0, 0));
                        _displayService.Write(line2Text[..i], new DisplayCursorPosition(16 - i, 1));

                        Task.Delay(300).Wait();
                    }

                    for (int i = 1; i <= 15; i++)
                    {
                        _displayService.Write(line1Text.PadLeft(line1Text.Length + i), new DisplayCursorPosition(0, 0));

                        if (i <= 4) _displayService.Write(line2Text.PadRight(line1Text.Length + i), new DisplayCursorPosition(4 - i, 1));
                        else _displayService.Write(line2Text[(i - 4)..].PadRight(line2Text.Length), new DisplayCursorPosition(0, 1));

                        Task.Delay(300).Wait();
                    }

                    _displayService.Write(" ", new DisplayCursorPosition(15, 0));
                    _displayService.Write(" ", new DisplayCursorPosition(0, 1));

                    Task.Delay(300).Wait();
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
                    if (retries > 3)
                    {
                        FatalError(ex);
                        throw;
                    }
                }
            }
        }

        public void ResetStatus()
        {
            BacklightStatus = ServiceStatus.OffByService;
            ControlBacklightSchedule(SharedMemoryByte.SecondaryDisplayStatus);
        }
    }
}
