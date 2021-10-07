

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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("Loading Ping, could take time. Please lay back <3"));
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
                ShutdownRequest.Cancel();
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shuting down!"));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not allowed to execute this request!"));
            }
        }
    }
}