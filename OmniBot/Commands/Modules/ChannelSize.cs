using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using OmniBot.Commands.Attributes;

namespace OmniBot.Commands.Modules;

[PublicAPI]
[SlashCommandGroup("resize", "Resize a voice channel")]
public class ChannelSize : ApplicationCommandModule
{
    private const ulong LoungeId = 419355451277312000;
    private const ulong GamingId = 951241315067240448;
    private const IConfiguration Configuration = null!;

    private static async Task Resize(InteractionContext ctx, ulong channelId, long newSize)
    {
        var channel = ctx.Guild.GetChannel(channelId);

        // Check the size is in a valid range
        if (newSize is < 1 or > 99) throw new EphemeralCommandException("New channel size must be between 1 and 99");

        // Check the channel size is greater than the current number of users in the channel
        if (channel.Users.Count > newSize)
            throw new EphemeralCommandException(
                $"New channel size ({newSize}) is less than the number of users currently in the channel ({channel.Users.Count})");

        // Resize the channel
        await channel.ModifyAsync(edit => edit.Userlimit = (int)newSize);

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Channel resized")
            .WithDescription($"Resized {channel.Name} to {newSize} user{(newSize == 1 ? "" : "s")}")
            .WithColor(DiscordColor.Azure);

        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("lounge", "Resize the lounge channel")]
    [RequireUserInChannel(LoungeId)]
    public static Task ResizeTest(InteractionContext ctx, [Option("size", "New size of the channel")] long newSize)
    {
        return Resize(ctx, LoungeId, newSize);
    }

    [SlashCommand("gaming", "Resize the gaming channel")]
    [RequireUserInChannel(GamingId)]
    public static Task ResizeGaming(InteractionContext ctx, [Option("size", "New size of the channel")] long newSize)
    {
        return Resize(ctx, GamingId, newSize);
    }
}