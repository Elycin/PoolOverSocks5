using Starksoft.Aspen.Proxy;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PoolOverSocks5
{
    class RelayHandler
    {
        // Placeholder variable to inherit the configuration class.
        private ConfigurationHandler configuration;

        // Thread signal.  
        public ManualResetEvent allDone = new ManualResetEvent(false);

        // Incoming data from the client.  
        public static string data = null;

        // Client that will connect to the Socks5 Proxy.
        public Socks5ProxyClient relayClientProxy;

        // TCP client to the pool.
        public TcpClient relayClient;

        // the default buffer size for network transfers.
        public Int32 buffersize = 1024; //bytes

        public RelayHandler(ConfigurationHandler configuration)
        {
            // Inherit from the main class.
            this.configuration = configuration;
        }

        /*
         * Relay Worker
         * This is the heart of the applicaition that does all the transpling.
         */
        public void Work()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[buffersize];

            // Establish the local endpoint for the socket.  
            IPAddress relayIPAddress = IPAddress.Parse(configuration.configuration.RelayAddress);
            IPEndPoint relayEndpoint = new IPEndPoint(relayIPAddress, configuration.configuration.RelayPort);

            // Create a TCP/IP socket.  
            Socket relayListener = new Socket(relayIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try {
                // Bind to the local interface.
                relayListener.Bind(relayEndpoint);
                relayListener.Listen(1);

                // Wait for an incoming connection.
                Console.WriteLine("Waiting for your miner to connect...");
                Socket handler = relayListener.Accept();

                // A miner has connected.
                Console.WriteLine("Your miner has connected!");

                // Connect to the socks5 proxy
                relayClientProxy = new Socks5ProxyClient(configuration.configuration.ProxyAddress, configuration.configuration.ProxyPort, "", "");
                Console.WriteLine("Successfully connected to your Socks5 Proxy!");

                // Connect to the pool
                relayClient = relayClientProxy.CreateConnection(configuration.configuration.PoolAddress, configuration.configuration.PoolPort);
                Console.WriteLine("Successfully connected to your pool!");

                // Loop while both sockets are connected.
                while (relayClient.Client.Connected && handler.Connected)
                {
                    // Simple 10ms sleep to not chew up the CPU.
                    Thread.Sleep(10);

                    // Clear the byte buffer by reinitialization.
                    bytes = new byte[buffersize];

                    // Make sure we're still connected to the miner.
                    if (handler.Connected) {
                        // Only process if there's data available.
                        if (handler.Available != 0) {
                            int bytesReceived = handler.Receive(bytes);
                            data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                            Console.WriteLine("MINER: '{0}'", data);
                            byte[] message = Encoding.ASCII.GetBytes(data);
                            relayClient.Client.Send(message);
                        }
                    } else {
                        // Client has disconnected - close the sockets and restart.
                        relayListener.Close();

                        // If the proxy is still connected, disconnect.
                        if (relayClientProxy.TcpClient.Connected) relayClientProxy.TcpClient.Close();

                        // Re-call the current function.
                        Work();
                    }

                    // Make sure we're still connected to the proxy
                    if (relayClient.Connected) {
                        // Only process if there's data available.
                        if (relayClient.Available != 0) {
                            byte[] relayRecv = new byte[buffersize];
                            int bytesReceived = relayClient.Client.Receive(relayRecv);
                            String dataInFromProxy = Encoding.ASCII.GetString(relayRecv, 0, bytesReceived);
                            Console.WriteLine("Socks5 Proxy: '{0}'", dataInFromProxy);
                            handler.Send(relayRecv, 0, bytesReceived, SocketFlags.None);
                        }
                    } else {
                        // Client has disconnected - close the sockets and restart.
                        relayListener.Close();

                        // Re-call the current function.
                        Work();
                    }
                }
            } catch ( Exception e) {
                // Generic exception handling.
                Console.WriteLine("An exception has occured in the relay.");

                Console.WriteLine(e.ToString());
                Program.PressAnyKeyToExit();
            }
        }
    }
}
