using Shield.Common.Domain;

namespace Shield.Display.Backlight
{
    public interface IBacklightService
    {
        Task ControlDisplayBacklightAsync(DisplayBacklightStatus changedStatus);
        Task ResetControlAsync(CancellationToken cancellationToken = default);
    }
}
