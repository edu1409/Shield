using Microsoft.Extensions.Logging;

namespace Shield.Logger
{
    public class FileLogger : ILogger
    {
        private string _currentFilePath;
        private readonly string _logDirectory;
        private readonly object _lock = new();
        private DateTime _currentDate;

        public FileLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
            _currentDate = DateTime.Now.Date;
            _currentFilePath = GetLogFilePath(_currentDate);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            // Use the formatter to create the log message
            var message = formatter(state, exception);

            // Include exception details if present
            if (exception != null) message += Environment.NewLine + exception.ToString();

            var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}";

            lock (_lock)
            {
                CheckForNewDay();
                File.AppendAllText(_currentFilePath, logRecord + Environment.NewLine);
            }
        }

        private void CheckForNewDay()
        {
            var currentDate = DateTime.Now.Date;
            if (_currentDate != currentDate)
            {
                _currentDate = currentDate;
                _currentFilePath = GetLogFilePath(_currentDate);
            }
        }

        private string GetLogFilePath(DateTime date)
        {
            return Path.Combine(_logDirectory, $"{date:yyyyMMdd}.log");
        }
    }

}
