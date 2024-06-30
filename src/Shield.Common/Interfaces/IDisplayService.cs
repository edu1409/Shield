using Shield.Common.Domain;

namespace Shield.Common.Interfaces
{
    public interface IDisplayService<T> : IDisposable
    {
        bool BacklightOn { set; }
        bool DisplayOn { set; }
        void Write(string text, DisplayCursorPosition cursorPosition);
        //Task SpinerAsync(DisplayCursorPosition position, int durationInMiliseconds);
        void Clear();
    }
}
