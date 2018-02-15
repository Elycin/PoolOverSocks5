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
        private ConfigurationHandler configurationClass;

        // Number of miners
        private Int32 minerCount = 0;

        // TCP Socket for the whole class
        public TcpListener relay;

        // Class constructor
        public RelayHandler(ConfigurationHandler configuration)
        {
            // Inherit from the main class.
            this.configurationClass = configuration;
        }

        
        // Worker - The heart of the application.
        public void Work()
        {
            // Create a new listener instance
            relay = new TcpListener(IPAddress.Parse(configurationClass.configuration.RelayAddress), configurationClass.configuration.RelayPort);

            // Start the TCP Listener
            relay.Start();

            // Notify the console
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Format("Relay has started listening on {0}:{1} - Connect your miners to this address!", configurationClass.configuration.RelayAddress, configurationClass.configuration.RelayPort));
            Console.ResetColor();

            // Start listening for new clients and repeat.
            while (true)
            {
                // Wait for client connection
                TcpClient newClient = relay.AcceptTcpClient();

                // Createa a new thread and fire it.
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(newClient);
            }
        }

        public void HandleClient(object obj)
        {
            // Placehonder varaibles for the connections we'll use.
            TcpClient minerClient = (TcpClient)obj; // Cast the object as a Net.TcpClient (Polymorphism)
            Socks5ProxyClient proxyClient = null;
            TcpClient poolClient = null;

            // Let the console know a miner is attempting to connect
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("A miner is attempting to connect...");
            minerCount += 1;

            // Handle the main routine
            try
            {
                // Try to connect to the pool on the socks5 proxy
                proxyClient = new Socks5ProxyClient(configurationClass.configuration.ProxyAddress, configurationClass.configuration.ProxyPort, "", "");
                poolClient = proxyClient.CreateConnection(configurationClass.configuration.PoolAddress, configurationClass.configuration.PoolPort);
                Console.WriteLine("Successfully connected to your pool!");

                // We can signal that the miner is ready to mine
                Console.WriteLine("The new miner is ready to mine!");

                // Print how many miners are attached.
                Console.WriteLine(String.Format("There are currently {0} miner(s) connected.", minerCount));

                // Lastly, reset the console color for now.
                Console.ResetColor();
                
                // Main routine that sleeps and exchanges data to prevent high cpu usage. 
                while (minerClient.Connected && poolClient.Connected)
                {
                    // Small sleep so we don't use 100% of the cpu
                    Thread.Sleep(10);

                    // Exchange the data.
                    ExchangeData(minerClient, poolClient);
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to connect to the pool or the socks5 proxy - miner will be dropped");
                Console.ResetColor();
            }

            // Close all connections if it's open, this thread is done.
            SafeCloseAndDispose(minerClient);
            SafeCloseAndDispose(poolClient);
            SafeCloseAndDispose(proxyClient.TcpClient);

            // Counter and debug.
            minerCount -= 1;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("A miner has been disconnected.");
            Console.WriteLine(String.Format("There are currently {0} miner(s) connected.", minerCount));
            Console.ResetColor();
        }

        // Exchange Data Function, handles the packet interaction between the miner and the pool over the proxy.
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
                return;
            }
        }

        // Attempt to safecy close the 
        private void SafeCloseAndDispose(TcpClient client)
        {
            try
            {
                if (client != null) if (client.Connected) client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Exception occured while closing client:\n{0}", e.ToString()));
            } 
            finally
            {
                try
                {
                    client.Dispose();
                }
                catch (Exception e2)
                {
                    Console.WriteLine(string.Format("Failed to dispose TcpClient variable:\n{0}", e2.ToString()));
                }
            }
        }
    }
}