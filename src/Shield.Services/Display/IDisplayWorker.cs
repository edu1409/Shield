using Shield.Common.Domain;
using Shield.Common.Interfaces;

namespace Shield.Services.Display
{
    public interface IDisplayWorker : IWorkerService
    {
        ServiceStatus BacklightStatus { set; }
        void ControlBacklightSchedule(SharedMemoryByte status);
    }
}
