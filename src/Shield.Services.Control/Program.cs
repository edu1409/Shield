using Iot.Device.Mpr121;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Common.Services;
using Shield.Services.Control.Services;
using System.Text;

namespace Shield.Services.Control
{
    internal class Program
    {
        private static readonly ServiceProvider _serviceProvider = ConfigureServices();
        
        private enum Resource
        {
            none = 0,
            fan,
            help,
            lcd
        }

        private enum Fan
        {
            intake = 0,
            exhaust = 1
        }

        private enum Lcd
        {
            primary,
            secondary
        }

        private enum State
        {
            on,
            off,
            reset
        }

        static void Main(string[] args)
        {
            var resultMessage = "No parameters specified. Use 'shield help' to get help.";

            if (args.Length > 0)
            {
                resultMessage = "Invalid parameters. Use 'shield help' to get help.";

                if (!Enum.TryParse(args[0], out Resource resource)) resultMessage = $"Invalid resource '{args[0]}'. Use 'shield help' to get help.";
                else
                {
                    if (resource == Resource.help) resultMessage = Help(); // Help
                    else if (resource == Resource.fan) //Fan
                    {
                        if (args.Length >= 3)
                        {
                            if (Enum.TryParse(args[1], out Fan fan)) // intake | exhaust
                            {
                                var fanName = char.ToUpper(fan.ToString()[0]) + fan.ToString()[1..];

                                if (Enum.TryParse(args[2], out State state)) // on | off | reset
                                {
                                    if (state == State.reset)
                                    {
                                        if (ResetFanStatus(fan)) resultMessage = string.Format(Constants.FAN_BACK_AUTOMATIC, fanName);
                                        else resultMessage = string.Format(Constants.FAN_MANUAL_NOCHANGE, fanName);
                                    }
                                    else
                                    {
                                        if (ChangeFanStatus(fan, state)) resultMessage = string.Format(Constants.FAN_MANUAL_CHANGE, fanName, state.ToString());
                                        else resultMessage = string.Format(Constants.FAN_MANUAL_NOCHANGE, fanName);
                                    }
                                }
                                else if (args[2].Equals("dutycycle", StringComparison.Ordinal))
                                {
                                    if (args.Length == 4 && double.TryParse(args[3], out double dutyCycle) && dutyCycle >=0 && dutyCycle <=1)
                                    {
                                        ChangeFanDutyCycle(fan, dutyCycle);
                                        resultMessage = string.Format(Constants.FAN_DUTY_CYCLE_CHANGE, fanName, dutyCycle);
                                    }
                                }
                            }
                        }
                    }
                    else if (resource == Resource.lcd) //Display
                    {
                        if (args.Length == 4)
                        {
                            if (Enum.TryParse(args[1], out Lcd lcd)) // primary | secondary
                            {
                                if (args[2].Equals("backlight", StringComparison.Ordinal)) // backlight
                                {
                                    if (Enum.TryParse(args[3], out State state)) // on | off | reset
                                    {
                                        var displayName = char.ToUpper(lcd.ToString()[0]) + lcd.ToString()[1..];

                                        if (state == State.reset)
                                        {
                                            if (ResetLcdBacklightStatus(lcd)) resultMessage = string.Format(Constants.BACKLIGHT_BACK_AUTOMATIC, displayName);
                                            else resultMessage = string.Format(Constants.BACKLIGHT_MANUAL_NOCHANGE, displayName);
                                        }
                                        else
                                        {
                                            if (ChangeLcdBacklightStatus(lcd, state)) resultMessage = string.Format(Constants.BACKLIGHT_MANUAL_CHANGE, displayName, state.ToString());
                                            else resultMessage = string.Format(Constants.BACKLIGHT_MANUAL_NOCHANGE, displayName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"{resultMessage}\n");
        }

        private static string Help()
        {
            var message = new StringBuilder();

            message.Append("\n\rAvailable commands:");
            message.Append("\n\r\tshield fan intake on|off|reset                    Turn intake fan on, off or return to automatic control.");
            message.Append("\n\r\tshield fan intake dutycycle 0..1                  Set intake fan duty cycle to a value between 0 and 1.");
            message.Append("\n\r\tshield fan exhaust on|off|reset                   Turn exaust fan on, off or return to automatic control.");
            message.Append("\n\r\tshield fan exhaust dutycycle 0..1                 Set exaust fan duty cycle to a value between 0 and 1.");
            message.Append("\n\r\tshield lcd primary backlight on|off|reset         Turn primary display backlight on, off or return to automatic control.");
            message.Append("\n\r\tshield lcd secondary backlight on|off|reset       Turn secondary display backlight on, off or return to automatic control.");
            message.Append("\n\r\tshield help                                       Show the available commands.");

            return message.ToString();
        }

        /// <summary>
        /// Check if user's command is changing the current backlight status or not
        /// </summary>
        private static bool ChangeLcdBacklightStatus(Lcd lcd, State state)
        {
            var memoryStatusByte = lcd == Lcd.primary ? SharedMemoryByte.PrimaryDisplayStatus : SharedMemoryByte.SecondaryDisplayStatus;

            return ChangeStatus(memoryStatusByte, state);
        }

        /// <summary>
        /// Returns display backlight control to automatic.
        /// </summary>
        private static bool ResetLcdBacklightStatus(Lcd lcd)
        {
            var memoryStatusByte = lcd == Lcd.primary ? SharedMemoryByte.PrimaryDisplayStatus : SharedMemoryByte.SecondaryDisplayStatus;

            return ResetStatus(memoryStatusByte);
        }

        /// <summary>
        /// Check if user's command is changing the current fan status or not
        /// </summary>
        private static bool ChangeFanStatus(Fan fan, State state)
        {
            var memoryStatusByte = fan == Fan.intake ? SharedMemoryByte.IntakeFanStatus: SharedMemoryByte.ExhaustFanStatus;

            return ChangeStatus(memoryStatusByte, state);
        }

        /// <summary>
        /// Return fan control to automatic.
        /// </summary>
        private static bool ResetFanStatus(Fan fan)
        {
            var memoryStatusByte = fan == Fan.intake ? SharedMemoryByte.IntakeFanStatus : SharedMemoryByte.ExhaustFanStatus;

            return ResetStatus(memoryStatusByte);
        }

        private static void ChangeFanDutyCycle(Fan fan, double dutyCycle)
        {
            var client = _serviceProvider.GetService<IIpcServiceClient>();
            client!.SendMessage(fan == Fan.intake ? SharedMemoryByte.IntakeFanDutyCycle : SharedMemoryByte.ExhaustFanDutyCycle, dutyCycle);
        }

        private static bool ChangeStatus(SharedMemoryByte memoryStatusByte, State state)
        {
            var svc = _serviceProvider.GetRequiredService<ISharedMemoryService>();
            var currentStatus = svc.Read(memoryStatusByte);
            var newStatus = state == State.on ? ServiceStatus.OnByManual : ServiceStatus.OffByManual;

            //Execute the change only if not change from ON (service or manual) to ON (manual) or from OFF (service or manual) to OFF (manual)
            if (!(((currentStatus == ServiceStatus.OnByService || currentStatus == ServiceStatus.OnByManual) && newStatus == ServiceStatus.OnByManual) ||
                ((currentStatus == ServiceStatus.OffByService || currentStatus == ServiceStatus.OffByManual) && newStatus == ServiceStatus.OffByManual)))
            {
                var client = _serviceProvider.GetService<IIpcServiceClient>();
                client!.SendMessage(memoryStatusByte, newStatus);

                return true;
            }

            return false;
        }

        private static bool ResetStatus(SharedMemoryByte memoryStatusByte)
        {
            var svc = _serviceProvider.GetRequiredService<ISharedMemoryService>();
            var currentStatus = svc.Read(memoryStatusByte);

            //reset only executes if current control is manual
            if (currentStatus != ServiceStatus.OnByService
                && currentStatus != ServiceStatus.OffByService)
            {
                var client = _serviceProvider.GetService<IIpcServiceClient>();
                client!.SendMessage(memoryStatusByte, currentStatus, true);

                return true;
            }

            return false;
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

            return services.BuildServiceProvider();
        }
    }
}