using DSharpPlus;

namespace OmniBot;

internal sealed class OmniBotHost : IHostedService
{
    private readonly DiscordClient _discord;

    public OmniBotHost(DiscordClient discord, Commands.Commands _)
    {
        _discord = discord;
        _discord.Ready += (_, _) =>
        {
            var botUser = _discord.CurrentUser!;
            var username = $"{botUser.Username}#{botUser.Discriminator}";
            _discord.Logger.LogInformation("Connected as {Username}", username);
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync(CancellationToken token)
    {
        await _discord.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken token)
    {
        await _discord.DisconnectAsync();
    }
}