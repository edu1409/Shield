using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Common.Services;
using Shield.Hd44780;
using Shield.Logger;
using Shield.Utils.BME280;
using System.Text;

namespace Shield.Display.Backlight
{
    internal class Program
    {
        private static readonly ServiceProvider _serviceProvider = ConfigureServices();

        private static IBacklightService? _backlightService;
        
        private enum Command
        {
            off,
            on,
            reset,
            help
        }
        
        static void Main(string[] args)
        {
            if (args.Length == 0) Console.WriteLine("No command specified. Use 'shdbl help' to get help.");
            else if (Enum.TryParse(args[0], out Command cmd)) Execute(cmd);
            else Console.WriteLine($"Invalid command '{args[0]}'. Use 'shdbl help' to get help.");
        }

        private static void Execute(Command cmd)
        {
            if (cmd == Command.help)
            {
                ShowHelp();
                return;
            }

            using var mutex = Display.Program.StartMutex();

            ISharedMemoryService sharedMemory = _serviceProvider.GetService<ISharedMemoryService>()!;
            ILogger<Program> logger = _serviceProvider.GetService<ILogger<Program>>()!;

            var currentStatus = sharedMemory!.Read();
            string message = string.Empty;

            if (cmd == Command.on || cmd == Command.off) message = ChangeBacklightStatus(cmd, currentStatus);
            else if (cmd == Command.reset) message = ResetBacklightStatus(currentStatus);

            logger.LogInformation(message);
            Console.WriteLine(message);

            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Check if user's command is changing the current backlight status or not
        /// </summary>
        private static string ChangeBacklightStatus(Command cmd, DisplayBacklightStatus currentStatus)
        {
            string? message = Constants.BACKLIGHT_MANUAL_NOCHANGE;

            var newStatus = cmd == Command.on ? DisplayBacklightStatus.OnByManual : DisplayBacklightStatus.OffByManual;

            //Execute only if not change from ON (service or manual) to ON (manual) or from OFF (service or manual) to OFF (manual)
            if (!(((currentStatus == DisplayBacklightStatus.OnByService || currentStatus == DisplayBacklightStatus.OnByManual)
                && newStatus == DisplayBacklightStatus.OnByManual) ||
                ((currentStatus == DisplayBacklightStatus.OffByService || currentStatus == DisplayBacklightStatus.OffByManual)
                && newStatus == DisplayBacklightStatus.OffByManual)))
            {
                _backlightService = _serviceProvider.GetService<IBacklightService>()!;
                _backlightService.ControlDisplayBacklightAsync(newStatus).Wait();

                message = string.Format(Constants.BACKLIGHT_MANUAL_CHANGE, newStatus == DisplayBacklightStatus.OnByManual ? "ON" : "OFF");
            }

            return message;
        }

        /// <summary>
        /// Returns display backlight control to automatic.
        /// </summary>
        private static string ResetBacklightStatus(DisplayBacklightStatus currentStatus)
        {
            var message = Constants.BACKLIGHT_MANUAL_NOCHANGE;

            //reset only executes if current control is manual
            if (currentStatus != DisplayBacklightStatus.OnByService
                && currentStatus != DisplayBacklightStatus.OffByService)
            {
                _backlightService = _serviceProvider.GetService<IBacklightService>()!;
                _backlightService.ResetControlAsync().Wait();

                message = Constants.BACKLIGHT_BACK_AUTOMATIC;
            }

            return message;
        }

        private static void ShowHelp()
        {
            var message = new StringBuilder();

            message.Append($"\n\rUsage: shieldbacklight [command]");
            message.Append("\n\n\rTurns display backlight ON or OFF.");
            message.Append("\n\n\rCommands:");
            message.Append("\n\r\ton\tSet display backlight ON.");
            message.Append("\n\r\toff\tSet display backlight OFF.");
            message.Append("\n\r\treset\tReturns display backlight control to automatic.");
            message.Append("\n\r\thelp\tShow command line help.\n\r");

            Console.WriteLine(message.ToString());
        }

        /// <summary>
        /// Configure dependency injection
        /// </summary>
        private static ServiceProvider ConfigureServices()
        {
            //using appsettings.json defined in Shield.Display
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();

            var services = new ServiceCollection();
            services.AddOptions()
                .AddSingleton<IBacklightService, BacklightService>()
                .AddSingleton<IDisplayWorker, DisplayWorker>()
                .AddSingleton<IDisplayService, Hd44780Service>()
                .AddSingleton<IClimateSensorService, Bme280Service>()
                .AddSingleton<ISharedMemoryService, SharedMemoryService>()
                .Configure<DisplayOptions>(config.GetSection(nameof(DisplayOptions)))
                .Configure<ClimateSensorOptions>(config.GetSection(nameof(ClimateSensorOptions)))
                .Configure<SharedMemoryOptions>(options => options.Source = SharedMemorySource.Client);

                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddFileLoggerProvider();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                });

            return services.BuildServiceProvider();
        }
    }
}