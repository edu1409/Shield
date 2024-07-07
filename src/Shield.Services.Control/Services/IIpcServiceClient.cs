using Shield.Common.Domain;

namespace Shield.Services.Control.Services
{
    public interface IIpcServiceClient 
    {
        IpcMessage? SendMessage(SharedMemoryByte lcd, ServiceStatus status, bool reset = false);
        IpcMessage? SendMessage(SharedMemoryByte lcd, double dutyCycle);
    }
}
