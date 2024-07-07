using Shield.Common.Domain;
using Shield.Services.Display;
using Shield.Services.Fan;
using Shield.Services.Ipc;
using System.Device.Gpio;

namespace Shield.Services
{
    public sealed class Worker(ILogger<Worker> logger,
        IPrimaryDisplayWorker primaryDisplayWorker,
        ISecondaryDisplayWorker secondaryDisplayWorker,
        IIntakeFanWorker fanInWorker,
        IExhaustFanWorker fanOutWorker,
        IIpcServiceServer ipcServiceServer) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IPrimaryDisplayWorker _primaryDisplayWorker = primaryDisplayWorker;
        private readonly ISecondaryDisplayWorker _secondaryDisplayWorker = secondaryDisplayWorker;
        private readonly IIntakeFanWorker _fanInWorker = fanInWorker;
        private readonly IExhaustFanWorker _fanOutWorker = fanOutWorker;
        private readonly IIpcServiceServer _ipcServiceServer = ipcServiceServer;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Led(true);

            _ipcServiceServer.Execute();

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Led(false);

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            Led(false);
            
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(Constants.SERVICE_STARTED);

            var taskList = new List<Task>
            {
                Task.Run(() => _primaryDisplayWorker.Execute(), stoppingToken),
                Task.Run(() => _secondaryDisplayWorker.Execute(), stoppingToken),
                Task.Run(() => _fanInWorker.Execute(), stoppingToken),
                Task.Run(() => _fanOutWorker.Execute(), stoppingToken)
            };

            Task.WaitAll([.. taskList], stoppingToken);

            return Task.CompletedTask;
        }

        private static void Led(bool on)
        {
            int pin = 17;
            using var controller = new GpioController();
            controller.OpenPin(pin, PinMode.Output);

            controller.Write(pin, on ? PinValue.High : PinValue.Low);
        }
    }
}
