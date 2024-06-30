using Shield.Common.Domain;
using System.Net.Sockets;
using System.Text;

namespace Shield.Display.Backlight.Services
{
    public class IpcServiceClient : IIpcServiceClient
    {
        private readonly TcpClient _tcpClient = new ("shield", Constants.IPC_PORT);

        public void SendMessage(Lcd lcd, DisplayBacklightStatus status, bool reset)
        {
            var message = new IpcServiceMessage(lcd, status, reset);

            try
            {
                using var stream = _tcpClient.GetStream();
                var msg = Encoding.UTF8.GetBytes(message.Serialize());
                stream.Write(msg, 0, msg.Length);
            }
            finally
            {
                _tcpClient?.Close();
                _tcpClient?.Dispose();
            }
        }
    }
}
