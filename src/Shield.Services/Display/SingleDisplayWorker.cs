using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Lcd;

namespace Shield.Services.Display
{
    public class SingleDisplayWorker(ILogger<SingleDisplayWorker> logger,
        IDisplayService<Lcd16x2> displayService,
        ISharedMemoryService sharedMemoryService)
        : DisplayWorkerBase<Lcd16x2>(logger, displayService, sharedMemoryService), ISingleDisplayWorker
    {
        public override ServiceStatus BacklightStatus
        {
            set
            {
                base.BacklightStatus = value;
                _sharedMemoryService.Write(SharedMemoryByte.SingleDisplayStatus, value);
            }
        }
        public override void Execute()
        {
            ExecuteAsync().Wait();
        }


        public async override Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            BacklightStatus = ServiceStatus.OnByService;

            while (!stoppingToken.IsCancellationRequested)
            {
                base.LCD16x2Banner(SharedMemoryByte.SingleDisplayStatus, stoppingToken);

                _displayService.Write(Constants.LCD16x2_TITLE, new DisplayCursorPosition(2, 0));
                _displayService.Write(DateTime.Now.ToString("dd/MM/yyyy HH:mm"), new DisplayCursorPosition(0, 1));

                await Task.Delay(15000, stoppingToken);

                _displayService.Clear();
            }
        }

        public void ResetStatus()
        {
            BacklightStatus = ServiceStatus.OnByService;
            //ControlBacklightSchedule(SharedMemoryByte.SingleDisplayStatus);
        }
    }
}
