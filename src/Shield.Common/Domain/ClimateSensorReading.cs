namespace Shield.Common.Domain
{
    public class ClimateSensorReading
    {
        public UnitsNet.Temperature Temperature { get; set; } = UnitsNet.Temperature.Zero;
        public UnitsNet.RelativeHumidity RelativeHumidity { get; set; } = UnitsNet.RelativeHumidity.Zero;
    }
}
