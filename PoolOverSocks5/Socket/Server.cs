using PoolOverSocks5.Socket;
using Starksoft.Aspen.Proxy;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PoolOverSocks5
{
    internal class Server
    {
        // Class Inheritance
        private ConfigurationHandler configurationClass;

        // Miner Connections
        private List<Miner> ConnectedMiners;

        // TCP Socket for the whole class
        public TcpListener ServerConnection;


        // Class constructor
        public Server(ConfigurationHandler configuration)
        {
            // Initialize the placeholder variables;
            ConnectedMiners = new List<Miner>();

            // Inherit from the main class.
            this.configurationClass = configuration;
        }

        
        // Worker - The heart of the application.
        public void Work()
        {
            // Create a new listener instance
            ServerConnection = new TcpListener(IPAddress.Parse(configurationClass.configuration.RelayAddress), configurationClass.configuration.RelayPort);

            // Start the TCP Listener
            ServerConnection.Start();

            // Notify the console
            string pendingData = string.Format("Relay has started listening on {0}:{1} - Connect your miners to this address!", configurationClass.configuration.RelayAddress, configurationClass.configuration.RelayPort);
            Program.ConsoleWriteLineWithColor(ConsoleColor.Green, pendingData);

            // Start listening for new clients and repeat.
            while (true)
            {
                // Wait for client connection
                TcpClient newClient = ServerConnection.AcceptTcpClient();

                // Cleanup old miners
                List<Miner> historicConnectedMiners = new List<Miner>(ConnectedMiners);
                foreach (Miner currentMiner in historicConnectedMiners) if (currentMiner.wantsToBeDisposed) ConnectedMiners.Remove(currentMiner);

                // Create a new miner
                Miner newMiner = new Miner(ConnectedMiners.Count + 1, configurationClass, newClient);

                // Keep track of it
                ConnectedMiners.Add(newMiner);

                // Write to the cosnole how many connections we have
                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, String.Format("There are now {0} miner(s) connected.", ConnectedMiners.Count));
            }
        }
    }
}