using Starksoft.Aspen.Proxy;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PoolOverSocks5
{
    internal class RelayHandler
    {
        // Class Inheritance
        private ConfigurationHandler configuration;

        // TCP Socket for the whole class
        public TcpListener relay;

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
            // Start Listening
            relay = new TcpListener(IPAddress.Parse(configuration.configuration.RelayAddress), configuration.configuration.RelayPort);
            relay.Start();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("Relay has started listening on {0}:{1} - Connect your miners to this address!", configuration.configuration.RelayAddress, configuration.configuration.RelayPort));
            Console.ResetColor();

            while (true)
            {
                // wait for client connection
                TcpClient newClient = relay.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(newClient);
            }
        }

        public void HandleClient(object obj)
        {
            TcpClient minerClient = (TcpClient)obj;
            Socks5ProxyClient proxyClient;
            TcpClient poolClient;

            // Try to connect to the socks5 proxy
            proxyClient = new Socks5ProxyClient(configuration.configuration.ProxyAddress, configuration.configuration.ProxyPort, "", "");
            Program.LogResponderHandler("Application", "Successfully connected to your Socks5 proxy");

            poolClient = proxyClient.CreateConnection(configuration.configuration.PoolAddress, configuration.configuration.PoolPort);
            Program.LogResponderHandler("Application", "Successfully connected to your pool");

            // Register a success
            Program.LogResponderHandler("Application", "Your miner will now get the data from the pool over the socks5 proxy");

            Thread.Sleep(500);

            while (minerClient.Connected && poolClient.Connected)
            {
                // Small sleep so we don't use 100% of the cpu
                Thread.Sleep(10);

                // Exchange the data.
                ExchangeData(minerClient, poolClient);
            }

            // Close all connections if it's open, this thread is done.
            SafeClose(minerClient);
            SafeClose(poolClient);
            SafeClose(proxyClient.TcpClient);
        }

        private void ExchangeData(TcpClient miner, TcpClient pool)
        {
            try
            {
                if (miner.Available != 0)
                {
                    byte[] receivedMinerData = new byte[4096];
                    int bytesReceived = miner.Client.Receive(receivedMinerData);
                    string data = Encoding.ASCII.GetString(receivedMinerData, 0, bytesReceived);
                    Program.LogResponderHandler("Miner", data.Substring(0, data.Length - 1));
                    byte[] message = Encoding.ASCII.GetBytes(data);
                    pool.Client.Send(message);
                }

                if (pool.Available != 0)
                {
                    byte[] recievedPoolData = new byte[4096];
                    int bytesReceived = pool.Client.Receive(recievedPoolData);
                    String dataInFromProxy = Encoding.ASCII.GetString(recievedPoolData, 0, bytesReceived);
                    Program.LogResponderHandler("Proxy", dataInFromProxy.Substring(0, dataInFromProxy.Length - 1));
                    miner.Client.Send(recievedPoolData, 0, bytesReceived, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                // Can suppress - error while exchanging data, someone disconnected.
            }
        }

        private void SafeClose(TcpClient client)
        {
            try
            {
                if (client.Connected) client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Exception occured while closing client:\n{0}", e.ToString()));
            }
        }
    }
}