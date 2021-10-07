using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TemplateBot
{
    class Program
    {
        public static Config Config { get; set; }
        public static ulong[] Guilds;

        // The Main Entrypoint.
        static void Main()
        {
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(@"config.json"));

            string token = Config.Token;

            List<ulong> guilds = new List<ulong>();
            foreach (var guild in Config.Guilds)
            {
                guilds.Add(guild);
            }

            Guilds = guilds.ToArray();
            // Fancy Logging Stuff
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Initializing {Assembly.GetExecutingAssembly().FullName.Split(",")[0]}");
            Console.ResetColor();
            Console.WriteLine(" ");

            var bot = new Bot(token);
            bot.RunAsync().Wait();

            Console.WriteLine(" ");
            Console.WriteLine($"Shutdown of {Assembly.GetExecutingAssembly().FullName.Split(",")[0]} successfull");
            Console.WriteLine(" ");
            Console.WriteLine($"Press any key to exit the aplication..");
            Console.ResetColor();
            Console.ReadKey(true);
            Environment.Exit(0);
        }
    }

    public class Config
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("botname")]
        public string Botname { get; set; }

        [JsonPropertyName("uidbot")]
        public ulong Uidbot { get; set; }

        [JsonPropertyName("guilds")]
        public ulong[] Guilds { get; set; }
    }
}
