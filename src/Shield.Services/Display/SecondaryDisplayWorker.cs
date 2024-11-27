//using Shield.Common;
//using Shield.Common.Domain;
//using Shield.Common.Interfaces;
//using Shield.Lcd;

//namespace Shield.Services.Display
//{
//    public class SecondaryDisplayWorker(ILogger<SecondaryDisplayWorker> logger,
//        IDisplayService<Lcd16x2> displayService,
//        ISharedMemoryService sharedMemoryService)
//        : DisplayWorkerBase<Lcd16x2>(logger, displayService, sharedMemoryService), ISecondaryDisplayWorker
//    {
//        public override ServiceStatus BacklightStatus
//        {
//            set
//            {
//                base.BacklightStatus = value;
//                _sharedMemoryService.Write(SharedMemoryByte.SecondaryDisplayStatus, value);
//            }
//        }
//        public override void Execute()
//        {
//            ExecuteAsync().Wait();
//        }

//        public override async Task ExecuteAsync(CancellationToken stoppingToken = default)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                await base.LCD16x2Banner(SharedMemoryByte.SecondaryDisplayStatus);
//            }
//        }

//        public void ResetStatus()
//        {
//            BacklightStatus = ServiceStatus.OffByService;
//            ControlBacklightSchedule(SharedMemoryByte.SecondaryDisplayStatus);
//        }
//    }
//}
