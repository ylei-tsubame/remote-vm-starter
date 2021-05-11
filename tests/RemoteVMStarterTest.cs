using System;
using System.Diagnostics;
using System.Threading;

namespace RemoteVMStarter
{
    public class RemoteVMStarterTest
    {
        public static void Main(string[] args)
        {
            string listenerAddressString = "127.0.0.1";
            Int32 listenerPort = 10999;

            RemoteVMStarterListener listener = new RemoteVMStarterListener(
                listenerAddressString, listenerPort
            );
            RemoteVMStarterClient client = new RemoteVMStarterClient(
                listenerAddressString, listenerPort
            );

            Trace.TraceInformation("Testing listener start");

            listener.Start();

            Trace.TraceInformation("Testing all regular cases");

            for (byte vmKey = 1; vmKey != 0; vmKey += 1)
            {
                client.SendVMKey(vmKey.ToString());
            }

            // Unreliable way to wait until all clients finish.
            Thread.Sleep(3000);

            Trace.TraceInformation("Testing listener stop");

            listener.Stop();

            Trace.TraceInformation("Testing listener restart");

            listener.Start();

            Trace.TraceInformation("Testing irregular cases");

            client.SendVMKey("-10");
            client.SendVMKey("Placeholder");
            client.SendVMKey("0x06");
            client.SendVMKey("0b00101001");

            Trace.TraceInformation("Cleaning up listener");

            listener.Stop();
        }
    }
}
