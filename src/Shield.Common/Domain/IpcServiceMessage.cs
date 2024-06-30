using System.Text.Json;

namespace Shield.Common.Domain
{
    public class IpcServiceMessage(Lcd lcd, DisplayBacklightStatus status, bool resetStatus)
    {
        public Lcd Lcd { get; private set; } = lcd;
        public DisplayBacklightStatus Status { get; private set; } = status;
        public bool ResetStatus { get; private set; } = resetStatus;

        public string Serialize() => JsonSerializer.Serialize(this);

        public static IpcServiceMessage? Deserialize(string json) 
            => JsonSerializer.Deserialize<IpcServiceMessage>(json);
    }
}
