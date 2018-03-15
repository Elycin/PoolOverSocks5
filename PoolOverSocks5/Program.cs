using System;
using System.IO;
using System.Threading;

namespace PoolOverSocks5
{
    class Program
    {
        /*
         * Message of the day
         * Just some simple text to display information about the project.
         */
        private static string[] motd = new string[] {
            "", //newline for spacing
            "PoolOverSocks5",
            "Project by Elycin <Ely Haughie>",
            "Source Code: https://github.com/elycin/pooloversocks5",
            "BTC: 1MwzVSXVfm1Gfvtc2n3vqam8434cGA5GgT",
            "" //newline for spacing
        };

        /*
         * Configuration Handler
         * Placeholder variable to hold the configuration handling class
         * The class will handle all passed command line arguments.
         */
        private static ConfigurationHandler configurationHandler;

        /*
         * Relay Handler
         * This is the actual class that will handle the networking magic and relay data along the socks5 proxy.
         */
        private static Server relayHandler;

        /*
         * Last Responder for Logger
         * This is to keep track of the last thing to print to the console
         */
        private static string lastResponder;

        /*
         * Main Method
         * The main function of where the appllication enters
         */
        static void Main(string[] args)
        {
            // Initialize a new ConfugrationHandler into the placeholder variable.
            configurationHandler = new ConfigurationHandler();

            // Print out the MOTD.
            foreach (string line in motd) Console.WriteLine(line);

            // Start the relay.
            relayHandler = new Server(configurationHandler);
            relayHandler.Work();
        }

        /*
         * Press Any Key To Exit function
         * Just a simple static function that can be called anywhere to signal that there was an error.
         */
        public static void PressAnyKeyToExit()
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            Environment.Exit(1);
        }

        public static void LogResponderHandler(string responder, string data)
        {
            if (lastResponder != responder)
            {
                // Update the last responder
                lastResponder = responder;

                // Write
                ConsoleWriteLineWithColor(ConsoleColor.Green, string.Format("\n{0} Response:", responder));
            }

            // Print the remaining data
            Console.WriteLine(data);
        }

        public static void ConsoleWriteLineWithColor(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
