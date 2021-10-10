using System;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using Microsoft.Extensions.Logging;

using static TemplateBot.Program;

namespace TemplateBot
{
    internal class Bot : IDisposable
    {
        public static DiscordClient Client { get; set; }
        public static ApplicationCommandsExtension ApplicationCommands;
        private InteractivityExtension INext;
        public static CancellationTokenSource ShutdownRequest;

        public Bot(string token)
        {
            ShutdownRequest = new CancellationTokenSource();

            LogLevel logLevel;
#if DEBUG
            logLevel = LogLevel.Debug;
#else
            logLevel = LogLevel.Error;
#endif

            var cfg = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = logLevel,
                Intents = DiscordIntents.AllUnprivileged,
                MessageCacheSize = 2048
            };

            Client = new DiscordClient(cfg);

            Client.UseApplicationCommands();

            ApplicationCommands = Client.GetApplicationCommands();

            INext = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                ButtonBehavior = ButtonPaginationBehavior.Disable
            });
        }

        public void Dispose()
        {
            Client.Dispose();
            INext = null;
            Client = null;
            ApplicationCommands = null;
            Environment.Exit(0);
        }

        public async Task RunAsync()
        {
            await Client.ConnectAsync();
            RegisterCommands(ApplicationCommands);
            while (!ShutdownRequest.IsCancellationRequested)
            {
                await Task.Delay(2000);
            }
            await Client.UpdateStatusAsync(activity: null, userStatus: UserStatus.Offline, idleSince: null);
            await Client.DisconnectAsync();
            await Task.Delay(2500);
            Dispose();
        }

        private void RegisterEventListener(DiscordClient client, ApplicationCommandsExtension ac)
        {

            /* Client Basic Events */
            client.SocketOpened += Client_SocketOpened;
            client.SocketClosed += Client_SocketClosed;
            client.SocketErrored += Client_SocketErrored;
            client.Heartbeated += Client_Heartbeated;
            client.Ready += Client_Ready;
            client.Resumed += Client_Resumed;

            /* Slash Infos */
            client.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
            client.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
            client.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;
            ac.SlashCommandErrored += Ac_SlashCommandErrored;
            ac.SlashCommandExecuted += Ac_SlashCommandExecuted;
            ac.ContextMenuErrored += Ac_ContextMenuErrored;
            ac.ContextMenuExecuted += Ac_ContextMenuExecuted;
        }

        private void RegisterCommands(ApplicationCommandsExtension ac)
        {
            //ac.RegisterCommands<Commands.SlashCommands>(); // global - SlashCommands.Main = Ordner.Class

            foreach (var id in Program.Guilds)
            {
                ac.RegisterCommands<Commands.SlashCommands>(id);
            }
        }
        private static Task Client_Ready(DiscordClient dcl, ReadyEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Starting {Client.CurrentUser.Username}");
            Console.WriteLine("Client ready!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Loading Commands...");
            Console.ForegroundColor = ConsoleColor.Magenta;
            var alist = dcl.GetApplicationCommands().RegisteredCommands;
            foreach (var command in alist)
            {
                Console.WriteLine(command.Key == null ? "Global Commands" : $"Guild Commands for {command.Key}:");
                foreach (var ac in command.Value)
                {
                    Console.WriteLine($"Command {ac.Name} ({ac.Id}) loaded with default permission {ac.DefaultPermission}");
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bot ready!");
            DiscordActivity activity = new() //TODO: Activity 
            {
                Name = "`/help`",
                ActivityType = ActivityType.Playing
            };
            Client.UpdateStatusAsync(activity: activity, userStatus: UserStatus.Online, idleSince: null);
            return Task.CompletedTask;
        }

        private static Task Client_Resumed(DiscordClient dcl, ReadyEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Bot resumed!");
            return Task.CompletedTask;
        }

        private static Task Discord_ApplicationCommandUpdated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command updated: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandDeleted(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command deleted: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandCreated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command created: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        public static Task Ac_SlashCommandExecuted(ApplicationCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            Console.WriteLine($"Slash/Info: {e.Context.CommandName}");
            return Task.CompletedTask;
        }

        public static Task Ac_SlashCommandErrored(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Console.WriteLine($"Slash/Error: {e.Exception.Message} | CN: {e.Context.CommandName} | IID: {e.Context.InteractionId}");
            return Task.CompletedTask;
        }

        public static Task Ac_ContextMenuExecuted(ApplicationCommandsExtension sender, ContextMenuExecutedEventArgs e)
        {
            Console.WriteLine($"Slash/Info: {e.Context.CommandName}");
            return Task.CompletedTask;
        }

        public static Task Ac_ContextMenuErrored(ApplicationCommandsExtension sender, ContextMenuErrorEventArgs e)
        {
            Console.WriteLine($"Slash/Error: {e.Exception.Message} | CN: {e.Context.CommandName} | IID: {e.Context.InteractionId}");
            return Task.CompletedTask;
        }

        private static Task Client_SocketOpened(DiscordClient dcl, SocketEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Socket opened");
            return Task.CompletedTask;
        }

        private static Task Client_SocketErrored(DiscordClient dcl, SocketErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Socket has an error! " + e.Exception.Message.ToString());
            return Task.CompletedTask;
        }

        private static Task Client_SocketClosed(DiscordClient dcl, SocketCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Socket closed: " + e.CloseMessage);
            return Task.CompletedTask;
        }

        private static Task Client_Heartbeated(DiscordClient dcl, HeartbeatEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Received Heartbeat:" + e.Ping);
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }
    }
}
