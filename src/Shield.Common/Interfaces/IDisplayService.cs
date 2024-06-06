using Shield.Common.Domain;

namespace Shield.Common.Interfaces
{
    public interface IDisplayService
    {
        bool BacklightOn { set; }
        void Write(string text, DisplayCursorPosition cursorPosition);
        //Task SpinerAsync(DisplayCursorPosition position, int durationInMiliseconds);
        void Clear();
    }
}
