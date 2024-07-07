using System.Text.Json;

namespace Shield.Common.Domain
{
    public class IpcFanMessage : IpcMessage
    {
        public double DutyCycle { get; set; }
    }
}
