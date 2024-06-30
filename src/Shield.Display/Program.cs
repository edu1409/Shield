using Shield.Bme280;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Common.Services;
using Shield.Display.Services;
using Shield.Lcd;
using Shield.Logger;

namespace Shield.Display
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<Worker>()
                .AddSingleton<IPrimaryDisplayWorker, PrimaryDisplayWorker>()
                .AddSingleton<ISecondaryDisplayWorker, SecondaryDisplayWorker>()
                .AddSingleton<IDisplayService<Lcd20x4>, Lcd20x4>()
                .AddSingleton<IDisplayService<Lcd16x2>, Lcd16x2>()
                .AddSingleton<IClimateSensorService, Bme280Service>()
                .AddSingleton<ISharedMemoryService, SharedMemoryService>()
                .AddSingleton<IIpcServiceServer, IpcServiceServer>()
                .Configure<DisplayOptions>(builder.Configuration.GetSection(nameof(DisplayOptions)))
                .Configure<ClimateSensorOptions>(builder.Configuration.GetSection(nameof(ClimateSensorOptions)))
                .Configure<SharedMemoryOptions>(options => options.Source = SharedMemorySource.Startup);
            
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddFileLoggerProvider();
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            });

            var host = builder.Build();
            host.Run();
        }
    }
}