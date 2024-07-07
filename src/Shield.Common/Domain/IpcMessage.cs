using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shield.Common.Domain
{
    [JsonDerivedType(typeof(IpcErrorMessage), typeDiscriminator: "IpcErrorMessage")]
    [JsonDerivedType(typeof(IpcFanMessage), typeDiscriminator: "IpcFanMessage")]
    public class IpcMessage
    {
        public SharedMemoryByte MemoryByte { get; set; }
        public ServiceStatus Status { get; set; }
        public bool ResetStatus { get; set; }
        public ApplicationException? Exception { get; set; }
        
        public string Serialize() => JsonSerializer.Serialize(this);
        public static IpcMessage? Deserialize(string json)
            => JsonSerializer.Deserialize<IpcMessage>(json);
    }
}
