using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;

namespace OmniBot.Commands.Modules;

[PublicAPI]
public class Kill : ApplicationCommandModule
{
    [SlashCommand("kill", "Kills a user")]
    public static Task KillAsync(InteractionContext ctx, [Option("user", "The user to kill")] DiscordUser user)
    {
        return ctx.CreateResponseAsync(
            $"{ctx.Member.Mention} has been charged with the attempted murder of {user.Mention} and is awaiting sentencing.");
    }
}