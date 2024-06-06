using Microsoft.Extensions.Logging;

namespace Shield.Logger
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLoggerProvider(this ILoggingBuilder builder)
        {
            builder.AddProvider(new FileLoggerProvider());
            return builder;
        }
    }
}
