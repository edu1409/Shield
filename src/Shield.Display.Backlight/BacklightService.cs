using Shield.Common.Domain;

namespace Shield.Display.Backlight
{
    public class BacklightService(IDisplayWorker displayWorker) : IBacklightService
    {
        private readonly IDisplayWorker _displayWorker = displayWorker;

        public async Task ControlDisplayBacklightAsync(DisplayBacklightStatus changedBacklightStatus)
        {
            _displayWorker.UpdateTime();
            await _displayWorker.UpdateClimateInformationAsync();
            _displayWorker.BacklightStatus = changedBacklightStatus;
        }

        /// <summary>
        /// Returns the display backlight to automatic control.
        /// </summary>
        public async Task ResetControlAsync(CancellationToken cancellationToken = default)
        {
            await _displayWorker.WelcomeAsync(cancellationToken);
            _displayWorker.UpdateTime();
            await _displayWorker.UpdateClimateInformationAsync(cancellationToken);
            _displayWorker.ControlBacklightSchedule();
        }
    }
}
