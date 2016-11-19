using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace CustomService
{
    public partial class CustomService : ServiceBase
    {
        private Thread listenerThread;
        #region ctors
        public CustomService(string[] args)
        {
            InitializeComponent();
            string eventSourceName = "Source";
            string logName = "NewLog";

            if (args.Count() > 0)
            {
                eventSourceName = args[0];
            }
            if (args.Count() > 1)
            {
                logName = args[1];
            }
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        public CustomService()
        {
            InitializeComponent();

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("Source"))
            {
                EventLog.CreateEventSource("Source", "NewLog");
            }
            eventLog1.Source = "Source";
            eventLog1.Log = "NewLog";
        }
        #endregion

        protected override void OnStart(string[] args)
        {
            listenerThread = new Thread(th => ListenPort(11000));
            listenerThread.Start();
            
        }

        /// <summary>
        /// Listen somebody port
        /// </summary>
        /// <param name="port">The port</param>
        private void ListenPort(int port)
        {
            // Install socket local endpoint
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            // Create socket Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // ssign a local socket endpoint, and listens for incoming sockets
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                // Started listening connection
                while (true)
                {
                    Console.WriteLine("Expect connection port {0}", ipEndPoint);

                    //The program is paused, waiting for an incoming connection
                    Socket handler = sListener.Accept();
                    string data = null;

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    data += System.Text.Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    // Send a reply to the client
                    string reply = "Thank you for your request to " + data.Length.ToString()
                            + " characters";
                    byte[] msg = Encoding.UTF8.GetBytes(reply);
                    handler.Send(msg);
                                       
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry(ex.Message);
            }
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }

        protected override void OnStop()
        {
            listenerThread.Abort();
            listenerThread.Join();
            eventLog1.WriteEntry("In onStop.");
        }
    }
}
