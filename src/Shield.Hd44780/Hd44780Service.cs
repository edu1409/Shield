using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using Microsoft.Extensions.Options;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using System.Device.Gpio;
using System.Device.I2c;

namespace Shield.Hd44780
{
    public class Hd44780Service : IDisplayService
    {
        private readonly Lcd2004 _display;

        public bool BacklightOn
        {
            set { _display.BacklightOn = value; }
        }

        public Hd44780Service(IOptions<DisplayOptions> displayOptions)
        {
            _display = new(
                controller: new GpioController(
                    PinNumberingScheme.Logical,
                    new Pcf8574(I2cDevice.Create(new I2cConnectionSettings(
                        displayOptions.Value.I2cBusId, displayOptions.Value.I2cAddress)))),
                registerSelectPin: 0,
                enablePin: 2,
                dataPins: [4, 5, 6, 7],
                backlightPin: 3,
                backlightBrightness: 0.0F,
                readWritePin: 1);

            //Set '\' char
            _display.CreateCustomCharacter(0, [0b00000, 0b10000, 0b01000, 0b00100, 0b00010, 0b00001, 0b00000, 0b00000]);

            //Set 'º' char
            _display.CreateCustomCharacter(1, [0b00110, 0b01001, 0b01001, 0b00110, 0b00000, 0b00000, 0b00000, 0b00000]);
        }

        public void Write(string text, DisplayCursorPosition cursorPosition)
        {
            _display.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
            _display.Write(text);
        }
        public void Clear()
        {
            _display.Clear();
        }

        //public async Task SpinerAsync(DisplayCursorPosition position, int durationInMiliseconds)
        //{
        //    var currentChar = "-";

        //    var start = DateTimeOffset.Now;
        //    var end = DateTimeOffset.Now.AddMilliseconds(durationInMiliseconds);

        //    while (start <= end)
        //    {
        //        if (currentChar == "-") currentChar = "\0";
        //        else if (currentChar == "\0") currentChar = "|";
        //        else if (currentChar == "|") currentChar = "/";
        //        else if (currentChar == "/") currentChar = "-";

        //        Write(currentChar, position);
        //        await Task.Delay(250);
        //        start = DateTimeOffset.Now;
        //    }
        //}
    }
}
