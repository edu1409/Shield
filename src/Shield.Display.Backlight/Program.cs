using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Common.Services;
using Shield.Display.Backlight.Services;
using Shield.Logger;
using System.Text;

namespace Shield.Display.Backlight
{
    internal class Program
    {
        private static readonly ServiceProvider _serviceProvider = ConfigureServices();
        
        private enum Command
        {
            off,
            on,
            reset,
            help
        }
        
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("No parameters specified. Use 'shdbl help' to get help.");
            }
            else 
            {
                if (Enum.TryParse(args[0], out Command cmd))
                {
                    if (cmd == Command.help)
                    {
                        ShowHelp();
                    }
                    else
                    {
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Invalid parameters. Use 'shdbl help' to get help.");
                        }
                        else
                        {
                            var command = char.ToUpper(args[1][0]) + args[1][1..].ToLower();
                            
                            if (Enum.TryParse(command, out Lcd lcd))
                            {
                                Execute(cmd, lcd);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid display '{args[1]}'. Use 'shdbl help' to get help.");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid command '{args[0]}'. Use 'shdbl help' to get help.");
                }
            }
        }

        private static void Execute(Command cmd, Lcd lcd)
        {
            if (cmd == Command.help)
            {
                ShowHelp();
                return;
            }

            var client = _serviceProvider.GetService<IIpcServiceClient>();
            //var logger = _serviceProvider.GetService<ILogger<Program>>()!;

            var result = client!.SendMessage(lcd, DisplayBacklightStatus.None, false);

            var currentStatus = result!.Status;
            string message = string.Empty;

            if (cmd == Command.on || cmd == Command.off) message = ChangeBacklightStatus(cmd, lcd, currentStatus);
            else if (cmd == Command.reset) message = ResetBacklightStatus(lcd, currentStatus);

            //logger.LogInformation(message);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Check if user's command is changing the current backlight status or not
        /// </summary>
        private static string ChangeBacklightStatus(Command cmd, Lcd lcd, DisplayBacklightStatus currentStatus)
        {
            string? resultMessage = $"{lcd} {Constants.BACKLIGHT_MANUAL_NOCHANGE}";

            var newStatus = cmd == Command.on ? DisplayBacklightStatus.OnByManual : DisplayBacklightStatus.OffByManual;

            //Execute only if not change from ON (service or manual) to ON (manual) or from OFF (service or manual) to OFF (manual)
            if (!(((currentStatus == DisplayBacklightStatus.OnByService || currentStatus == DisplayBacklightStatus.OnByManual)
                && newStatus == DisplayBacklightStatus.OnByManual) ||
                ((currentStatus == DisplayBacklightStatus.OffByService || currentStatus == DisplayBacklightStatus.OffByManual)
                && newStatus == DisplayBacklightStatus.OffByManual)))
            {
                var client = _serviceProvider.GetService<IIpcServiceClient>();
                client!.SendMessage(lcd, newStatus);

                resultMessage = string.Format(Constants.BACKLIGHT_MANUAL_CHANGE, lcd.ToString(), newStatus == DisplayBacklightStatus.OnByManual ? "ON" : "OFF");
            }

            return resultMessage;
        }

        /// <summary>
        /// Returns display backlight control to automatic.
        /// </summary>
        private static string ResetBacklightStatus(Lcd lcd, DisplayBacklightStatus currentStatus)
        {
            var resultMessage = $"{lcd} {Constants.BACKLIGHT_MANUAL_NOCHANGE}";

            //reset only executes if current control is manual
            if (currentStatus != DisplayBacklightStatus.OnByService
                && currentStatus != DisplayBacklightStatus.OffByService)
            {
                var client = _serviceProvider.GetService<IIpcServiceClient>();
                client!.SendMessage(lcd, currentStatus, true);

                resultMessage = $"{lcd} {Constants.BACKLIGHT_BACK_AUTOMATIC}";
            }

            return resultMessage;
        }

        private static void ShowHelp()
        {
            var message = new StringBuilder();

            message.Append($"\n\rUsage: shdb [command] [display]");
            message.Append("\n\n\rTurns display backlight ON or OFF.");
            message.Append("\n\n\r[command]");
            message.Append("\n\r\ton\t\tSet display backlight ON.");
            message.Append("\n\r\toff\t\tSet display backlight OFF.");
            message.Append("\n\r\treset\t\tReturns display backlight control to automatic.");
            message.Append("\n\n\r[display]");
            message.Append("\n\r\tprimary\t\tFor primary display.");
            message.Append("\n\r\tsecondary\tFor secondary display.\n\r");

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
                .Configure<SharedMemoryOptions>(options => options.Source = SharedMemorySource.Command)
                .AddSingleton<ISharedMemoryService, SharedMemoryService>()
                .AddSingleton<IIpcServiceClient, IpcServiceClient>();
 
                //services.AddLogging(loggingBuilder =>
                //{
                //    loggingBuilder.ClearProviders();
                //    loggingBuilder.AddFileLoggerProvider();
                //    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                //});

            return services.BuildServiceProvider();
        }
    }
}