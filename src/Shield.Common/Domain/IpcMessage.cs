using System.Text.Json;

namespace Shield.Common.Domain
{
    public class IpcMessage
    {
        public Lcd Lcd { get; set; }
        public DisplayBacklightStatus Status { get; set; }
        public bool ResetStatus { get; set; }
        public ApplicationException? Exception { get; set; }
        public string Serialize() => JsonSerializer.Serialize(this);
        public static IpcMessage? Deserialize(string json)
            => JsonSerializer.Deserialize<IpcMessage>(json);
    }
}
