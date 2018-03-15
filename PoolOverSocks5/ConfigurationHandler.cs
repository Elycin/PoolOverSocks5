using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace PoolOverSocks5
{
    class ConfigurationHandler
    {
        public JObject loadedConfiguration;

        private readonly string[] REQUIRED_KEYS = { "Relay Address", "Proxy Address", "Pool Address", "Relay Port", "Proxy Port", "Pool Port", "Relay Concurrent Connections" };

        public ConfigurationHandler()
        {
            if (DoesConfigurationExist())
            {
                ReadConfiguration();
            }
            else
            {
                CreateNewConfiguration();
                WriteConfiguration();
            }
        }

        private void ReadConfiguration()
        {
            try
            {
                using (StreamReader file = File.OpenText(@GetConfigurationAbsolutePath()))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    loadedConfiguration = (JObject)JToken.ReadFrom(reader);
                }

                ValidateConfiguration();
               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read configuration from the disk.");
                Console.WriteLine("Exception: " + ex.ToString());
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        private void ValidateConfiguration()
        {
            foreach (string key in REQUIRED_KEYS)
            {
                try
                {
                     string testing = loadedConfiguration.GetValue(key).ToString();
                } catch (Exception e)
                {
                    Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, (new String('=', Console.BufferWidth - 1)));
                    Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "Your configuration is missing a key value pair.");
                    Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "Please check it and make sure that the key: '" + key + "' is not missing.");
                    Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "\nPress any key to exit.");
                    Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, (new String('=', Console.BufferWidth - 1)));
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
            Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "Configuration successfully loaded from disk.\n");
        }

        private void WriteConfiguration()
        {
            try
            {
                // Write the configuration to disk.
                File.WriteAllText(GetConfigurationAbsolutePath(), JsonConvert.SerializeObject(loadedConfiguration, Formatting.Indented));

                // Let the user know.
                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, (new String('=', Console.BufferWidth - 1)));
                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "A previous configuration has not been found, as a result a new one has been written for you.");
                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "Please change the settings and run 'dotnet run' again to start the relay.\n");
                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "The configuration can be found at");
                Program.ConsoleWriteLineWithColor(ConsoleColor.Green, GetConfigurationAbsolutePath());

                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, "\nPress any key to exit.");
                Program.ConsoleWriteLineWithColor(ConsoleColor.Yellow, (new String('=', Console.BufferWidth - 1)));
                Console.ReadLine();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to write configuration to disk.");
                Console.WriteLine("Exception: " + ex.ToString());
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        private void CreateNewConfiguration()
        {
            JObject newConfiguration = new JObject
            {
                // Pool Information
                { "Relay Address", "127.0.0.1" },
                { "Relay Port", 3333 },
                { "Relay Concurrent Connections", 16 },
                { "Proxy Address", "127.0.0.1" },
                { "Proxy Port", 9050 },
                { "Pool Address", "pool.supportxmr.com" },
                { "Pool Port", 3333 },
            };

            loadedConfiguration = newConfiguration;
        }

        public string GetRelayAddress() => loadedConfiguration.GetValue("Relay Address").ToString();

        public string GetProxyAddress() => loadedConfiguration.GetValue("Proxy Address").ToString();

        public string GetPoolAddress() => loadedConfiguration.GetValue("Pool Address").ToString();

        public Int16 GetRelayConcurrentConnections() => Int16.Parse(loadedConfiguration.GetValue("Relay Concurrent Connections").ToString());

        public int GetRelayPort() => int.Parse(loadedConfiguration.GetValue("Relay Port").ToString());

        public int GetProxyPort() => int.Parse(loadedConfiguration.GetValue("Proxy Port").ToString());

        public int GetPoolPort() => int.Parse(loadedConfiguration.GetValue("Pool Port").ToString());

        public string GetConfigurationVersion() => typeof(RuntimeEnvironment).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        private bool DoesConfigurationExist() => File.Exists(GetConfigurationAbsolutePath());

        public string GetConfigurationDirectory() => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public string GetConfigurationAbsolutePath() => Path.Combine(GetConfigurationDirectory(), "config.json");
    }
}
