using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Fan;

namespace Shield.Services.Fan
{
    public class IntakeFanWorker(ILogger<IntakeFanWorker> logger,
        IFanService<IntakeFan> fanService,
        ISharedMemoryService sharedMemoryService)
        : FanWorker<IntakeFan>(logger, fanService, sharedMemoryService), IIntakeFanWorker
    {
        public override ServiceStatus FanStatus 
        {
            set
            {
                base.FanStatus = value;
                _sharedMemoryService.Write(SharedMemoryByte.IntakeFanStatus, value);
            }
        }

        public override ServiceStatus GetStatus()
        {
            return _sharedMemoryService.Read(SharedMemoryByte.IntakeFanStatus);
        }
    }
}
