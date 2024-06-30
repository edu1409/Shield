namespace Shield.Common.Domain
{
    public class DisplayCursorPosition
    {
        public DisplayCursorPosition() { }
        public DisplayCursorPosition(int left, int top)
        {
            Left = left;
            Top = top;
        }
    
        public int Left { get; set; }
        public int Top { get; set; }
    }
}
