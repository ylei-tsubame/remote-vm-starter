using System;
using System.Diagnostics;

namespace RemoteVMStarter
{
    public class RemoteVMStarterClientMain
    {
        public static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                string listenerAddressString = args[0];
                Int32 listenerPort = Int32.Parse(args[1]);
                string vmKey = args[2];

                RemoteVMStarterClient client = new RemoteVMStarterClient(
                    listenerAddressString, listenerPort
                );
                client.SendVMKey(vmKey);
            }
            else
            {
                Trace.TraceError(
                    "Positional arguments required: [destAddress] [destPort] [vmKey]"
                );
            }
        }
    }
}
