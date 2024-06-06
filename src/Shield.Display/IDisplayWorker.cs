using Shield.Common.Domain;

namespace Shield.Display
{
    public interface IDisplayWorker
    {
        DisplayBacklightStatus BacklightStatus { set; }
        Task WelcomeAsync(CancellationToken cancellationToken = default);
        void ControlBacklightSchedule();
        void UpdateTime();
        Task UpdateClimateInformationAsync(CancellationToken cancellationToken = default);
        void FatalError(Exception exception);

    }
}
