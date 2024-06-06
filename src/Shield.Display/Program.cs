using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Common.Services;
using Shield.Hd44780;
using Shield.Logger;
using Shield.Bme280;

namespace Shield.Display
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<Worker>()
                .AddSingleton<IDisplayWorker, DisplayWorker>()
                .AddSingleton<IDisplayService, Hd44780Service>()
                .AddSingleton<IClimateSensorService, Bme280Service>()
                .AddSingleton<ISharedMemoryService, SharedMemoryService>()
                .Configure<DisplayOptions>(builder.Configuration.GetSection(nameof(DisplayOptions)))
                .Configure<ClimateSensorOptions>(builder.Configuration.GetSection(nameof(ClimateSensorOptions)))
                .Configure<SharedMemoryOptions>(options => options.Source = SharedMemorySource.Server);
            
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddFileLoggerProvider();
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            });

            var host = builder.Build();
            host.Run();
        }

        /// <summary>
        /// Gets or create <see cref="Mutex"/> to avoid concurrency between 
        /// <see cref="Shield.Display.Worker"/> and Shield.Display.Backlight.Program .
        /// </summary>
        public static Mutex StartMutex()
        {
            Mutex mutex;

            try
            {
                mutex = Mutex.OpenExisting(Constants.SERVICE_MUTEX);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                mutex = new Mutex(false, Constants.SERVICE_MUTEX);
            }

            mutex.WaitOne();

            return mutex;
        }
    }
}