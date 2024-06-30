using Shield.Common.Domain;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Shield.Display.Services
{
    public class IpcServiceServer(ILogger<IpcServiceServer> logger,
        IPrimaryDisplayWorker primaryDisplayWorker,
        ISecondaryDisplayWorker secondaryDisplayWorker) : IIpcServiceServer
    {
        private readonly TcpListener _listener = new(IPAddress.Any, Constants.IPC_PORT);
        private readonly ILogger<IpcServiceServer> _logger = logger;
        private readonly IPrimaryDisplayWorker _primaryDisplayWorker = primaryDisplayWorker;
        private readonly ISecondaryDisplayWorker _secondaryDisplayWorker = secondaryDisplayWorker;

        public void Start()
        {
            _listener.Start();

            StartClientCallback();

            _logger.LogInformation("Ipc server has been started. Listening for client.");

        }

        private void StartClientCallback()
        {
            _listener.BeginAcceptTcpClient(ClientCallback, null);
        }

        private void ClientCallback(IAsyncResult result)
        {
            var client = _listener.EndAcceptTcpClient(result);

            StartClientCallback();

            try
            {
                var buffer = new byte[512];
                using var stream = client.GetStream();

                var messageLenght = stream.Read(buffer, 0, buffer.Length);
                var messageReceived = Encoding.UTF8.GetString(buffer, 0, messageLenght);

                _logger.LogDebug(messageReceived);

                var serviceMessage = IpcServiceMessage.Deserialize(messageReceived);

                //Do the job
                switch (serviceMessage?.Lcd)
                {
                    case Common.Domain.Lcd.Primary:
                        if (serviceMessage.ResetStatus)
                        {
                            _primaryDisplayWorker.Welcome();
                            _primaryDisplayWorker.UpdateTime();
                            _primaryDisplayWorker.UpdateClimateInformation();
                            _primaryDisplayWorker.ControlBacklightSchedule(Common.Domain.Lcd.Primary);
                        }
                        else
                        {
                            _primaryDisplayWorker.UpdateTime();
                            _primaryDisplayWorker.UpdateClimateInformation();
                            _primaryDisplayWorker.BacklightStatus = serviceMessage.Status;
                        }

                        _logger.LogDebug($"Command {serviceMessage.Status} for {Common.Domain.Lcd.Primary}.");
                        break;
                    case Common.Domain.Lcd.Secondary:
                        if (serviceMessage.ResetStatus)
                        {
                            _secondaryDisplayWorker.BacklightStatus = DisplayBacklightStatus.OffByService;
                            _secondaryDisplayWorker.ControlBacklightSchedule(Common.Domain.Lcd.Secondary);
                        }
                        else 
                        {
                            _secondaryDisplayWorker.BacklightStatus = serviceMessage.Status;
                        }

                        _logger.LogDebug($"Command {serviceMessage.Status} for {Common.Domain.Lcd.Secondary}.");
                        break;
                }
            }
            finally
            {
                client?.Close();
                client?.Dispose();
            }
        }
    }
}
