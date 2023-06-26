using DSharpPlus;
using DSharpPlus.EventArgs;

namespace OmniBot.Commands;

// This only exists to make fun of the user for sending a slash command as a normal message
public static class InvalidSlashCommandHandler
{
    private static readonly Random Random = new();

    private static readonly string[] ReplyGifs =
    {
        "https://cdn.discordapp.com/attachments/342309270139830272/1122859045502595112/caption.gif",
        "https://cdn.discordapp.com/attachments/342309270139830272/1122859147608735834/caption.gif",
        "https://cdn.discordapp.com/attachments/342309270139830272/1122859273395908618/caption.gif",
        "https://cdn.discordapp.com/attachments/342309270139830272/1122859386671468684/caption.gif"
    };

    public static void Register(DiscordClient client)
    {
        client.MessageCreated += Handle;
    }

    private static async Task Handle(BaseDiscordClient client, MessageCreateEventArgs args)
    {
        if (args.Message.Content.StartsWith("/"))
        {
            string gif;

            lock (Random)
            {
                gif = ReplyGifs[Random.Next(ReplyGifs.Length)];
            }

            await args.Message.RespondAsync(gif);
        }
    }
}