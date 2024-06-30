using Shield.Common.Domain;
using Shield.Common.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Shield.Display.Services
{
    public class IpcServiceServer(ILogger<IpcServiceServer> logger,
        IPrimaryDisplayWorker primaryDisplayWorker,
        ISecondaryDisplayWorker secondaryDisplayWorker,
        ISharedMemoryService sharedMemoryService) : IIpcServiceServer
    {
        private readonly TcpListener _listener = new(IPAddress.Any, Constants.IPC_PORT);
        private readonly ILogger<IpcServiceServer> _logger = logger;
        private readonly IPrimaryDisplayWorker _primaryDisplayWorker = primaryDisplayWorker;
        private readonly ISecondaryDisplayWorker _secondaryDisplayWorker = secondaryDisplayWorker;
        private readonly ISharedMemoryService _sharedMemoryService = sharedMemoryService;

        public void Start()
        {
            _listener.Start();
            _listener.BeginAcceptTcpClient(ClientCallback, null);

            _logger.LogInformation("Ipc server has been started. Listening for client.");
        }

        private void ClientCallback(IAsyncResult result)
        {
            var client = _listener.EndAcceptTcpClient(result);

            try
            {
                using var stream = client.GetStream();
                var receivedMessage = ReceiveMessage(stream);
                
                //Action to do according parameters (actualy Exception and DisplayBacklightStatus.None
                if (receivedMessage?.Exception is null) //No exception
                {
                    if (receivedMessage?.Status == DisplayBacklightStatus.None) //read current display status
                    {
                        receivedMessage.Status = _sharedMemoryService.Read(receivedMessage.Lcd);
                    }
                    else //Update the display
                    {
                        UpdateDisplay(receivedMessage!);
                    }
                }

                //send response to client
                var msg = Encoding.UTF8.GetBytes(receivedMessage!.Serialize());
                stream.Write(msg, 0, msg.Length);
            }
            finally
            {
                client?.Close();
                client?.Dispose();

                _listener.BeginAcceptTcpClient(ClientCallback, null);
            }
        }

        private IpcMessage? ReceiveMessage(NetworkStream stream)
        {
            var buffer = new byte[512];
            var messageLenght = stream.Read(buffer, 0, buffer.Length);
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, messageLenght);

            return IpcMessage.Deserialize(receivedMessage);
        }

        private void UpdateDisplay(IpcMessage serviceMessage)
        {
            try
            {
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
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = $"{GetType().Name}: Error updating display.";
                _logger.LogError(ex, message);
                serviceMessage.Exception = new ApplicationException(message, ex);
            }
        }
    }
}
