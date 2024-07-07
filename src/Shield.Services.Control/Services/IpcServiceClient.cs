using Shield.Common.Domain;
using System.Net.Sockets;
using System.Text;

namespace Shield.Services.Control.Services
{
    public class IpcServiceClient : IIpcServiceClient
    {
        /// <summary>
        /// Send message to server.
        /// </summary>
        /// <param name="statusByte"></param>
        /// <param name="status"></param>
        /// <param name="reset"></param>
        /// <returns>IcpMessage object with response from server.</returns>
        public IpcMessage? SendMessage(SharedMemoryByte statusByte, ServiceStatus status, bool reset)
        {
            IpcMessage? message;

            using var Client = new TcpClient("localhost", Constants.IPC_PORT);
            
            try
            {
                using var stream = Client.GetStream();

                //send message to network stream
                var ipcMessage = new IpcMessage{ MemoryByte = statusByte, Status = status, ResetStatus = reset };

                var messageToSend = Encoding.UTF8.GetBytes(ipcMessage.Serialize());
                stream.Write(messageToSend, 0, messageToSend.Length);

                var buffer = new byte[512];
                int bytesRead = 0;
                
                //wait return message
                do 
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Task.Delay(100).Wait();

                } while (bytesRead == 0);

                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                message = IpcMessage.Deserialize(receivedMessage);
            }
            catch(Exception ex)
            { 
                message = IpcErrorMessage.Create(new ApplicationException("Error sending IpcMessage.", ex));
            }

            return message;
        }

        public IpcMessage? SendMessage(SharedMemoryByte memoryStatusByte, double dutyCycle)
        {
            IpcMessage? message;

            using var Client = new TcpClient("localhost", Constants.IPC_PORT);

            try
            {
                using var stream = Client.GetStream();

                //send message to network stream
                var ipcMessage = new IpcFanMessage { MemoryByte = memoryStatusByte, DutyCycle = dutyCycle };

                var messageToSend = Encoding.UTF8.GetBytes(ipcMessage.Serialize());
                stream.Write(messageToSend, 0, messageToSend.Length);

                var buffer = new byte[512];
                int bytesRead = 0;

                //wait return message
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Task.Delay(100).Wait();

                } while (bytesRead == 0);

                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                message = IpcMessage.Deserialize(receivedMessage);
            }
            catch (Exception ex)
            {
                message = IpcErrorMessage.Create(new ApplicationException("Error sending IpcMessage.", ex));
            }

            return message;
        }
    }
}
