using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Fan;

namespace Shield.Services.Fan
{
    public class ExhaustFanWorker(ILogger<ExhaustFanWorker> logger,
        IFanService<ExhaustFan> fanService,
        ISharedMemoryService sharedMemoryService)
        : FanWorker<ExhaustFan>(logger, fanService, sharedMemoryService), IExhaustFanWorker
    {
        public override ServiceStatus FanStatus
        {
            set
            {
                base.FanStatus = value;
                _sharedMemoryService.Write(SharedMemoryByte.ExhaustFanStatus, value);
            }
        }

        public override ServiceStatus GetStatus()
        {
            return _sharedMemoryService.Read(SharedMemoryByte.ExhaustFanStatus);
        }
    }
}
