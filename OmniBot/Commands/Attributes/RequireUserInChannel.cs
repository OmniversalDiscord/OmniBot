using DSharpPlus.SlashCommands;
using OmniBot.Commands.Extensions;

namespace OmniBot.Commands.Attributes;

public class RequireUserInChannel : SlashCheckBaseAttribute
{
    public ulong ChannelId { get; }
    
    public RequireUserInChannel(ulong channelId)
    {
        ChannelId = channelId;
    }

    public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        var user = ctx.Member!;

        // If the user is an admin, they can do whatever they want!
        return user.IsCoffeeCrew() ? Task.FromResult(true) :
            // Otherwise, check if the user is in the channel
            Task.FromResult(user.VoiceState?.Channel?.Id == ChannelId);
    }
}