using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PoolOverSocks5
{
    class ConfigurationHandler
    {
        /*
         * Configuration Structure
         * This is a predefined layout of how the configuration data should be stored.
         * If this class is inherited, it can be accessed convieniently via the configuration variable below.
         * 
         * Example: ConfigurationStruct.configuration.poolAddress
         */
        public struct ConfigurationStruct
        {
            public string PoolAddress;
            public Int16 PoolPort;
            public string ProxyAddress;
            public Int16 ProxyPort;
            public string RelayAddress;
            public Int16 RelayPort;
        }
        
        public ConfigurationStruct configuration;

        private string[] cliError = new string[] {
            "Error: Invalid amount of command line arguments specified.",
            "Please run the application in the following manner:",
            "", // whitespace.
            "Examples:", // whitespace.
            "> dotnet run \t [PoolAddress:port] \t\t [Socks5Address:port] \t\t [RelayAddress:port]",
            "> dotnet run \t pool.supportxmr.com:3333 \t 127.0.0.1:9050 \t\t 127.0.0.1:3333",
            "> dotnet run \t pool.supportxmr.com:3333 \t 5.135.194.50:3020 \t\t 127.0.0.1:3333",
            "> dotnet run \t pool.supportxmr.com:3333 \t 127.0.0.1:9050 \t\t 10.0.0.2:3333",
            "> dotnet run \t pool.supportxmr.com:3333 \t 127.0.0.1:9050 \t\t 192.168.1.2:3333",
            "> dotnet run \t pool.supportxmr.com:3333 \t 127.0.0.1:9050 \t\t 0.0.0.0:3333",
            "" // whitespace for printing the next object.
        };

        public ConfigurationHandler()
        {
            configuration = new ConfigurationStruct();
            ParseCommandLineArguments();
        }

        public void ParseCommandLineArguments()
        {
            // Load the CLA (Command Line Arguments) into a string array.
            string[] args = Environment.GetCommandLineArgs();

            // Check the SA (string array)'s length for just the right amount.
            if (args.Length != 4) {

                // print that there was a command line argument error, about the number specified.
                foreach (string line in cliError) Console.WriteLine(line);

                // Exit the application.
                Environment.Exit(1);

            } else {

                // split the command line argument into segments
                string[] pool = args[1].Split(":");
                string[] proxy = args[2].Split(":");
                string[] relay = args[3].Split(":");

                // load pool data into struct.
                configuration.PoolAddress = pool[0];
                configuration.PoolPort = Int16.Parse(pool[1]);

                // load proxy data into struct.
                configuration.ProxyAddress = proxy[0];
                configuration.ProxyPort = Int16.Parse(proxy[1]);

                // load relay data into struct.
                configuration.RelayAddress = relay[0];
                configuration.RelayPort = Int16.Parse(relay[1]);

            }
        }
    }
}
