

using System;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;

using static TemplateBot.Bot;

namespace TemplateBot.Commands
{
    class SlashCommands : ApplicationCommandsModule
    {
        [SlashCommand("ping", "Send's the actual ping")]
        public static async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("Loading Ping, could take time. Please wait."));
            await Task.Delay(2000);
            await ctx.Channel.SendMessageAsync($"Pong: `{Client.Ping}ms`");
        }

        [SlashCommand("shutdown", "Bot shutdown (restricted)"), ApplicationCommandRequireOwner]
        public static async Task Shutdown(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Shutdown request"));
            if (ctx.Client.CurrentApplication.Team.Members.Where(x => x.User == ctx.User).Any())
            {
                await Task.Delay(5000);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shutdown request accepted."));
                await Task.Delay(2000);
                ShutdownRequest.Cancel();
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shutting down!"));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not allowed to execute this request!"));
            }
        }

        [SlashCommand("about", "Informations about the Bot")]
        public static async Task About(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var bot = ctx.Client;
            var owner = (bot.CurrentApplication.Owners.Count() == 1) ? bot.CurrentApplication.Team.Owner.Username : bot.CurrentApplication.TeamName;
            var embed = new DiscordEmbedBuilder();
            embed.WithAuthor(bot.CurrentUser.UsernameWithDiscriminator, null, ctx.Client.CurrentUser.AvatarUrl)
                .WithTitle($"Informations about {bot.CurrentUser.Username}")
                .WithTitle($"The Owner of the Bot is {owner}")
                .AddField("Number of Guilds:", $"`{bot.Guilds.Count}`", true)
                .AddField("Number of Commands:", $"`{bot.GetApplicationCommands().RegisteredCommands.First().Value.Count}`", true)
                .AddField("The Dev(s):", $"{getDevs(bot.CurrentApplication)}")
                .AddField("Library:", $"The Bot was written in C# using the {Formatter.MaskedUrl(bot.BotLibrary, new Uri("https://github.com/Aiko-IT-Systems/DisCatSharp"))} Library. \n The Template fot this Bot can be found {Formatter.MaskedUrl("here", new Uri("https://github.com/Aiko-It-Systems/DisCatSharp.TemplateBot"))}.")
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.Azure);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        private static String getDevs(DiscordApplication bot)
        {
            String devs = null;
            foreach (var dev in bot.Team.Members)
            {
                devs = $"{devs}\n{dev.User.Mention}";
            };
            return devs;
        }
    }
}