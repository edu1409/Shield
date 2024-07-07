namespace Shield.Common.Domain
{
    public class IpcErrorMessage : IpcMessage
    {
        private IpcErrorMessage() { }

        public static IpcErrorMessage Create(ApplicationException ex) 
        {
            return new IpcErrorMessage { Exception = ex };
        }
    }
}
