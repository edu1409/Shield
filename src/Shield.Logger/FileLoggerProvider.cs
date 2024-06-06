using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Shield.Logger
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;

        public FileLoggerProvider()
        {
            _logDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "logs");
            
            if (!Directory.Exists(_logDirectory)) Directory.CreateDirectory(_logDirectory);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_logDirectory);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
