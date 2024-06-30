using Iot.Device.CharacterLcd;
using System.Device.Gpio;

namespace Shield.Lcd
{
    /// <summary>
    /// Pin     Logical         Physical
    /// rs      7               26
    /// e       8               24
    /// data    25,24,23,18     22,18,16,12
    /// bl      27              13
    /// </summary>
    public class Lcd16x2 : LcdService<Lcd16x2>
    {
        public Lcd16x2() : base(
            new Lcd1602(7, 8, [25, 24, 23, 18], 27))
        { }
    }
}
