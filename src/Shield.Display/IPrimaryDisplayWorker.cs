namespace Shield.Display
{
    public interface IPrimaryDisplayWorker : IDisplayWorker
    {
        void Welcome();
        void UpdateTime();
        void UpdateClimateInformation();
    }
}
