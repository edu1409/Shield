using Shield.Common.Domain;
using Shield.Common.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Device.Pwm;

namespace Shield.Fan
{
    public abstract class FanService<T>(int pwmChipNumber, FanPwmChannel pwmChannel) : IFanService<T>
    {
        private readonly PwmChannel _pwmChannel = PwmChannel.Create(pwmChipNumber, (int)pwmChannel);

        public bool On
        {
            set
            {
                if (value) Start();
                else Stop();
            }
        }

        [Range(0, 1, ErrorMessage = "Invalid duty cycle value. The value must be between 0 and 1.")]
        public double DutyCycle
        {
            get { return _pwmChannel.DutyCycle; }
            set { _pwmChannel.DutyCycle = value; }
        }

        public void Start()
        {
            _pwmChannel.Start();
            _pwmChannel.DutyCycle = 0;
        }

        public void Stop()
        {
            _pwmChannel.DutyCycle = 0;
            _pwmChannel.Stop();
        }

        public void Dispose()
        {
            _pwmChannel?.Stop();
            _pwmChannel?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
