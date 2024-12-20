//using Shield.Bme280;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Common.Services;
using Shield.Fan;
using Shield.Lcd;
using Shield.Logger;
using Shield.Services.Display;
using Shield.Services.Fan;
using Shield.Services.Ipc;

namespace Shield.Services
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            //change to a one display one without climate sensor
            builder.Services.AddHostedService<Worker>()
                //.AddSingleton<IPrimaryDisplayWorker, PrimaryDisplayWorker>()
                //.AddSingleton<ISecondaryDisplayWorker, SecondaryDisplayWorker>()
                .AddSingleton<ISingleDisplayWorker, SingleDisplayWorker>()
                //.AddSingleton<IDisplayService<Lcd20x4>, Lcd20x4>()
                .AddSingleton<IDisplayService<Lcd16x2>, Lcd16x2>()
                //.AddSingleton<IClimateSensorService, Bme280Service>()
                .AddSingleton<IIntakeFanWorker, IntakeFanWorker>()
                .AddSingleton<IExhaustFanWorker, ExhaustFanWorker>()
                .AddSingleton<IFanService<IntakeFan>, IntakeFan>()
                .AddSingleton<IFanService<ExhaustFan>, ExhaustFan>()
                .AddSingleton<ISharedMemoryService, SharedMemoryService>()
                .AddSingleton<IIpcServiceServer, IpcServiceServer>()
                .Configure<DisplayOptions>(builder.Configuration.GetSection(nameof(DisplayOptions)))
                //.Configure<ClimateSensorOptions>(builder.Configuration.GetSection(nameof(ClimateSensorOptions)))
                .Configure<FanOptions>(o => o.PwmChipNumber = 2)
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