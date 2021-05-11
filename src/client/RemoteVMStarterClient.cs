using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteVMStarter
{
    public class RemoteVMStarterClient
    {
        private string listenerAddressString;
        private Int32 listenerPort;

        public RemoteVMStarterClient(string addressString, Int32 port)
        {
            this.listenerAddressString = addressString;
            this.listenerPort = port;
        }

        public void SendVMKey(string vmKeyString)
        {
            byte vmKey;

            if (Byte.TryParse(vmKeyString, out vmKey))
            {
                TcpClient client = new TcpClient(
                    this.listenerAddressString, this.listenerPort
                );
                client.SendBufferSize = RemoteVMStarterConstants.DataSize;

                NetworkStream clientStream = client.GetStream();
                // Send raw byte; no need to encode using StreamWriter.
                BinaryWriter clientStreamWriter = new BinaryWriter(clientStream);

                clientStreamWriter.Write(vmKey);
                clientStreamWriter.Flush();

                clientStreamWriter.Close();
                clientStream.Close();
                client.Close();
            }
            else
            {
                Trace.TraceError(
                    "Failed to convert [{0}] to byte", vmKeyString
                );
            }
        }
    }
}
