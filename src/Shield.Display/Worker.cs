using Shield.Common.Domain;
using Shield.Display.Services;
using System.Device.Gpio;

namespace Shield.Display
{
    public sealed class Worker(ILogger<Worker> logger,
        IPrimaryDisplayWorker primaryDisplayWorker,
        ISecondaryDisplayWorker secondaryDisplayWorker,
        IIpcServiceServer ipcServiceServer) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IPrimaryDisplayWorker _primaryDisplayWorker = primaryDisplayWorker;
        private readonly ISecondaryDisplayWorker _secondaryDisplayWorker = secondaryDisplayWorker;
        private readonly IIpcServiceServer _ipcServiceServer = ipcServiceServer;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Led(true);

            _ipcServiceServer.Start();

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

            var taskPrimaryDisplay = Task.Run(() => _primaryDisplayWorker.Execute(), stoppingToken);
            var taskSecondaryDisplay = Task.Run(() => _secondaryDisplayWorker.Execute(), stoppingToken);

            Task.WaitAll([taskPrimaryDisplay, taskSecondaryDisplay], cancellationToken: stoppingToken);

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
