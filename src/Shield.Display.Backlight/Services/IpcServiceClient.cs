using Shield.Common.Domain;
using System.Net.Sockets;
using System.Text;

namespace Shield.Display.Backlight.Services
{
    public class IpcServiceClient : IIpcServiceClient
    {
        /// <summary>
        /// Send message to server.
        /// </summary>
        /// <param name="lcd"></param>
        /// <param name="status"></param>
        /// <param name="reset"></param>
        /// <returns>IcpMessage object with response from server.</returns>
        public IpcMessage? SendMessage(Lcd lcd, DisplayBacklightStatus status, bool reset)
        {
            var Client = new TcpClient("localhost", Constants.IPC_PORT);
            
            try
            {
                using var stream = Client.GetStream();
                //send message to network stream
                var icpMessage = new IpcMessage { Lcd = lcd, Status = status, ResetStatus = reset };

                var messageToSend = Encoding.UTF8.GetBytes(icpMessage.Serialize());
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
                var msg = IpcMessage.Deserialize(receivedMessage);

                return msg;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;//TODO change to include exception and define what to do.
            }
        }
    }
}
