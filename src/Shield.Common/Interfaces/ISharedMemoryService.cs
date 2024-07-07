using Shield.Common.Domain;

namespace Shield.Common.Interfaces
{
    public interface ISharedMemoryService
    {
        public void Write(SharedMemoryByte display, ServiceStatus value);
        ServiceStatus Read(SharedMemoryByte display);
    }
}
