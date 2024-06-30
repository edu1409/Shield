using Iot.Device.CharacterLcd;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using System.Device.Gpio;

namespace Shield.Lcd
{
    public abstract class LcdService<T> : IDisplayService<T>
    {
        protected readonly Hd44780 _display;
        
        protected LcdService(Hd44780 display)
        {
            _display = display;
            _display.BlinkingCursorVisible = false;
        }

        public virtual bool BacklightOn
        {
            set => _display.BacklightOn = value;
        }

        public virtual bool DisplayOn
        {
            set => _display.DisplayOn = value;
        }

        public virtual void Write(string text, DisplayCursorPosition cursorPosition)
        {
            _display.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
            _display.Write(text);
        }

        public virtual void Clear()
        {
            _display?.Clear();
        }
        public virtual void Dispose()
        {
            try
            {
                BacklightOn = false;
                DisplayOn = false;
            }
            finally
            {
                _display?.Dispose();
                GC.SuppressFinalize(this);
            }
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
