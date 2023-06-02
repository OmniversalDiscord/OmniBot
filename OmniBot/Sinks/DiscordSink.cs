using DSharpPlus;
using DSharpPlus.Entities;
using Serilog.Core;
using Serilog.Events;

namespace OmniBot.Sinks;

public class DiscordSink : ILogEventSink
{
    private readonly DiscordClient _client;
    private readonly ulong _logChannelId;
    private readonly string _ping;
    private DiscordChannel? _logChannel;

    public DiscordSink(DiscordClient client, IConfiguration configuration)
    {
        _client = client;
        _logChannelId = ulong.Parse(configuration["Logging:Discord:Channel"]!);

        // Just confirming that the user ID is valid
        var userId = ulong.Parse(configuration["Logging:Discord:PingUser"]!);

        _ping = $"<@{userId}>";
    }

    public void Emit(LogEvent logEvent)
    {
        Task.Run(() => SendLog(logEvent));
    }

    private async Task SendLog(LogEvent logEvent)
    {
        if (_client.Guilds.Count == 0) return;

        // Discord logs min level is Warning
        if (logEvent.Level < LogEventLevel.Warning) return;

        if (_logChannel == null) _logChannel = await _client.GetChannelAsync(_logChannelId);

        var color = logEvent.Level switch
        {
            LogEventLevel.Warning => DiscordColor.Yellow,
            LogEventLevel.Error => DiscordColor.Red,
            LogEventLevel.Fatal => DiscordColor.Red,
            _ => DiscordColor.White
        };

        var message = logEvent.RenderMessage();
        var exception = logEvent.Exception;
        if (exception != null)
            message += $"\n```{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}```";

        var embed = new DiscordEmbedBuilder()
            .WithTitle(logEvent.Level.ToString())
            .WithDescription(message)
            .WithColor(color)
            .WithTimestamp(DateTimeOffset.Now);

        await _logChannel.SendMessageAsync(_ping, embed);
    }
}