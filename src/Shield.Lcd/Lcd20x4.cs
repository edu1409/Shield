using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using Microsoft.Extensions.Options;
using Shield.Common.Domain;
using System.Device.Gpio;
using System.Device.I2c;

namespace Shield.Lcd
{
    public class Lcd20x4 : LcdService<Lcd20x4>
    {
        public Lcd20x4(IOptions<DisplayOptions> displayOptions) : base(
            new Lcd2004(0, 2, [4, 5, 6, 7], 3, 0.0F, 1, new GpioController(
                PinNumberingScheme.Logical, new Pcf8574(I2cDevice.Create(new 
                    I2cConnectionSettings(displayOptions.Value.I2cBusId, displayOptions.Value.I2cAddress))))))
        {
            //Set '\' char
            _display.CreateCustomCharacter(0, [0b00000, 0b10000, 0b01000, 0b00100, 0b00010, 0b00001, 0b00000, 0b00000]);

            //Set 'º' char
            _display.CreateCustomCharacter(1, [0b00110, 0b01001, 0b01001, 0b00110, 0b00000, 0b00000, 0b00000, 0b00000]);
        }
    }
}
