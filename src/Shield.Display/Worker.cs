using Shield.Common.Domain;

namespace Shield.Display
{
    public class Worker(ILogger<Worker> logger,
        IDisplayWorker displayWorker) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IDisplayWorker _displayWorker = displayWorker;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int climateWait = 0, retries = 0;

            _logger.LogInformation(Constants.SERVICE_STARTED);

            await _displayWorker.WelcomeAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Block process to control possible concurrency with Shield.Display.Backline execution
                    using (var mutex = Program.StartMutex())
                    {
                        _displayWorker.UpdateTime();

                        //Update climate information each 15 minutes
                        if (climateWait % 15 == 0)
                        {
                            _displayWorker.UpdateClimateInformationAsync(stoppingToken).Wait(stoppingToken);
                            climateWait = 0;
                        }

                        _displayWorker.ControlBacklightSchedule();

                        mutex.ReleaseMutex();
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                    retries = 0;
                    climateWait++;
                }
                catch (Exception ex)
                {
                    _displayWorker.FatalError(ex);
                    retries++;
                    //Ends application after 3 consecutive exceptions
                    if (retries > 3) throw;
                }
            }
        }
    }
}
