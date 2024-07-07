using Shield.Common.Domain;
using Shield.Common.Interfaces;
using Shield.Services.Display;
using Shield.Services.Fan;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Shield.Services.Ipc
{
    public class IpcServiceServer(ILogger<IpcServiceServer> logger,
        IPrimaryDisplayWorker primaryDisplayWorker,
        ISecondaryDisplayWorker secondaryDisplayWorker,
        IIntakeFanWorker intakeFanWorker,
        IExhaustFanWorker exhaustFanWorker,
        ISharedMemoryService sharedMemoryService) : IIpcServiceServer
    {
        private readonly TcpListener _listener = new(IPAddress.Any, Constants.IPC_PORT);
        private readonly ILogger<IpcServiceServer> _logger = logger;
        private readonly IPrimaryDisplayWorker _primaryDisplayWorker = primaryDisplayWorker;
        private readonly ISecondaryDisplayWorker _secondaryDisplayWorker = secondaryDisplayWorker;
        private readonly IIntakeFanWorker _intakeFanWorker = intakeFanWorker;
        private readonly IExhaustFanWorker _exhaustFanWorker = exhaustFanWorker;
        private readonly ISharedMemoryService _sharedMemoryService = sharedMemoryService;

        public void Execute()
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

                //Action to do according parameters (Exception and ServiceStatus.None)
                if (receivedMessage?.Exception is null) //No exception
                {
                    if (receivedMessage is IpcFanMessage fanMessage)
                    {
                        var worker = GetWorkerService(fanMessage.MemoryByte);
                        
                        if (worker is IFanWorker fanWorker) fanWorker.DutyCycle = fanMessage.DutyCycle;
                    }
                    else if (receivedMessage?.Status == ServiceStatus.None) //read current display status
                    {
                        receivedMessage.Status = _sharedMemoryService.Read(receivedMessage.MemoryByte);
                    }
                    else
                    {
                        UpdateServiceStatus(receivedMessage!);
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

        private static IpcMessage? ReceiveMessage(NetworkStream stream)
        {
            var buffer = new byte[512];
            var messageLenght = stream.Read(buffer, 0, buffer.Length);
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, messageLenght);

            return IpcMessage.Deserialize(receivedMessage);
        }

        private void UpdateServiceStatus(IpcMessage serviceMessage)
        {
            try
            {
                var worker = GetWorkerService(serviceMessage.MemoryByte);

                if (serviceMessage.ResetStatus)
                {
                    worker.ResetStatus();
                }
                else
                {
                    worker.Update(serviceMessage.Status);
                }
            }
            catch (Exception ex)
            {
                var message = $"{GetType().Name}: Error updating display.";
                _logger.LogError(ex, message);
                serviceMessage.Exception = new ApplicationException(message, ex);
            }
        }

        private IWorkerService GetWorkerService(SharedMemoryByte statusByte) => statusByte switch
        {
            SharedMemoryByte.PrimaryDisplayStatus => _primaryDisplayWorker,
            SharedMemoryByte.SecondaryDisplayStatus => _secondaryDisplayWorker,
            SharedMemoryByte.IntakeFanStatus => _intakeFanWorker,
            SharedMemoryByte.IntakeFanDutyCycle => _intakeFanWorker,
            SharedMemoryByte.ExhaustFanStatus => _exhaustFanWorker,
            SharedMemoryByte.ExhaustFanDutyCycle => _exhaustFanWorker,
            
            _ => throw new NotImplementedException()
        };
    }
}
