using Shield.Common.Domain;

namespace Shield.Display.Backlight.Services
{
    public interface IIpcServiceClient 
    {
        IpcMessage? SendMessage(Lcd lcd, DisplayBacklightStatus status, bool reset = false);
    }
}
