using Shield.Common.Domain;

namespace Shield.Display
{
    public interface IDisplayWorker
    {
        DisplayBacklightStatus BacklightStatus { set; }
        void Execute();
        void ControlBacklightSchedule(Common.Domain.Lcd lcd);
    }
}
