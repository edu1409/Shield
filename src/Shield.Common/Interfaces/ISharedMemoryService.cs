using Shield.Common.Domain;

namespace Shield.Common.Interfaces
{
    public interface ISharedMemoryService
    {
        void Write(DisplayBacklightStatus value);
        DisplayBacklightStatus Read();
    }
}
