using Sensor = Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using System.Device.I2c;

namespace Shield.Bme280
{
    public class Bme280Service : IClimateSensorService
    {
        private readonly ILogger<Bme280Service> _logger;
        private readonly Sensor.Bme280 _sensor;

        public Bme280Service(ILogger<Bme280Service> logger, IOptions<ClimateSensorOptions> sensorOptions)
        {
            _logger = logger;
            _sensor = new Sensor.Bme280(I2cDevice.Create(
                new I2cConnectionSettings(sensorOptions.Value.I2cBusId, sensorOptions.Value.I2cAddress)));
            _sensor.SetPowerMode(Bmx280PowerMode.Forced);
        }

        public async Task<ClimateSensorReading> ReadAsync(CancellationToken cancellationToken = default)
        {
            ClimateSensorReading sensorReading;

            try
            {
                await Task.Delay(_sensor.GetMeasurementDuration(), cancellationToken);

                sensorReading = new ClimateSensorReading
                {
                    Temperature = _sensor.TryReadTemperature(out var temp) ? temp : UnitsNet.Temperature.Zero,
                    RelativeHumidity = _sensor.TryReadHumidity(out var hum) ? hum : UnitsNet.RelativeHumidity.Zero
                };

                _logger.LogInformation(string.Format(Constants.CLIMATE_SENSOR_READING, sensorReading.Temperature.DegreesCelsius, sensorReading.RelativeHumidity));
            }
            catch (Exception ex)
            {
                sensorReading = new();
                _logger.LogError(ex, Constants.CLIMATE_SENSOR_READING_ERROR);
            }

            return sensorReading;
        }
    }
}
