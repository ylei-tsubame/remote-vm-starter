using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;

namespace RemoteVMStarter
{
    public class RemoteVMStarterService : ServiceBase
    {
        RemoteVMStarterListener listener;

        public RemoteVMStarterService()
        {
            this.ServiceName = "RemoteVMStarterService";
            this.CanStop = true;
            // Use the default EventLog instead of making a custom one.
            this.AutoLog = true;
        }

        public static void Main()
        {
            ServiceBase.Run(new RemoteVMStarterService());
        }

        protected override void OnStart(string[] args)
        {
            string listenerAddressString = ConfigurationManager
                .AppSettings["listenerAddress"];
            Int32 listenerPort = Int32.Parse(
                ConfigurationManager.AppSettings["listenerPort"]
            );

            Trace.TraceInformation(
                "Listener address = [{0}]\nListener port = [{1}]",
                listenerAddressString,
                listenerPort
            );

            this.listener = new RemoteVMStarterListener(
                listenerAddressString, listenerPort
            );
            this.listener.Start();
        }

        protected override void OnStop()
        {
            this.listener.Stop();
            // Release for garbage colletion.
            this.listener = null;
        }
    }
}
