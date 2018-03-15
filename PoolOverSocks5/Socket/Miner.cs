using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Starksoft.Aspen.Proxy;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PoolOverSocks5.Socket
{
    internal class Miner
    {
        // Configuration class inheritance variable;
        private ConfigurationHandler configuration;

        // The maximum size of the pending buffer per frame.
        private const int MAX_BUFFER_SIZE = 4096;

        // Identification
        public Int32 id;

        // Socket variables
        private TcpClient MinerConnection;
        private TcpClient PoolConnection;
        private Socks5ProxyClient ProxyConnection;

        // Thread Variables
        private Thread thread;

        // Dispoasl boolean.
        public bool wantsToBeDisposed;

        // Changing variables
        private int bytesReceived = 0;
        private byte[] incomingData = null;
        private string incomingDataString = null;
        private JObject parsedSerializer = new JObject();

        // Class Constructor
        public Miner(Int32 miner_id,  ConfigurationHandler configuration, TcpClient client)
        {
            // Let the console know a miner is attempting to connect.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(String.Format("Miner ID {0} has connected.", miner_id));
            Console.ResetColor();

            // Remember our assigned ID.
            this.id = miner_id;

            // Inherit the configuration class.
            this.configuration = configuration;

            // Inherit the TCP Client.
            MinerConnection = client;

            // Initialize the new thread.
            thread = new Thread(Run);

            // Run the thread.
            thread.Start();
        }

        // The working thread.
        private void Run()
        {
            try
            {
                // Try to connect to the proxy.
                ProxyConnection = new Socks5ProxyClient(configuration.configuration.ProxyAddress, configuration.configuration.ProxyPort, "", "");

                // Try to connect to the pool
                PoolConnection = ProxyConnection.CreateConnection(configuration.configuration.PoolAddress, configuration.configuration.PoolPort);

                // Write to the console that the pool has beenc onnected.
                Program.ConsoleWriteLineWithColor(ConsoleColor.Green, "Successfully connected to your pool!");
                Program.ConsoleWriteLineWithColor(ConsoleColor.Green, "The new miner is ready to mine!");
            }
            catch (Exception exception)
            {
                Program.ConsoleWriteLineWithColor(ConsoleColor.Red, "Failed to establish a connection to the pool, the miner will be disconnected.");
                Console.WriteLine(exception.ToString());
            }

            // Main routine that sleeps and exchanges data to prevent high cpu usage.
            while (MinerConnection.Connected && PoolConnection.Connected)
            {
                // Small sleep so we don't use 100% of the cpu
                Thread.Sleep(10);

                // Exchange the data.
                ExchangeData();
            }

            // See you, space cowboy.
            wantsToBeDisposed = true;
        }

        // Exchanges data between the pool and the client.
        private void ExchangeData()
        {
            try
            {
                if (MinerConnection.Available != 0)
                {
                    // re-initializze the buffer.
                    incomingData = new byte[MAX_BUFFER_SIZE];

                    // Determine the new buffer size from the incoming data from the miner.
                    bytesReceived = MinerConnection.Client.Receive(incomingData);

                    // Parse as string to chop the buffer down and parse json.
                    incomingDataString = Encoding.ASCII.GetString(incomingData, 0, bytesReceived);
                    parsedSerializer = JObject.Parse(@incomingDataString.Substring(0, incomingDataString.Length - 1));

                    // Log to the console what we have.
                    Program.LogResponderHandler("Miner", JsonConvert.SerializeObject(parsedSerializer, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() }));

                    // Send to the pool.
                    PoolConnection.Client.Send(Encoding.ASCII.GetBytes(incomingDataString), 0, bytesReceived, SocketFlags.None);
                }

                if (PoolConnection.Available != 0)
                {
                    // re-initializze the buffer.
                    incomingData = new byte[MAX_BUFFER_SIZE];

                    // Determine the new buffer size from the incoming data from the pool.
                    bytesReceived = PoolConnection.Client.Receive(incomingData);

                    // Parse as string to chop the buffer down and parse json.
                    incomingDataString = Encoding.ASCII.GetString(incomingData, 0, bytesReceived);
                    parsedSerializer = JObject.Parse(@incomingDataString.Substring(0, incomingDataString.Length - 1));

                    // Log to the console what we have.
                    Program.LogResponderHandler("Pool", JsonConvert.SerializeObject(parsedSerializer, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() }));

                    // Send to the miner.
                    MinerConnection.Client.Send(Encoding.ASCII.GetBytes(incomingDataString), 0, bytesReceived, SocketFlags.None);
                }
            }
            catch (Exception exception)
            {
                // Write information to the console.
                Program.ConsoleWriteLineWithColor(ConsoleColor.Red, "There was an exception while attempting to exchange data between the pool and the client.");
                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "The connection will be dropped.");
                Console.WriteLine(exception.ToString());

                // Safely close all the socket connections to free up resources.
                SafeClose(MinerConnection);
                SafeClose(PoolConnection);

                // Upon the next iteration of the while loop in the Run void, the thread will exit and this will be the end.
            }
        }

        private void SafeClose(TcpClient client)
        {
            try
            {
                if (client != null) if (client.Connected) client.Close();
            }
            catch (Exception e)
            {
                Program.ConsoleWriteLineWithColor(ConsoleColor.Red, "Failed to close TCP Socket: " + e.ToString());
            }
        }

        
    }
}