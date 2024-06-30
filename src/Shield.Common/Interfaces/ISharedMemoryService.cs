using Shield.Common.Domain;

namespace Shield.Common.Interfaces
{
    public interface ISharedMemoryService
    {
        public void Write(DisplayBacklightStatus value, Lcd display);
        DisplayBacklightStatus Read(Lcd display);
    }
}
