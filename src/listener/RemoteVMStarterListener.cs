using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace RemoteVMStarter
{
    public class RemoteVMStarterListener
    {
        private IPAddress listenerAddress;
        private Int32 listenerPort;

        private TcpListener listener;
        private Thread clientHandler;
        private ManualResetEventSlim stopped;

        public RemoteVMStarterListener(string addressString, Int32 port)
        {
            this.listenerAddress = IPAddress.Parse(addressString);
            this.listenerPort = port;

            // Reusable; compared to the listener and the clientHandler thread.
            this.stopped = new ManualResetEventSlim(false);
        }

        public void Start()
        {
            if (this.listener == null)
            {
                this.listener = new TcpListener(
                    this.listenerAddress, this.listenerPort
                );
                this.clientHandler = new Thread(
                    new ThreadStart(this.BeginAcceptClient)
                );

                this.listener.Start();
                this.clientHandler.Start();

                Trace.TraceInformation(
                    "Listener started on [{0}:{1}]",
                    this.listenerAddress,
                    this.listenerPort
                );
            }
        }

        public void Stop()
        {
            if (this.listener != null)
            {
                this.stopped.Set();

                this.clientHandler.Join();
                this.listener.Stop();

                // Release for garbage collection.
                this.clientHandler = null;
                this.listener = null;

                Trace.TraceInformation("Listener stopped");
            }
        }

        public void BeginAcceptClient()
        {
            while (!this.stopped.IsSet)
            {
                if (this.listener.Pending())
                {
                    TcpClient client = this.listener.AcceptTcpClient();
                    client.ReceiveBufferSize = RemoteVMStarterConstants.DataSize;

                    NetworkStream clientStream = client.GetStream();

                    byte[] bytes = new byte[RemoteVMStarterConstants.DataSize];
                    clientStream.Read(
                        bytes, 0, RemoteVMStarterConstants.DataSize
                    );

                    string[] vmNames = this.GetVMNames(bytes);

                    if (vmNames != null)
                    {
                        foreach (string vmName in vmNames)
                        {
                            this.StartVM(vmName);
                        }
                    }

                    clientStream.Close();
                    client.Close();
                }
            }
        }

        private string[] GetVMNames(byte[] bytes)
        {
            string vmKey = BitConverter.ToString(bytes);

            Trace.TraceInformation("VM key = [{0}]", vmKey);

            string vmNamesString = ConfigurationManager.AppSettings[vmKey];

            char[] delimiters = new char[] { ',' };

            return vmNamesString == null
                ? null
                : vmNamesString.Split(
                    delimiters, StringSplitOptions.RemoveEmptyEntries
                );
        }

        private string GetVMState(string vmName)
        {
            Collection<PSObject> psObjects = PowerShell
                .Create()
                .AddCommand("Get-VM")
                .AddParameter("Name", vmName)
                .Invoke();

            return psObjects.Count == 1
                ? psObjects[0].Properties["State"].Value.ToString()
                : null;
        }

        private void StartVM(string vmName)
        {
            Trace.TraceInformation("VM name = [{0}]", vmName);

            string vmState = this.GetVMState(vmName);

            Trace.TraceInformation("VM state = [{0}]", vmState);

            if (vmState == RemoteVMStarterConstants.VMStates.Off)
            {
                Trace.TraceInformation(
                    "All conditions met; starting VM [{0}]", vmName
                );

                PowerShell
                    .Create()
                    .AddCommand("Start-VM")
                    .AddParameter("Name", vmName)
                    .Invoke();
            }
        }
    }
}
