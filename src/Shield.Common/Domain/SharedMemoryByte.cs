namespace Shield.Common.Domain
{
    public enum SharedMemoryByte
    {
        PrimaryDisplayStatus = 1,
        SecondaryDisplayStatus = 2,
        IntakeFanStatus = 3,
        ExhaustFanStatus = 4,
        IntakeFanDutyCycle = 5, //8 bytes
        ExhaustFanDutyCycle = 13, //8 bytes
        SingleDisplayStatus = 14
    }
}
