namespace Shield.Common.Domain
{
    public class DisplayOptions : I2cOptions
    {
        public override byte I2cAddress { get; set; } = 0x27;
    }
}
