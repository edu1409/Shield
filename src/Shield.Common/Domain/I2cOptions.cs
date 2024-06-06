using Iot.Device.Bmxx80;

namespace Shield.Common.Domain
{
    public abstract class I2cOptions
    {
        public virtual int I2cBusId { get; set; } = 1;
        public virtual byte I2cAddress { get; set; } = Bmx280Base.DefaultI2cAddress;
    }
}
