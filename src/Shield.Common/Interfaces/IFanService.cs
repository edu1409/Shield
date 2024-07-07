namespace Shield.Common.Interfaces
{
    public interface IFanService<T> : IDisposable
    {
        public bool On { set; }
        double DutyCycle { get; set; }
        void Start();
        void Stop();
    }
}
