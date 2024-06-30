using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Shield.Logger
{
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;

        private FileLogger? _logger;

        public FileLoggerProvider()
        {
            _logDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "logs");
            
            if (!Directory.Exists(_logDirectory)) Directory.CreateDirectory(_logDirectory);
        }

        public ILogger CreateLogger(string categoryName)
        {
            _logger = new FileLogger(_logDirectory);
            return _logger;
        }

        public void Dispose()
        {
            _logger = null;
        }
    }
}
